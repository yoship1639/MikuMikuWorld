using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    class Transit<T>
    {
        public Transit(T value)
        {
            To = value;
            From = value;
        }

        public T To { get; protected set; }
        public T From { get; protected set; }
        public float Rate { get; private set; } = 1.0f;

        private double trans = 1.0;
        private double nowTrans = 0.0;

        public void Set(T value)
        {
            From = value;
            To = value;
        }

        public void Trans(T to, double transTime)
        {
            From = To;
            To = to;

            trans = transTime;
            nowTrans = 0.0;
            Rate = 0.0f;
        }

        public void Update(double deltaTime)
        {
            if (Rate < 1.0f)
            {
                nowTrans += deltaTime;
                Rate = MMWMath.Saturate((float)(nowTrans / trans));
            }
        }
    }

    class TransitColor : Transit<Color4>
    {
        public TransitColor(Color4 color) : base(color) { }

        public Color4 Now
        {
            get { return MMWMath.Lerp(From, To, Rate); }
        }
    }
}
