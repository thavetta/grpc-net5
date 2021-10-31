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

        private static void OutputResult(WeatherReply reply)
        {
            Console.WriteLine("Vysledek: ");
            Console.WriteLine("Teplota: " + reply.Aktual.Temperature);
            Console.WriteLine("Tlak: " + reply.Aktual.Pressure);

            switch (reply.Aktual.WindDirectionCase)
            {
                case WeatherInfo.WindDirectionOneofCase.Code:
                    Console.WriteLine("Smer vetru: " + reply.Aktual.Code);
                    break;
                case WeatherInfo.WindDirectionOneofCase.Degree:
                    Console.WriteLine("Smer vetru: " + reply.Aktual.Degree);
                    break;
            }

            Console.WriteLine("Status: " + reply.Aktual.Status);
            Console.WriteLine("Varovani:");
            foreach (var item in reply.Aktual.Warnning)
            {
                Console.WriteLine("- " + item);
            }
        }
    }

}
