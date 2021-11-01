using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab4Weather.Services
{
    public class AirportWeatherService : Lab4Weather.AirportWeather.AirportWeatherBase
    {
        private readonly ILogger<AirportWeatherService> _logger;
        private readonly IRandomInt _random;

        public AirportWeatherService(IRandomInt random, ILogger<AirportWeatherService> logger)
        {
            _logger = logger;
            _random = random;
            _logger.LogInformation("Konstruktor service objektu " + this.GetHashCode());
        }
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

        public async override Task<SumReply> Sum(IAsyncStreamReader<NumberRequest> requestStream, ServerCallContext context)
        {
            pocitadlo = 0;
            await foreach(var message in requestStream.ReadAllAsync())
            {
                _logger.LogInformation("Pridavam " + message.Number);
                pocitadlo += message.Number;
            }

            return new SumReply() { Number = pocitadlo };
        }

        private int pocitadlo;

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
    }
}
