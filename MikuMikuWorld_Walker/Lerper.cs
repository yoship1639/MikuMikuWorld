using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    class Lerper
    {
        public Lerper(float target)
        {
            Target = target;
            Now = target;
        }
        public float Target { get; set; }
        public float Now { get; set; }
        public float Speed { get; set; } = 10.0f;
        public void Update(double deltaTime)
        {
            Now = MMWMath.Lerp(Now, Target, (float)deltaTime * Speed);
        }
    }
}
