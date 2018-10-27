using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    public struct Line<T> where T : struct
    {
        public T from;
        public T to;

        public Line(T from, T to)
        {
            this.from = from;
            this.to = to;
        }
    }
}
