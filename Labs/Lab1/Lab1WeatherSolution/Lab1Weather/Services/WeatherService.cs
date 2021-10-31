using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab1Weather
{
    public class WeatherService : AirportWeather.AirportWeatherBase
    {
        private static HashSet<string> airports = new()
            { "BRQ", "BTS", "PRG", "KSC", "TAT", "OSR" };

        private readonly ILogger<WeatherService> _logger;
        public WeatherService(ILogger<WeatherService> logger)
        {
            _logger = logger;
            _logger.LogInformation("Vytvarim WeatherService");
        }

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
    }
}
