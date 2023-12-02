using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using Demo.Tokens.Common.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;

namespace Demo.Tokens.Web.Host;

public class Program
{
    public const string IdentityUrl = "https://localhost:5001";

    private const AuthClientType AuthenticationMode = AuthClientType.Implicit;

    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();

        ConfigureAuthentication(builder);

        builder.Services.Configure<AdminApiOptions>(builder.Configuration.GetSection("AdminApi"));
        builder.Services.Configure<UserApiOptions>(builder.Configuration.GetSection("UserApi"));

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddHttpClient("userApi", (sp, client) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();

            client.BaseAddress = config.GetValue<Uri>("UserApi:Host");
            var token = httpContextAccessor.HttpContext?.GetTokenAsync("access_token").GetAwaiter().GetResult();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        // added to enable authentication
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapRazorPages();

        app.Run();
    }

    private static void ConfigureAuthentication(WebApplicationBuilder builder)
    {
        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = "Cookies";
            options.DefaultChallengeScheme = "oidc";
        })
            .AddCookie("Cookies")
            .AddOpenIdConnect("oidc", options =>
            {
                options.Authority = IdentityUrl;

                switch (AuthenticationMode)
                {
                    case AuthClientType.AuthCode:
                        ConfigureAuthCode(options);
                        break;
                    case AuthClientType.Implicit:
                        ConfigureImplicit(options);
                        break;
                    case AuthClientType.ReferenceToken:
                        ConfigureCodeReference(options);
                        break;
                }

                options.SaveTokens = true;
            });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("User", policy =>
            {
                policy.RequireClaim("idp", "local", "auth0");
            });
            options.AddPolicy("Admin", policy =>
            {
                policy.RequireClaim("idp", "aad");
            });
        });
    }

    private static void ConfigureAuthCode(OpenIdConnectOptions options)
    {
        options.ClientId = "web-ui";
        options.ClientSecret = "1f668bf6-5ef5-4e77-ae84-28614dfc9d2d";
        options.ResponseType = "code";

        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.Scope.Add("api1");
        options.Scope.Add("offline_access");
        options.GetClaimsFromUserInfoEndpoint = true;
    }

    private static void ConfigureImplicit(OpenIdConnectOptions options)
    {
        options.ClientId = "web-implicit-ui";
        options.ResponseType = "token id_token";

        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.Scope.Add("api1");
        options.GetClaimsFromUserInfoEndpoint = true;
    }

    private static void ConfigureCodeReference(OpenIdConnectOptions options)
    {
        options.ClientId = "web-ref-ui";
        options.ResponseType = "code";

        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.Scope.Add("api1");
        options.GetClaimsFromUserInfoEndpoint = true;
    }
}

public enum AuthClientType
{
    AuthCode,
    Implicit,
    ReferenceToken,
}