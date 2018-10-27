using MikuMikuWorld;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exporter_MMW
{
    class CameraController : GameComponent
    {
        public float Distance { get; set; } = 2.0f;
        public Vector3 Target { get; set; }
        public float Rotate { get; set; }
        public float Height { get; set; }

        private float rotate;
        private float height;

        protected override void Update(double deltaTime)
        {
            if (Input.MouseWheel > 0) Distance *= 1.1f;
            if (Input.MouseWheel < 0) Distance *= 0.9f;
            MMW.MainCamera.Near = Distance * 0.01f;
            MMW.MainCamera.Far = Distance * 1000.0f;

            var delta = Vector2.Zero;
            if (Input.IsButtonDown(OpenTK.Input.MouseButton.Middle))
            {
                var d = Input.MouseDelta;
                if (d != Vector2.Zero)
                {
                    var up = Vector3.TransformVector(Vector3.UnitY, Matrix4.CreateRotationX(height));
                    up = Vector3.TransformVector(up, Matrix4.CreateRotationY(-rotate));
                    var zz = (MMW.MainCamera.Target - MMW.MainCamera.Transform.Position).Normalized();

                    Vector3 x, y;
                    y = up;
                    x = Vector3.Cross(y, zz);

                    y *= d.Y * Distance * 0.002f;
                    x *= d.X * Distance * 0.002f;

                    Target += y + x;
                }
            }
            else if (Input.IsButtonDown(OpenTK.Input.MouseButton.Right)) delta = Input.MouseDelta;

            Rotate += delta.X * 0.01f;
            Height += delta.Y * 0.01f;
            Height = MMWMath.Clamp(Height, -1.5f, 1.5f);

            rotate = MMWMath.Lerp(rotate, Rotate, (float)deltaTime * 30.0f);
            height = MMWMath.Lerp(height, Height, (float)deltaTime * 30.0f);

            Vector3 v;
            v.X = (float)(Math.Sin(rotate) * Math.Cos(height));
            v.Z = (float)(-Math.Cos(rotate) * Math.Cos(height));
            v.Y = (float)Math.Sin(height);

            v *= Distance;
            var dir = -v;
            v += Target;

            MMW.MainCamera.Transform.Position = v;
            MMW.MainCamera.Target = Target;

            MMW.DirectionalLight.Direction = dir;
        }
    }
}
