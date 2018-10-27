using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.GameComponents.Lights
{
    public class DirectionalLight : Light
    {
        public DirectionalLight() : this(-Vector3.UnitY) { }
        public DirectionalLight(Vector3 dir)
        {
            Direction = dir;

            getter.Add("Intensity", (obj) => Intensity);
            getter.Add("Color", (obj) => Color);
            getter.Add("Direction", (obj) => Direction);
            getter.Add("LocalDirection", (obj) => LocalDirection);
            getter.Add("WorldDirection", (obj) => WorldDirection);

            setter.Add("Intensity", (obj, value) => Intensity = (float)value);
            setter.Add("Color", (obj, value) => Color = (Color4)value);
            setter.Add("Direction", (obj, value) => Direction = (Vector3)value);
        }

        public Vector3 Direction { get; set; } = Vector3.UnitY * -1.0f;

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
            return new DirectionalLight()
            {
                Intensity = Intensity,
                Color = Color,
                Direction = Direction,
                Radius = Radius,
                ClipBounds = ClipBounds,
            };
        }
    }
}
