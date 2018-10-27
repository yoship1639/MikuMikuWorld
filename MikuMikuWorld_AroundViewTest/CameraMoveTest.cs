using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    class CameraMoveTest : GameComponent
    {
        public override bool ComponentDupulication
        {
            get
            {
                return false;
            }
        }

        private float amb = 0.5f;
        private float intensity = 7.0f;

        private Vector3 targetPos;

        protected override void OnLoad()
        {
            base.OnLoad();

            targetPos = GameObject.Transform.Position;
        }

        protected override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            var front = GameObject.Transform.WorldDirectionZ;
            var left = GameObject.Transform.WorldDirectionX;

            if (Input.IsKeyDown(OpenTK.Input.Key.W)) targetPos += front * (float)deltaTime * 10.0f;
            if (Input.IsKeyDown(OpenTK.Input.Key.S)) targetPos -= front * (float)deltaTime * 10.0f;
            if (Input.IsKeyDown(OpenTK.Input.Key.A)) targetPos += left * (float)deltaTime * 10.0f;
            if (Input.IsKeyDown(OpenTK.Input.Key.D)) targetPos -= left * (float)deltaTime * 10.0f;

            MMW.MainCamera.GameObject.Transform.Position = Vector3.Lerp(MMW.MainCamera.GameObject.Transform.Position, targetPos, 0.1f);

            if (Input.IsKeyDown(OpenTK.Input.Key.Z)) amb = MathHelper.Clamp(amb - (float)deltaTime, 0.0f, 100.0f);
            if (Input.IsKeyDown(OpenTK.Input.Key.X)) amb = MathHelper.Clamp(amb + (float)deltaTime, 0.0f, 100.0f);
            if (Input.IsKeyDown(OpenTK.Input.Key.C)) intensity = MathHelper.Clamp(intensity - (float)deltaTime, 0.0f, 100.0f);
            if (Input.IsKeyDown(OpenTK.Input.Key.V)) intensity = MathHelper.Clamp(intensity + (float)deltaTime, 0.0f, 100.0f);

            MMW.GlobalAmbient = new Color4(amb, amb, amb, 0.0f);
            MMW.DirectionalLight.Intensity = intensity;
        }

        public override GameComponent Clone()
        {
            throw new NotImplementedException();
        }
    }
}
