using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;

namespace Lab4Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Ready for test");
            Console.ReadKey();

            var reply = await CallClient("BRQ");

            OutputResult(reply);

            reply = await CallClient("BRQ",true);

            OutputResult(reply);
            Console.WriteLine("Predpoved:");
            OutputResult(reply, 1);
            OutputResult(reply, 2);
            OutputResult(reply, 3);
            Console.WriteLine("**********************");

            await ServerStream();

            await Sumuj();
            

            _channel.Dispose();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private async static Task ServerStream()
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(60));

            using var call = _client.GetWeatherStream(new AirportRequest() { AirportCode = "BTS" },
                cancellationToken: cts.Token);

            try
            {
                await foreach(var message in call.ResponseStream.ReadAllAsync())
                {
                    Console.WriteLine("Info v case: " + DateTime.Now.TimeOfDay);
                    Console.WriteLine("Teplota " + message.Temperature);
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                Console.WriteLine("Stream ukoncen");
            }
        }

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

        private static GrpcChannel _channel;
        private static AirportWeather.AirportWeatherClient _client;

        private static AsyncUnaryCall<WeatherReply> CallClient(string code, bool useOld = false)
        {
            if (!(useOld && _channel != null))
            {
                _channel = GrpcChannel.ForAddress("https://localhost:5001");
            }

            if (!(useOld && _client != null))
            {
                _client = new AirportWeather.AirportWeatherClient(_channel);
            }
            
            var reply = _client.GetWeatherAsync(
                new AirportRequest { AirportCode = code });
            return reply;
        }

        private static void OutputResult(WeatherReply reply, int index = 0)
        {
            WeatherInfo weather = index == 0 ? reply.Actual : reply.Forecast[index];
            Console.WriteLine("Vysledek: ");
            Console.WriteLine("Teplota: " + weather.Temperature);
            Console.WriteLine("Tlak: " + weather.Pressure);

            switch (weather.WindDirectionCase)
            {
                case WeatherInfo.WindDirectionOneofCase.Code:
                    Console.WriteLine("Smer vetru: " + weather.Code);
                    break;
                case WeatherInfo.WindDirectionOneofCase.Degree:
                    Console.WriteLine("Smer vetru: " + weather.Degree);
                    break;
            }

            Console.WriteLine("Status: " + weather.Status);
            Console.WriteLine("Varovani:");
            foreach (var item in weather.Warnning)
            {
                Console.WriteLine("- " + item);
            }
        }
    }

}
