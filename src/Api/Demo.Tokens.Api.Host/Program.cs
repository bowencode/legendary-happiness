using Demo.Tokens.Common.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace Demo.Tokens.Api.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var auth0Options = builder.Configuration.GetSection("Auth0");
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.Authority = "https://localhost:5001";
                    options.Audience = "https://localhost:5001/resources";
                    options.MapInboundClaims = false;
                    options.RequireHttpsMetadata = true;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        RequireAudience = true,
                    };

                    options.ForwardDefaultSelector = SchemeSelector.ForwardReferenceToken("reference");
                })
                .AddJwtBearer("Auth0Bearer", options =>
                {
                    var authority = auth0Options.GetValue<string>("Authority");
                    var audience = auth0Options.GetValue<string>("Audience");
                    options.Authority = authority;
                    options.Audience = audience;
                })
                .AddOAuth2Introspection("reference", options =>
                {
                    options.Authority = "https://localhost:5001";
                    options.ClientId = "api-introspection";
                    options.ClientSecret = "98337efc-8193-45dc-ad3d-4cbdf7501f30";
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

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: "ClientSideSPA",
                    policy =>
                    {
                        policy.WithOrigins("https://localhost:5173");
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
    }
}