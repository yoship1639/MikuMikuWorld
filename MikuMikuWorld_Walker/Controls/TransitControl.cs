using MikuMikuWorld.Controls;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    class TransitControl : Control
    {
        public enum TransitType
        {
            Linear,
            Lerp,
            SmoothStep,
        }

        public Vector2 Target { get; set; }
        public float Speed { get; set; } = 10.0f;
        public TransitType Type { get; set; } = TransitType.Lerp;

        public void Update(double deltaTime)
        {
            if (deltaTime > 0.1) return;
            if (Type == TransitType.Lerp)
            {
                var rate = MMWMath.Saturate((float)deltaTime * Speed);
                LocalLocation = Vector2.Lerp(LocalLocation, Target, rate);
            }
        }
    }
}
