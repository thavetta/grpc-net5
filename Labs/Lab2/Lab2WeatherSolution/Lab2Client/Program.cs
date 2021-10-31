using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;

namespace Lab2Client
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


            reply = await CallClient("BRQ", true);

            OutputResult(reply);
            reply = await CallClient("BRQ", true);

            OutputResult(reply);

            _channel.Dispose();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
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
                _client = new Lab2Client.AirportWeather.AirportWeatherClient(_channel);
            }
            
            var reply = _client.GetWeatherAsync(
                new Lab2Client.AirportRequest { AirportCode = code });
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
