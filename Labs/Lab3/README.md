# Lab 3

## Možnosti konfigurace

* Vytvořte pomocnou třídu generující náhodná čísla, která ale bude používat společný logovací systém
* Použijte ji v naší aplikaci pro plnění hodnot počasí
* Nastavte maximální velikost správ a ověřte chování při překročení limitu
* Logujte informace o činnosti gRPC služby do souboru

## Postup

1. Zkopírujte výslednou solution z LAB2 do nového umístění, přejmenujte Solution na Lab3WeatherSolution, projekty na Lab3xxxxxx
1. Opravte v proto file namespace na Lab3Weather
1. Přidejte do projektu interface IRandomInt

        public interface IRandomInt
        {
            int Next(int max);
            int Next(int min, int max);
        }
1. Přidejte do projektu třídu implementující daný inteface

        public class RandomIntLite : IRandomInt
        {
            private ILogger<RandomIntLite> _logger;
            public RandomIntLite(ILogger<RandomIntLite> logger)
            {
                _logger = logger;
                _logger.LogInformation("Konstruktor RandomIntLite");
            }

            private static readonly Random generator = new Random();
            public int Next(int max)
            {
                _logger.LogInformation($"Generuji 0 az {max}");
                return generator.Next(max + 1);
            }

            public int Next(int min, int max)
            {
                _logger.LogInformation($"Generuji {min} az {max}");
                return generator.Next(min, max + 1);
            }
        }
1. Upravte AirportWeatherService, aby podporoval i předání třídy implementující IRandomInt do konstruktoru

        public class AirportWeatherService : Lab3Weather.AirportWeather.AirportWeatherBase
        {
            private readonly ILogger<AirportWeatherService> _logger;
            private readonly IRandomInt _random;

            public AirportWeatherService(IRandomInt random, ILogger<AirportWeatherService> logger)
            {
                _logger = logger;
                _random = random;
                _logger.LogInformation("Konstruktor service objektu " + this.GetHashCode());
            }
1. Upravte metodu GetWeatherInfom aby používala nový typ pro generování náhodných čísel

        private WeatherInfo GetWeatherInfo()
        {
            var weather = new WeatherInfo()
            {
                Pressure = _random.Next(990, 1025),
                Temperature = _random.Next(0, 30),
                WindSpeed = _random.Next(0, 15),
                Code = (WindDirection) _random.Next(0,8),
                Status = (WeatherStatus) _random.Next(0,5)
            };
            weather.Warnning.Add("First info " + DateTime.Now.Ticks);
            weather.Warnning.Add("Second info " + +DateTime.Now.Ticks);
            return weather;
        }
1. Upravte Program.cs, aby zaregistroval do DI i nově vytvořenou třídu pro generování náhodných čísel.

        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddScoped<IRandomInt, RandomIntLite>();
        builder.Services.AddGrpc().AddServiceOptions<Lab3Weather.Services.AirportWeatherService>(options =>
        {
            options.MaxReceiveMessageSize = 1024;
            options.MaxSendMessageSize = 1024; //nastavení velikosti na 100 skonèí výjimkou
            options.EnableDetailedErrors = true;
        });
        var app = builder.Build();

        app.MapGrpcService<Lab3Weather.Services.AirportWeatherService>();
        app.MapGet("/",() => "Bez klienta gRPC smula...");

        app.Run();
1. Otestujte funkčnost aplikace

## Serilog

1. Standardní logger v ASP.NET neumí logovat do souboru. Jednou z možností je využít Serilog.
1. přidejte do hlavního projektu reference na

* Serilog.AspNetCore

1. Doplňte do Program.cs za vytvořením builderu příkaz na načtení konfigurace pro Serilog z konfiguračního souboru

        builder.Host.UseSerilog((context,services,configuration) => configuration.ReadFrom.Configuration(context.Configuration));
1. Přidejte do konfiguračního souboru sekci nastavující výstup logu systému Serilog. Nastavte výstup na Console a do souboru. Appsettings.json by pak mohl vypadat takto

        {
          "Serilog": {
            "MinimumLevel": "Information",
            "Override": {
            "Microsoft.AspNetCore": "Warning"
            },
            "WriteTo": [
                {
                    "Name": "Console"
                },
                {
                    "Name": "File",
                    "Args": {
                        "path": "Serilogs\\applog.txt"
                    }
                }
            ]
          },
          "AllowedHosts": "*",
          "Kestrel": {
            "EndpointDefaults": {
                "Protocols": "Http2"
            }
          }
        }
1. Vytvořte složku Serilogs v projektu
1. V klientovi můžete upravit v proto souboru namespace a opravit namespace v Program.cs
1. Otestujte funkčnost řešení včetně logování
1. Otestujte chování aplikace když extrémně zmenšíte povolenou velikost message (pod 100 bytů)
