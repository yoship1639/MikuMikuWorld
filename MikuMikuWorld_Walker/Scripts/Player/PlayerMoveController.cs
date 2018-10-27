using MikuMikuWorld.GameComponents;
using MikuMikuWorld.Physics;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AL = OpenTK.Audio.OpenAL.AL;

namespace MikuMikuWorld.Scripts.Player
{
    class PlayerMoveController : GameComponent
    {
        public string CameraType { get; set; } = "third person";

        private UserData userData;
        RigidBody rb;

        public Vector3 Velocity { get; set; }

        protected override void OnLoad()
        {
            rb = GameObject.GetComponent<RigidBody>();
            userData = MMW.GetAsset<UserData>();
        }

        protected override void Update(double deltaTime)
        {
            var cam = MMW.MainCamera;
            var d = cam.Transform.WorldPosition - cam.Target;
            d.Y = 0.0f;
            d.Normalize();

            var dirZ = d;

            var dirX = dirZ * Matrix3.CreateRotationY(MathHelper.PiOver2);

            var deltaDir = Vector3.Zero;
            if (Input.IsKeyDown(Key.W)) deltaDir -= dirZ;
            if (Input.IsKeyDown(Key.S)) deltaDir += dirZ;
            if (Input.IsKeyDown(Key.A)) deltaDir -= dirX;
            if (Input.IsKeyDown(Key.D)) deltaDir += dirX;

            if (deltaDir != Vector3.Zero)
            {
                deltaDir.Normalize();
                var move = deltaDir * (float)deltaTime * 6.0f;
                Velocity += move;
                var length = Velocity.Length;
                if (length > 2.5f) length = 2.5f;
                Velocity = Velocity.Normalized() * length;
                Transform.Position += Velocity * (float)deltaTime;
                //rb.ApplyForce(deltaDir * (float)deltaTime * 12.5f * rb.Mass * 50.0f);
                userData.TotalMoveDistance += (Velocity * (float)deltaTime).Length;

                if (CameraType == "third person")
                {
                    var dot = -(Vector3.Dot(Transform.WorldDirectionZ, deltaDir) - 1.0f) * 0.5f * MathHelper.Pi;
                    var cross = Vector3.Cross(Transform.WorldDirectionZ, deltaDir);

                    var r = MMWMath.Clamp(dot, 0.0f, (float)deltaTime * 8.0f);

                    if (cross.Y > 0.0f) Transform.Rotate.Y -= r;
                    else Transform.Rotate.Y += r;
                }
            }
            else
            {
                var length = Velocity.Length;
                length -= (float)deltaTime * 5.0f;
                if (length <= 0.0f) Velocity = Vector3.Zero;
                else Velocity = Velocity.Normalized() * length;
                Transform.Position += Velocity * (float)deltaTime;
            }

            if (Input.IsKeyPressed(Key.Space))
            {
                var wp = Transform.WorldPosition;
                var rays = Bullet.RayTest(wp + Vector3.UnitY * 0.1f, wp - Vector3.UnitY * 0.2f, GameObject);
                if (rays.Count > 0)
                {
                    rb.ApplyImpulse(Vector3.UnitY * 4.5f * rb.Mass);
                    userData.TotalJumpCount++;
                } 
            } 

            Transform.UpdatePhysicalTransform();
        }

        protected override void OnReceivedMessage(string message, params object[] args)
        {
            if (message == "camera change")
            {
                CameraType = (string)args[0];
            }
            else if (message == "enable player controller")
            {
                Enabled = true;
            }
            else if (message == "disable player controller")
            {
                Enabled = false;
            }
            else if (message == "show dialog")
            {
                Enabled = false;
            }
            else if (message == "close dialog")
            {
                Enabled = true;
            }
        }
    }
}
