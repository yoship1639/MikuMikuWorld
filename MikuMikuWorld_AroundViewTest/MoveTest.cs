using MikuMikuWorld.Assets;
using MikuMikuWorld.GameComponents;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    class MoveTest : GameComponent
    {
        public override bool ComponentDupulication { get { return true; } }

        

        protected override void OnLoad()
        {
            base.OnLoad();

        }


        protected override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            //if (Input.IsKeyDown(OpenTK.Input.Key.T)) MMW.DirectionalLight.GameObject.Transform.Rotate.Z += (float)deltaTime;
            //if (Input.IsKeyDown(OpenTK.Input.Key.G)) MMW.DirectionalLight.GameObject.Transform.Rotate.Z -= (float)deltaTime;
            //if (Input.IsKeyDown(OpenTK.Input.Key.F)) MMW.DirectionalLight.GameObject.Transform.Rotate.X -= (float)deltaTime;
            //if (Input.IsKeyDown(OpenTK.Input.Key.H)) MMW.DirectionalLight.GameObject.Transform.Rotate.X += (float)deltaTime;

            if (Input.IsKeyDown(OpenTK.Input.Key.Right)) GameObject.Transform.Rotate.Y += (float)deltaTime;
            if (Input.IsKeyDown(OpenTK.Input.Key.Left)) GameObject.Transform.Rotate.Y -= (float)deltaTime;

            
        }

        public override GameComponent Clone()
        {
            throw new NotImplementedException();
        }
    }
}
