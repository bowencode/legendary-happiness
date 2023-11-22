using Demo.Tokens.Common.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace Demo.Tokens.Web.AdminApi.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

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
                });

            builder.Services.AddSingleton<IAuthorizationHandler, ScopeAuthorizationHandler>();
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("ReadUsernames", policy =>
                {
                    policy.AddRequirements(new ScopeRequirement("read:username"));
                });
                options.AddPolicy("ReadUsers", policy =>
                {
                    policy.RequireAssertion(context =>
                    {
                        if (context.User.HasClaim("idp", "aad"))
                            return true;
                        
                        if (context.User.HasClaim(c => c.Type == "scope" && c.Value.Split(' ').Contains("read:users")))
                            return true;

                        return false;
                    });
                });
                options.AddPolicy("ReadUserDetails", policy =>
                {
                    policy.RequireClaim("idp", "aad");
                    policy.AddRequirements(new ScopeRequirement("read:user-details"));
                });
            });

            builder.Services.AddControllers();
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

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }


    }
}