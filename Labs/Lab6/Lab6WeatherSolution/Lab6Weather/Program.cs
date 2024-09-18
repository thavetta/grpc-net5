using Lab6Weather.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Lab6Weather
{
    public class Program
    {
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

            var app = builder.Build();

            app.MapGrpcService<AirportWeatherService>();
            app.MapGet("/", () => "Bez klienta gRPC smula...");

            app.Run();
        }

        
    }
}
