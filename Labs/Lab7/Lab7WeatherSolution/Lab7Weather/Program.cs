using Lab7Weather.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace Lab7Weather
{
    public class Program
    {
        private static readonly JwtSecurityTokenHandler JwtTokenHandler = new JwtSecurityTokenHandler();
        private static readonly SymmetricSecurityKey SecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this is my custom Secret key for authentication"));

        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog((context, services, configuration) => configuration.ReadFrom.Configuration(context.Configuration));
            builder.Services.AddSingleton<IRandomInt, RandomIntLite>();

            //Odpoznámkováním se i AirportWeatherService stane singletonem
            //builder.Services.AddSingleton<AirportWeatherService>();

            builder.Services.AddGrpc(options =>
            {
                options.MaxReceiveMessageSize = 1024;
                options.MaxSendMessageSize = 1024; //nastavení velikosti na 100 skonèí výjimkou
                options.EnableDetailedErrors = true;
                options.Interceptors.Add<ServerLoggerInterceptor>();
            });
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
                {
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireClaim(ClaimTypes.Name);
                });
            });
            builder.Services.AddAuthentication()
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateAudience = false,
                            ValidateIssuer = false,
                            ValidateActor = false,
                            ValidateLifetime = true,
                            IssuerSigningKey = SecurityKey
                        };
                });
            var app = builder.Build();

            app.MapGrpcService<AirportWeatherService>();
            app.MapGet("/generateJwtToken", context =>
            {
                return context.Response.WriteAsync(GenerateJwtToken(context.Request.Query["name"]!));
            });
            app.MapGet("/", () => "Bez klienta gRPC smula...");

            app.Run();
        }

        static string GenerateJwtToken(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new InvalidOperationException("Name is not specified.");
            }

            var claims = new[] { new Claim(ClaimTypes.Name, name) };
            var credentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken("ExampleServer", "ExampleClients", claims, expires: DateTime.Now.AddSeconds(60), signingCredentials: credentials);
            return JwtTokenHandler.WriteToken(token);
        }

    }
}
