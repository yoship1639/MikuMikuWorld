using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    class TimeTrigger
    {
        public double FirstSpan { get; set; } = 0.4;
        public double Span { get; set; } = 0.1;

        private double time1;
        private double time2;

        private double sleep = 0.0;

        public void Sleep(double time)
        {
            sleep = time;
        }

        public bool Trigger(double deltaTime, bool continuation)
        {
            if (sleep > 0.0)
            {
                sleep -= deltaTime;
                if (sleep > 0.0) return false;
            }
            if (!continuation)
            {
                time1 = 0.0;
                time2 = 0.0;
                return false;
            }
            if (time1 == 0.0 && time2 == 0.0)
            {
                time1 += deltaTime;
                return true;
            }
            else if (time1 < FirstSpan)
            {
                time1 += deltaTime;
                if (time1 >= FirstSpan) return true;
            }
            else if (time2 < Span)
            {
                time2 += deltaTime;
                if (time2 >= Span)
                {
                    time2 -= Span;
                    return true;
                }
            }
            return false;
        }
    }
}
