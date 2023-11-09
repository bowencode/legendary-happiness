using System.Net.Http.Headers;
using Demo.Notes.Common.Configuration;
using IdentityModel.AspNetCore.AccessTokenManagement;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Options;
using Yarp.ReverseProxy.Transforms;

namespace Demo.Notes.Web.Blazor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            AddIdentityAuthentication(builder);
            AddConfiguredReverseProxy(builder);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseBff();
            app.UseAuthorization();

            app.MapBffManagementEndpoints();

            app.MapReverseProxy();
            app.MapRazorPages();
            app.MapControllers();
            app.MapFallbackToFile("index.html");

            app.Run();
        }

        private static void AddIdentityAuthentication(WebApplicationBuilder builder)
        {
            var identityOptions = builder.Configuration.GetSection("Identity").Get<IdentityServerOptions>();

            var apiOptions = builder.Configuration.GetSection("ApiOptions").Get<IdentityServerOptions>();

            builder.Services.AddBff(opt =>
            {
                opt.AccessTokenManagementConfigureAction = (options) =>
                {
                    options.Client.Clients.Add("proxy", new ClientCredentialsTokenRequest
                    {
                        Address = $"{apiOptions.Authority.TrimEnd('/')}/connect/token",
                        ClientId = apiOptions.ClientId,
                        ClientSecret = apiOptions.ClientSecret,
                        Scope = apiOptions.GetScope(),
                    });
                };
            });

            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "cookie";
                    options.DefaultChallengeScheme = "oidc";
                    options.DefaultSignOutScheme = "oidc";
                })
                .AddCookie("cookie", options =>
                {
                    options.Cookie.Name = "__Host-blazor";
                    options.Cookie.SameSite = SameSiteMode.Strict;
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.Authority = identityOptions.Authority;

                    options.ClientId = identityOptions.ClientId;
                    options.ClientSecret = identityOptions.ClientSecret;
                    options.ResponseType = "code";
                    options.ResponseMode = "query";

                    options.Scope.Clear();
                    foreach (var scope in identityOptions.Scopes)
                    {
                        options.Scope.Add(scope);
                    }

                    options.MapInboundClaims = false;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SaveTokens = true;
                });
        }

        private static void AddConfiguredReverseProxy(WebApplicationBuilder builder)
        {
            var proxyBuilder = builder.Services.AddReverseProxy()
                .AddTransforms(builderContext =>
                {
                    var cluster = builderContext.Cluster?.ClusterId;
                    if (cluster == "userApi")
                    {
                        // Conditionally add a transform for routes that require auth.
                        builderContext.AddRequestTransform(async transformContext =>
                        {
                            var provider = builderContext.Services.GetRequiredService<IClientAccessTokenManagementService>();
                            var token = await provider.GetClientAccessTokenAsync("proxy");
                            transformContext.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        });
                    }
                    else if (cluster == "adminApi")
                    {
                        // Conditionally add a transform for routes that require auth.
                        builderContext.AddRequestTransform(async transformContext =>
                        {
                            // include user token in request
                            var token = await transformContext.HttpContext.GetUserAccessTokenAsync();
                            transformContext.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        });
                    }
                });
            proxyBuilder.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
        }
    }
}