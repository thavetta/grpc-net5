using Lab4Weather.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Lab4Weather
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog((context, services, configuration) => configuration.ReadFrom.Configuration(context.Configuration));
            builder.Services.AddSingleton<IRandomInt, RandomIntLite>();

            //Odpozn�mkov�n�m se i AirportWeatherService stane singletonem
            //builder.Services.AddSingleton<AirportWeatherService>();

            builder.Services.AddGrpc(options =>
            {
                options.MaxReceiveMessageSize = 1024;
                options.MaxSendMessageSize = 1024; //nastaven� velikosti na 100 skon�� v�jimkou
                options.EnableDetailedErrors = true;
            });

            var app = builder.Build();

            app.MapGrpcService<Lab4Weather.Services.AirportWeatherService>();
            app.MapGet("/", () => "Bez klienta gRPC smula...");

            app.Run();
        }

        
    }
}
