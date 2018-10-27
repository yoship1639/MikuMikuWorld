using MikuMikuWorld.GameComponents;
using MikuMikuWorld.Scripts.Character;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Scripts.Player
{
    class PlayerCameraController : GameComponent
    {
        private float rot = MathHelper.Pi;
        private float targetRot = MathHelper.Pi;
        private float height = 0.1f;
        private float targetHeight = 0.1f;
        private float fpHeight;
        private Vector3 target;
        private Vector3 nowDir;
        private CharacterInfo pi;
        private Transform ct;

        public string CameraType { get; set; } = "third person";
        public Transform Target { get; set; }
        public float Distance { get; set; } = 1.55f;
        public float NoiseIntensity { get; set; } = 0.1f;
        public float NoiseSpeed { get; set; } = 1.0f;


        protected override void OnLoad()
        {
            pi = GameObject.GetComponent<CharacterInfo>();
            MMW.MainCamera.ForceTarget = true;
            ct = MMW.FindGameObject(o => o.Name == "camera target").Transform;
        }

        protected override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            var cam = MMW.MainCamera;
            var prevPos = cam.Transform.Position;
            if (CameraType == "third person")
            {
                GameObject.GetComponent<MeshRenderer>().Visible = true;
                MMW.MainCamera.FoV = 1.2f;
                var center = Target != null ? Target.WorldPosition : Vector3.Zero;

                targetRot -= Input.MouseDelta.X * 0.005f;
                targetHeight += Input.MouseDelta.Y * 0.005f;
                targetHeight = MMWMath.Clamp(targetHeight, -MathHelper.PiOver6, MathHelper.PiOver2 - 0.3f);

                rot = MMWMath.Lerp(rot, targetRot, (float)deltaTime * 16.0f);
                height = MMWMath.Lerp(height, targetHeight, (float)deltaTime * 16.0f);
                Vector3.Lerp(ref target, ref center, (float)deltaTime * 6.0f, out target);

                var y = Math.Sin(height);
                var x = Math.Cos(height) * Math.Sin(rot);
                var z = Math.Cos(height) * Math.Cos(rot);

                var dx = (float)Math.Sin(MMW.TotalElapsedTime * 0.5);
                var dy = (float)Math.Cos(MMW.TotalElapsedTime * 0.7);
                var dz = (float)Math.Cos(MMW.TotalElapsedTime * 0.3);

                var pos = new Vector3((float)x, (float)y, (float)z) * Distance;
                pos += target;

                var rays = Physics.Bullet.RayTest(target, Vector3.Lerp(target, pos, 1.1f), GameObject);
                if (rays.Count > 0)
                {
                    var l = rays.Min(r => r.Rate);
                    var ray = rays.Find(r => r.Rate == l);
                    if (ray != null)
                    {
                        pos = Vector3.Lerp(target, ray.Position, 0.9f);
                    }
                }

                var nx = Noise.Fbm((float)MMW.TotalElapsedTime * 0.15f * NoiseSpeed, 3) * 0.01f;
                var ny = Noise.Fbm((float)MMW.TotalElapsedTime * 0.1f * NoiseSpeed, 4) * 0.01f;
                var nz = Noise.Fbm((float)MMW.TotalElapsedTime * 0.05f * NoiseSpeed, 5) * 0.01f;
                var rt = new Vector3(nx, ny, nz) * NoiseIntensity * 7.0f;
                var rp = new Vector3(ny, nz, nx) * NoiseIntensity * 4.0f;

                cam.Up = Matrix3.CreateRotationZ(nx * NoiseIntensity * 2.0f) * Vector3.UnitY;
                cam.Target = target + rt;
                cam.Transform.Position = pos + rp;
            }
            else if (CameraType == "first person")
            {
                GameObject.GetComponent<MeshRenderer>().Visible = false;
                MMW.MainCamera.FoV = 1.4f;
                Transform.Rotate.Y += Input.MouseDelta.X * 0.005f;

                var t = new Vector4(pi.Character.EyePosition, 1.0f) * Transform.WorldTransform;
                cam.Transform.Position = t.Xyz;

                fpHeight -= Input.MouseDelta.Y * 0.005f;
                fpHeight = MMWMath.Clamp(fpHeight, -MathHelper.PiOver2 + 0.1f, MathHelper.PiOver2 - 0.1f);
                var dir = ct.WorldDirectionZ;
                dir *= (float)Math.Cos(fpHeight);
                dir.Y += (float)Math.Sin(fpHeight);

                var target = dir + t.Xyz;
                Vector3.Lerp(ref nowDir, ref target, (float)deltaTime * 30.0f, out nowDir);
                cam.Target = nowDir;
            }
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
