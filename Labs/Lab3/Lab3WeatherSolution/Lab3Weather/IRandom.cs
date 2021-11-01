using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab3Weather
{
    public interface IRandomInt
    {
        int Next(int max);
        int Next(int min, int max);
    }
}
