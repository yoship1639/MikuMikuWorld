using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    static class Tables
    {
        public static long NextRankExp(long lv)
        {
            var total = 0;
            var prev = 0;
            for (var i = 0; i < lv; i++)
            {
                var v = prev + 12;
                total += v + (i * 3);
                prev = v;
            }

            return total;
        }
    }
}
