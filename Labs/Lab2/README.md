# Lab 2

## Složitější použití Protocol Buffer

* Vytvořte gRPC službu pro předání dat o aktuálním počasí včetně výhledu na další 3 hodiny (řešte pomocí Dictionary<hodina, predpoved>)
* Předpověď i aktuální stav budou obsahovat (teplota, tlak, rychlost a směr větru a stav)
* Pro stav použijte enumerátor s hodnotami
  * Slunečno, Zamračené, Mlha, Déšť, Sněžení
* Směr větru je dán číslem (0 – 359) nebo výčtem (N, NE, E, SE, S, SW, W, NW)
* Data o počasí můžou obsahovat libovolné množství varování
* Otestujte funkčnost

## Postup

1. Ve Visual Studiu vytvořte nový projekt typu **ASP.NET Core gRPC Service**.
1. Název projektu zvolte **Lab2Weather**, název Solution zvolte **Lab2WeatherSolution**.
1. Zvolte nejvyšší verzi .NET, kterou chcete aby aplikace podporovala.
1. Použití **Top-level statement** zvolte podle své priority.
1. V projektu ve složce Proto je uložen soubor *.proto* který definuje gRPC komunikaci. Zrušte soubor **greet.proto** a vytvořte vlastní **weather.v2.proto**.
1. Přidejte do souboru základní definice pro typ syntaxe souboru, jaký namespace se má v C# typech použít a jak se má jmenovat základ služeb.

        syntax = "proto3";

        option csharp_namespace = "Lab2Weather";

        package weather.v2;
1. Přidejte sekce, kde zadefinujete enumerátory pro směr větru a stav počasí. Název musí odpovídatpravidlům pro tvorbu enumerátorů, aby Proto compiler korektně převedl typ do C# kódu.

        enum WindDirection {
           WIND_DIRECTION_NONE = 0;
           WIND_DIRECTION_N = 1;
           WIND_DIRECTION_NE = 2;
           WIND_DIRECTION_E = 3;
           WIND_DIRECTION_SE = 4;
           WIND_DIRECTION_S = 5;
           WIND_DIRECTION_SW = 6;
           WIND_DIRECTION_W = 7;
           WIND_DIRECTION_NW = 8;
           WIND_DIRECTION_VAR = 9;
        }

        enum WeatherStatus {
           WEATHER_STATUS_NONE = 0;
           WEATHER_STATUS_CLEAR = 1;
           WEATHER_STATUS_CLOUDS = 2;
           WEATHER_STATUS_RAIN = 3;
           WEATHER_STATUS_SNOW = 4;
           WEATHER_STATUS_MIST = 5;
        }
1. Přidejte definici samotné služby

        // The weather service definition.
        service AirportWeather {
            // Sends a request for airport
            rpc GetWeather (AirportRequest) returns (WeatherReply);
        }
1. Přidejte definici informace o konkrétním stavu počasí. Zde je vidět použití variant typů některých dat.

        message WeatherInfo {
            int32 temperature = 1;
            int32 pressure = 2;
            oneof wind_direction {
                int32 degree = 3;
                WindDirection code = 4;
            }
            int32 wind_speed = 5;
            WeatherStatus status = 6;
            repeated string warnning = 7;
        }
1. A přidejte popis pro request a response data

        // The request message containing the airport code.
        message AirportRequest {
            string airport_code = 1;
        }

        // The response message containing the weather data.
        message WeatherReply {
            WeatherInfo actual = 1;
            map<int32,WeatherInfo> forecast = 2;
        }
1. Tím je definice .proto souboru hotová a můžete přejít na implementaci samotné služby.
1. Zrušte soubor GreetingService.cs ve složce Service a založte nový s názvem AirportWeatherService.
1. Odvoďte třídu z Lab2Weather.AirportWeather.AirportWeatherBase
1. přidejte kód pro DI předání logger instance

        public class AirportWeatherService : Lab2Weather.AirportWeather.AirportWeatherBase
        {
            private readonly ILogger<AirportWeatherService> _logger;
            public AirportWeatherService(ILogger<AirportWeatherService> logger)
            {
                _logger = logger;
                _logger.LogInformation("Konstruktor service objektu " + this.GetHashCode());
            }
        }
1. Vytvořte pomocné metody, které pomocí generátoru náhodných čísel vygenerují data pro stav počasí. Například:

        private static Random generator = new Random();

        private static WeatherInfo GetWeatherInfo()
        {
            var weather = new WeatherInfo()
            {
                Pressure = generator.Next(990, 1025),
                Temperature = generator.Next(0, 30),
                WindSpeed = generator.Next(0, 15),
                Code = (WindDirection) generator.Next(0,8),
                Status = (WeatherStatus) generator.Next(0,5)
            };
            weather.Warnning.Add("First info " + DateTime.Now.Ticks);
            weather.Warnning.Add("Second info " + +DateTime.Now.Ticks);
            return weather;
        }
1. A na závěr přidejte metodu která bude řešit požadavek ze sítě.

        public override Task<WeatherReply> GetWeather(AirportRequest request, ServerCallContext context)
        {
            _logger.LogInformation("resim GetWeather " + this.GetHashCode());
            WeatherInfo aktual = GetWeatherInfo();

            var forecast = new Dictionary<int, WeatherInfo>();
            forecast[1] = GetWeatherInfo();
            forecast[2] = GetWeatherInfo();
            forecast[3] = GetWeatherInfo();

            var reply = new WeatherReply();
            reply.Actual = aktual;
            reply.Forecast.Add(forecast);

            return Task.FromResult(reply);
        }
1. Nakonec opravte soubor Program.cs, kde nahraďte *GreeterService* textem *AirportWeatherService*.
1. Otestujte nastavení maximální velikosti message pro request i response

        builder.Services.AddGrpc().AddServiceOptions<AirportWeatherService>(options =>
        {
            options.MaxSendMessageSize = 2 * 1024 * 1024;
            options.MaxReceiveMessageSize = 5 * 1024 * 1024;
        });
1. Zkuste Build a Run pro ověření že vše je funkční a bez chyb.

## Přidání klienta

1. Přidejte do Solution nový projekt typu **Console Application**. Název zvolte **Lab2Client**.
1. Přidejte reference na nuget package

* Google.Protobuf
* Grpc.Net.Client
* Grpc.Tools

1. Zkopírujte soubor weather.v2.proto z serverového projektu do projektu klienta (do stejné složky kde je program.cs).
1. Změňte v textu proto souboru namespace na **Lab2Client**
1. Nastavte souboru weather.v2.proto vlastnosti **Build Action** na **Protobuf compiler** a dále **gRPC Stub Classes** na **Client Only**.
1. V program.cs doplňte using pro namespace Grpc.Net.Client.
1. V metodě Main otestujte zavolání služby pro některé letiště a vypište, co se vrátilo.
