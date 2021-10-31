using Grpc.Net.Client;
using System;
using System.Threading.Tasks;

namespace Lab1Klient
{
    class Program
    {
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
    }
}
