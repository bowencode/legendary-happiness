using Demo.Tokens.Common.Configuration;
using Demo.Tokens.Common.Extensions;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Demo.Tokens.Api.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            AddAuthenticatedApiAccess(builder.Services, builder.Configuration);

            var auth0Options = builder.Configuration.GetSection("Auth0");
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opt =>
                {
                    opt.Authority = "https://localhost:5001";
                    opt.Audience = "https://localhost:5001/resources";
                    opt.MapInboundClaims = false;
                    opt.RequireHttpsMetadata = true;
                    opt.SaveToken = true;
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        RequireAudience = true,
                    };
                })
                .AddJwtBearer("Auth0Bearer", options =>
                {
                    var authority = auth0Options.GetValue<string>("Authority");
                    var audience = auth0Options.GetValue<string>("Audience");
                    options.Authority = authority;
                    options.Audience = audience;
                });

            builder.Services.AddSingleton<IAuthorizationHandler, ScopeAuthorizationHandler>();
            builder.Services.AddSingleton<IAuthorizationHandler, UserAuthorizationHandler>();
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("ReceivedMessages", policy =>
                {
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    policy.AddRequirements(new ScopeRequirement("api1"));
                });
                options.AddPolicy("SentMessages", policy =>
                {
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    policy.AddRequirements(
                        new ScopeRequirement("api1"),
                        new UserRequirement("userId"));
                });
                options.AddPolicy("AllMessages", policy =>
                {
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAssertion(ctx =>
                    {
                        if (ctx.User.HasClaim(c => c is { Type: "idp", Value: "aad" }))
                        {
                            return true;
                        }
                        return ctx.User.HasClaim(c => c.Type == "scope" && c.Value.Split(' ').Contains("api2"));
                    });
                });

                var scope = auth0Options.GetValue<string>("RequiredScope");
                options.AddPolicy("ReadCalendar", policy =>
                {
                    policy.AddAuthenticationSchemes("Auth0Bearer");
                    policy.RequireAssertion(ctx =>
                    {
                        return ctx.User.HasClaim(c => c.Type == "scope" && c.Value.Split(' ').Contains(scope));
                    });
                });

                options.AddPolicy("ReadUserDetails", policy =>
                {
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireClaim("idp", "aad");
                });
            });

            builder.Services.Configure<CosmosOptions>(builder.Configuration.GetSection("Cosmos"));

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: "ClientSideSPA",
                    policy =>
                    {
                        policy.WithOrigins("https://localhost:3000");
                        policy.WithHeaders(HeaderNames.Authorization);
                    });
            });

            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.WriteIndented = true;
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        private static void AddAuthenticatedApiAccess(IServiceCollection services, IConfiguration configuration)
        {
            var apiOptions = configuration.GetSection("AdminApi").Get<AdminApiOptions>();

            services.AddAccessTokenManagement(options =>
            {
                options.Client.Clients.Add("tokenService", new ClientCredentialsTokenRequest
                {
                    Address = $"{apiOptions.Identity.Authority.TrimEnd('/')}/connect/token",
                    ClientId = apiOptions.Identity.ClientId,
                    ClientSecret = apiOptions.Identity.ClientSecret,
                    Scope = string.Join(" ", apiOptions.Identity.Scopes),
                });
            });

            services.AddClientAccessTokenHttpClient("adminApiClient", "tokenService", configureClient: client =>
            {
                client.BaseAddress = new Uri(apiOptions.Host);
            });
        }
    }
}