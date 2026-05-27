# Lab 4

## gRPC stream

* Vytvořte novou gRPC službu, která
* Bude mít metodu, která od klienta dostane sérii čísel a vrátí jejich součet
* Další metoda dostane kód letiště a bude každých 10 vteřin posílat aktuální údaje o počasí

* EXTRA:
* Pokud máte čas, přidejte metodu, která vytvoří echo server (čeká na string od klienta a beze změny je pošle klientovi zpět)

## Postup

1. Zkopírujte výslednou solution z LAB3 do nového umístění, přejmenujte Solution na Lab4WeatherSolution, projekty na Lab4xxxxxx
1. Opravte v proto file namespace na Lab4Weather
1. Přepište definici služby a metod tak, aby odpovídali zadání. Například takto:

        ```proto
        service AirportWeather {
  
          rpc GetWeather (AirportRequest) returns (WeatherReply);

          rpc GetWeatherStream (AirportRequest) returns (stream WeatherInfo);
          //Pouze pro demo streamu od klienta
          rpc Sum (stream NumberRequest) returns (SumReply);
        }

        message NumberRequest {
            int32 number = 1;
        }

        message SumReply {
            int32 number = 1;
        }
        ```

1. Doplňte do třídy AirportWeatherService metodu pro vyřízení pravidelného posílání informace o počasí

    ```csharp
    public override async Task GetWeatherStream(AirportRequest request, IServerStreamWriter<WeatherInfo> responseStream, ServerCallContext context)
    {
        while (!context.CancellationToken.IsCancellationRequested)
        {
            var info = GetWeatherInfo();
            _logger.LogInformation("Posilam nove pocasi");

            await responseStream.WriteAsync(info);
            await Task.Delay(10000);
        }
    }
    ```

1. A do třídy service přidejte metodu a pomocný field pro řešení součtu čísel, které postupně pošle klient

    ```csharp
    public async override Task<SumReply> Sum(IAsyncStreamReader<NumberRequest> requestStream, ServerCallContext context)
    {
        pocitadlo = 0;
        await foreach(var message in requestStream.ReadAllAsync(context.CancellationToken))
        {
            _logger.LogInformation("Pridavam " + message.Number);
            pocitadlo += message.Number;
        }
        return new SumReply() { Number = pocitadlo };
    }

    private int pocitadlo;
    ```

1. V klientské aplikaci přeneste nově upravený proto soubor
1. Doplňte metodu které bude čekat stream dat ohledně počasí, ale tak, že po 1 minutě odběr ukončí.

        ```csharp
        private async static Task ServerStream()
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(60));

            using var call = _client.GetWeatherStream(new AirportRequest() { AirportCode = "BTS" },
                cancellationToken: cts.Token);

            try
            {
                await foreach(var message in call.ResponseStream.ReadAllAsync(cts.Token))
                {
                    Console.WriteLine("Info v case: " + DateTime.Now.TimeOfDay);
                    Console.WriteLine("Teplota " + message.Temperature);
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                Console.WriteLine("Stream ukoncen");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Stream ukoncen klientem");
            }
        }

        ```

1. Jako další přidejte metodu, která postupně pošle čísla a po posledním čísle převezme výslednou sumu ze serveru.

        ```csharp
        private static async Task Sumuj()
        {
            Random random = new Random();
            using var call = _client.Sum();
            for (int i = 0; i < 10; i++)
            {
                int number = random.Next(100);
                Console.WriteLine("Posilam " + number);
                await call.RequestStream.WriteAsync(new NumberRequest() { Number = number });
                await Task.Delay(1500);

            }
            await call.RequestStream.CompleteAsync();

            var response = await call;
            Console.WriteLine("Soucet je " + response.Number);

        }

        ```

1. Zavolejte v klientovi přidané metody a otestujte oba stream režimy.
