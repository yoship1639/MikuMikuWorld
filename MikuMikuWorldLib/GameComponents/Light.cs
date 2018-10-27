using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.GameComponents
{
    public abstract class Light : GameComponent
    {
        public float Intensity { get; set; } = 1.0f;
        public Color4 Color { get; set; } = Color4.White;
        public float Radius { get; set; } = 10.0f;
        public float SpecularCoeff { get; set; } = 1.0f;
        public Bounds ClipBounds { get; set; } = new Bounds(Vector3.Zero, Vector3.One * 1000.0f);
    }
}
