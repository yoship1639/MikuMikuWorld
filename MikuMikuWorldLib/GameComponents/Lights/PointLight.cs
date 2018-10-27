using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.GameComponents.Lights
{
    public class PointLight : Light
    {
        public PointLight()
        {
            getter.Add("Intensity", (obj) => Intensity);
            getter.Add("Color", (obj) => Color);
            getter.Add("Radius", (obj) => Radius);
            getter.Add("SpecularCoeff", obj => SpecularCoeff);
            getter.Add("ClipBoundsCenter", (obj) => ClipBounds.Center);
            getter.Add("ClipBoundsExtents", (obj) => ClipBounds.Extents);

            setter.Add("Intensity", (obj, value) => Intensity = (float)value);
            setter.Add("Color", (obj, value) => Color = (Color4)value);
            setter.Add("Radius", (obj, value) => Radius = (float)value);
            setter.Add("SpecularCoeff", (obj, value) => SpecularCoeff = (float)value);
            setter.Add("ClipBoundsCenter", (obj, value) => ClipBounds = new Bounds((Vector3)value, ClipBounds.Extents));
            setter.Add("ClipBoundsExtents", (obj, value) => ClipBounds = new Bounds(ClipBounds.Center, (Vector3)value));
        }

        public override GameComponent Clone()
        {
            return new PointLight()
            {
                Intensity = Intensity,
                Color = Color,
                Radius = Radius,
                SpecularCoeff = SpecularCoeff,
                ClipBounds = ClipBounds,
            };
        }
    }
}
