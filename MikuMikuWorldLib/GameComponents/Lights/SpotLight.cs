using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.GameComponents.Lights
{
    public class SpotLight : Light
    {
        public SpotLight() : this(-Vector3.UnitY, 0.866f, 0.7071f) { }
        public SpotLight(Vector3 dir, float innerDot, float outerDot)
        {
            Direction = dir;
            InnerDot = innerDot;
            OuterDot = outerDot;

            getter.Add("Intensity", (obj) => Intensity);
            getter.Add("Color", (obj) => Color);
            getter.Add("Radius", (obj) => Radius);
            getter.Add("ClipBoundsCenter", (obj) => ClipBounds.Center);
            getter.Add("ClipBoundsExtents", (obj) => ClipBounds.Extents);
            getter.Add("Direction", (obj) => Direction);
            getter.Add("LocalDirection", (obj) => LocalDirection);
            getter.Add("WorldDirection", (obj) => WorldDirection);
            getter.Add("InnerDot", (obj) => InnerDot);
            getter.Add("OuterDot", (obj) => OuterDot);

            setter.Add("Intensity", (obj, value) => Intensity = (float)value);
            setter.Add("Color", (obj, value) => Color = (Color4)value);
            setter.Add("Radius", (obj, value) => Radius = (float)value);
            setter.Add("ClipBoundsCenter", (obj, value) => ClipBounds = new Bounds((Vector3)value, ClipBounds.Extents));
            setter.Add("ClipBoundsExtents", (obj, value) => ClipBounds = new Bounds(ClipBounds.Center, (Vector3)value));
            setter.Add("Direction", (obj, value) => Direction = (Vector3)value);
            setter.Add("InnerDot", (obj, value) => InnerDot = (float)value);
            setter.Add("OuterDot", (obj, value) => OuterDot = (float)value);
        }

        public Vector3 Direction { get; set; } = Vector3.UnitY * -1.0f;
        public float InnerDot { get; set; } = 0.866f;
        public float OuterDot { get; set; } = 0.7071f;

        public Vector3 LocalDirection
        {
            get
            {
                return Vector3.TransformVector(Direction, MatrixHelper.CreateRotate(GameObject.Transform.Rotate));
            }
        }
        public Vector3 WorldDirection
        {
            get
            {
                return Vector3.TransformVector(Direction, GameObject.Transform.WorldTransform);
            }
        }

        public override GameComponent Clone()
        {
            return new SpotLight()
            {
                Intensity = Intensity,
                Color = Color,
                Radius = Radius,
                ClipBounds = ClipBounds,
                Direction = Direction,
                InnerDot = InnerDot,
                OuterDot = OuterDot,
            };
        }
    }
}
