using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Demo.Partner.ExternalApi.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var section = builder.Configuration.GetSection("Auth");
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    var authority = section.GetValue<string>("Authority");
                    var audience = section.GetValue<string>("Audience");
                    options.Authority = authority;
                    options.Audience = audience;
                });
            builder.Services.AddAuthorization(options =>
            {
                var scope = section.GetValue<string>("RequiredScope");
                options.AddPolicy("ReadCalendar", policy =>
                {
                    policy.RequireAssertion(ctx =>
                    {
                        return ctx.User.HasClaim(c => c.Type == "scope" && c.Value.Split(' ').Contains(scope));
                    });
                });
            });

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