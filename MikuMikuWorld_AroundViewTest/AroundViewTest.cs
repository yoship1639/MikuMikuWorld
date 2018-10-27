using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    class AroundViewTest : GameComponent
    {
        public override bool ComponentDupulication
        {
            get
            {
                return false;
            }
        }

        float targetY;
        float targetX;

        protected override void Update(double deltaTime)
        {
            //targetY -= Input.MouseDelta.X * (float)deltaTime * 0.05f;
            //targetX += Input.MouseDelta.Y * (float)deltaTime * 0.05f;
            //targetY = MathHelper.Clamp(targetY, -0.1f, 0.1f);
            //targetX = MathHelper.Clamp(targetX, -0.1f, 0.1f);
            targetY -= Input.MouseDelta.X * (float)deltaTime * 0.5f;
            targetX += Input.MouseDelta.Y * (float)deltaTime * 0.5f;
            //targetY = MathHelper.Clamp(targetY, -2f, 2f);
            //targetX = MathHelper.Clamp(targetX, -2f, 2f);

            if (Input.IsKeyDown(OpenTK.Input.Key.Space)) MMW.MainCamera.FoV = MathHelper.PiOver6 * 0.5f;
            else MMW.MainCamera.FoV = MathHelper.PiOver3;

            GameObject.Transform.Rotate.Y = MMWMath.Lerp(GameObject.Transform.Rotate.Y, targetY, 0.1f);
            GameObject.Transform.Rotate.X = MMWMath.Lerp(GameObject.Transform.Rotate.X, targetX, 0.1f);
        }

        public override GameComponent Clone()
        {
            throw new NotImplementedException();
        }
    }
}
