using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab3Weather
{
    public class RandomIntLite : IRandomInt
    {
        private ILogger<RandomIntLite> _logger;
        public RandomIntLite(ILogger<RandomIntLite> logger)
        {
            _logger = logger;
            _logger.LogInformation("Konstruktor RandomIntLite");
        }

        private static readonly Random generator = new Random();
        public int Next(int max)
        {
            _logger.LogInformation($"Generuji 0 az {max}");
            return generator.Next(max + 1);
        }

        public int Next(int min, int max)
        {
            _logger.LogInformation($"Generuji {min} az {max}");
            return generator.Next(min, max + 1);
        }
    }
}
