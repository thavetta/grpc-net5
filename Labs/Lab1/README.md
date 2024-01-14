# Lab 1

## Základní ukázka tvorby služby a klienta

* Vytvořte gRPC službu, která vrátí informaci o počasí podle kódu letiště.
* Vrátíte tyto informace
  * Teplota; Rychlost větru; Směr větru; Tlak
  * Textový popis upozornění
* Podporovaná letiště – PRG, BRQ, OSR, BTS, KSC, TAT
* Vytvořte klienta a otestujte komunikaci
* Použijte aktuální knihovnu grpc.AspNetCore

## Postup

1. Ve Visual Studiu vytvořte nový projekt typu **ASP.NET Core gRPC Service**.
1. Název projektu zvolte **Lab1Weather**, název Solution zvolte **Lab1WeatherSolution**.
1. Zvolte nejvyšší verzi .NET, kterou chcete aby aplikace podporovala.
1. Použití **Top-level statement** zvolte podle své priority.
1. V projektu ve složce Proto je uložen soubor *.proto* který definuje gRPC komunikaci. Zrušte soubor **greet.proto** a vytvořte vlastní **weather.v1.proto**.
1. Přidejte do souboru základní definice pro typ syntaxe souboru, jaký namespace se má v C# typech použít a jak se má jmenovat základ služeb.

        syntax = "proto3";

        option csharp_namespace = "Lab1Weather";

        package weather.v1;

1. Dál přidejte definici samotné RPC metody, kterou bude služba podporovat.
        // The weather service definition.
        service AirportWeather {
  
        rpc GetWeather (WeatherRequest) returns (WeatherInfo);
        }
1. Přidejte definici typu **WeatherRequest** a **WeatherInfo** dle zadání.
        // The request message
        message WeatherRequest {
            string airport_code = 1;
        }

        // The response message
        message WeatherInfo {
        
            int32 temperature = 1;
            int32 pressure = 2;
            int32 wind_degree = 3;
            int32 wind_speed = 4;
            string warnning = 5;
        }
1. Ve vlastnostech (Properties Window) souboru weather.v1.proto nastavte **Build Action** na **Protobuf compiler** a dále **gRPC Stub Classes** na **Server Only**. Tím je určeno jaké třídy mají automaticky vzniknou dle proto souboru.
1. Ve složce Service smažte soubor **GreeterService.cs** a vytvořte nový **WeatherService.cs**.
1. Zadefinujte, že třída **WeatherService** je zděděná z **AirportWeather.AirportWeatherBase**.

        public class WeatherService : AirportWeather.AirportWeatherBase
1. Přidejte using pro namespace Grpc.Core
1. Přidejte HashSet definující seznam povolených letišť

        private static HashSet<string> airports = new()
            { "BRQ", "BTS", "PRG", "KSC", "TAT", "OSR" };
1. Přidejte do třídy členský prvek pro loggování a konstruktor podporující DI

        private readonly ILogger<WeatherService> _logger;
        public WeatherService(ILogger<WeatherService> logger)
        {
            _logger = logger;
            _logger.LogInformation("Vytvarim WeatherService");
        }
1. A nakonec pomocí override přidejte kód pro samotnou RPC metodu

        public override  Task<WeatherInfo> GetWeather(WeatherRequest request, ServerCallContext context)
        {
            string airport = request.AirportCode;
            _logger.LogInformation("Resim GetWeather pro " + airport);

            if (!airports.Contains(airport))
            {
                throw new ArgumentOutOfRangeException("Neplatny kod letiste");
            }

            return Task.FromResult(new WeatherInfo
            {
                Temperature = 25,
                Pressure = 1010,
                WindDegree = 245,
                WindSpeed = 10,
                Warnning = "Text warnning"
            });
        }
1. Nakonec opravte soubor Program.cs, kde nahraďte *GreeterService* textem *WeatherService*. Nic víc není potřeba měnit.
1. Zkuste Build a Run pro ověření že vše je funkční a bez chyb.

## Přidání klienta

1. Přidejte do Solution nový projekt typu **Console Application**. Název zvolte **Lab1Klient**.
1. Přidejte reference na nuget package

* Google.Protobuf
* Grpc.Net.Client
* Grpc.Tools

1. Zkopírujte soubor weather.v1.proto z serverového projektu do projektu klienta (do stejné složky kde je program.cs).
1. Nastavte souboru weather.v1.proto vlastnosti **Build Action** na **Protobuf compiler** a dále **gRPC Stub Classes** na **Client Only**.
1. V program.cs doplňte using pro namespace Grpc.Net.Client.
1. V metodě Main otestujte zavolání služby pro některé letiště a vypište, co se vrátilo.

       static async Task Main(string[] args)
        {
            Console.WriteLine("Ready for test:");
            Console.ReadKey();
            Console.WriteLine("Startuji akci");

            var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Lab1Weather.AirportWeather.AirportWeatherClient(channel);
            var result = await client.GetWeatherAsync(
                new Lab1Weather.WeatherRequest() { AirportCode = "BRQ" });
            Console.WriteLine("Vysledek:");
            Console.WriteLine("Teplota: " + result.Temperature);

            Console.ReadKey();
        }
1. Nastavte ať se v solution spouští oba projekty a otestujte funkčnost.
