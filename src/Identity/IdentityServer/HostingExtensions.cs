using Duende.IdentityServer;
using IdentityServer.Configuration;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Serilog;
using System.Net;

namespace IdentityServer;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages();

        var isBuilder = builder.Services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                options.EmitStaticAudienceClaim = true;
            })
            .AddTestUsers(TestUsers.Users);

        // in-memory, code config
        isBuilder.AddInMemoryIdentityResources(Config.IdentityResources);
        isBuilder.AddInMemoryApiScopes(Config.ApiScopes);
        isBuilder.AddInMemoryClients(Config.Clients);


        // if you want to use server-side sessions: https://blog.duendesoftware.com/posts/20220406_session_management/
        // then enable it
        //isBuilder.AddServerSideSessions();
        //
        // and put some authorization on the admin/management pages
        //builder.Services.AddAuthorization(options =>
        //       options.AddPolicy("admin",
        //           policy => policy.RequireClaim("sub", "1"))
        //   );
        //builder.Services.Configure<RazorPagesOptions>(options =>
        //    options.Conventions.AuthorizeFolder("/ServerSideSessions", "admin"));

        builder.Services.AddAuthentication()
            .AddGoogle(options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                // register your IdentityServer with Google at https://console.developers.google.com
                // enable the Google+ API
                // set the redirect URI to https://localhost:5001/signin-google
                options.ClientId = "copy client ID from Google here";
                options.ClientSecret = "copy client secret from Google here";
            })
            .AddOpenIdConnect("aad", "Azure Active Directory", options =>
            {
                var aad = builder.Configuration.GetSection("AAD").Get<AadOptions>();

                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                options.ClientId = aad.ClientId;
                options.Authority = $"https://login.microsoftonline.com/{aad.TenantId}";
                options.RequireHttpsMetadata = true;
                options.ClientSecret = aad.ClientSecret;
                options.CallbackPath = "/signin-oidc";

                options.Events.OnRemoteFailure = context =>
                {
                    context.HandleResponse();

                    if (context.Failure is OpenIdConnectProtocolException && context.Failure.Message.Contains("access_denied"))
                    {
                        context.Response.Redirect("/");
                    }
                    else
                    {
                        context.Response.Redirect("/Home/Error?message=" + WebUtility.UrlEncode(context.Failure?.Message));
                    }

                    return Task.CompletedTask;
                };
            })
            .AddOpenIdConnect("auth0", "Auth0", options =>
            {
                var auth0 = builder.Configuration.GetSection("Auth0").Get<Auth0Options>();

                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                options.ClientId = auth0.ClientId;
                options.Authority = auth0.Authority;
                options.RequireHttpsMetadata = true;
                options.ClientSecret = auth0.ClientSecret;
                options.CallbackPath = "/signin-oidc-auth0";
                options.Events = new OpenIdConnectEvents
                {
                    OnRedirectToIdentityProvider = context =>
                    {
                        context.ProtocolMessage.SetParameter("audience", auth0.Audience);
                        return Task.CompletedTask;
                    },
                };
                options.ResponseType = "code id_token";
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.Scope.Add("read:calendar");

            });

        return builder.Build();
    }
    
    public static WebApplication ConfigurePipeline(this WebApplication app)
    { 
        app.UseSerilogRequestLogging();
    
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseIdentityServer();
        app.UseAuthorization();
        
        app.MapRazorPages()
            .RequireAuthorization();

        return app;
    }
}
