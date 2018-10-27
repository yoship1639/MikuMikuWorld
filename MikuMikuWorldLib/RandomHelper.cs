using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    public static class RandomHelper
    {
        private static Random rand = new Random();

        public static int NextInt(int min = 0, int max = int.MaxValue)
        {
            return rand.Next(min, max);
        }

        public static float NextFloat()
        {
            return (float)rand.NextDouble();
        }

        public static double NextDouble()
        {
            return rand.NextDouble();
        }
    }
}
