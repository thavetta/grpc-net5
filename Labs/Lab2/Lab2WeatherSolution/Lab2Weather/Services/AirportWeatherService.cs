using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab2Weather.Services
{
    public class AirportWeatherService : Lab2Weather.AirportWeather.AirportWeatherBase
    {
        private readonly ILogger<AirportWeatherService> _logger;
        public AirportWeatherService(ILogger<AirportWeatherService> logger)
        {
            _logger = logger;
            _logger.LogInformation("Konstruktor service objektu " + this.GetHashCode());
        }
        public override Task<WeatherReply> GetWeather(AirportRequest request, ServerCallContext context)
        {
            _logger.LogInformation("resim GetWeather " + this.GetHashCode());
            var aktual = new WeatherInfo()
            {
                Pressure = 1020,
                Temperature = 21,
                WindSpeed = 5,
                Code = WindDirection.E,
                Status = WeatherStatus.Clear
            };
            aktual.Warnning.Add("First info");
            aktual.Warnning.Add("Second info");

            var forecast = new Dictionary<int, WeatherInfo>();
            forecast[1] = aktual;
            forecast[2] = aktual;
            forecast[3] = aktual;

            var reply = new WeatherReply();
            reply.Aktual = aktual;
            reply.Forecast.Add(forecast);

            return Task.FromResult(reply);
        }
    }
}
