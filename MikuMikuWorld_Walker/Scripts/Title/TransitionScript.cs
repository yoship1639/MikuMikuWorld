using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikuMikuWorld.GameComponents;
using MikuMikuWorld.Assets;
using MikuMikuWorld.Properties;
using System.Drawing;
using OpenTK.Graphics;
using OpenTK.Input;

namespace MikuMikuWorld.Scripts
{
    class TransitionScript : DrawableGameComponent
    {
        public bool AcceptInput { get; set; } = true;

        protected override void OnLoad()
        {
            base.OnLoad();
        }

        protected override void Update(double deltaTime)
        {
            base.Update(deltaTime);
        }

        protected override void Draw(double deltaTime, Camera camera)
        {
           
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            
        }

        public override GameComponent Clone()
        {
            throw new NotImplementedException();
        }
    }
}
