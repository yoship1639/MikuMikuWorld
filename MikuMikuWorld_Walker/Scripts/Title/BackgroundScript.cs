using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikuMikuWorld.GameComponents;
using MikuMikuWorld.Assets;
using MikuMikuWorld.Properties;
using OpenTK;
using OpenTK.Graphics;

namespace MikuMikuWorld.Scripts
{
    class BackgroundScript : DrawableGameComponent
    {
        Texture2D tex;
        public BGDrawer Background { get; private set; }

        public void Trans(Color4 color, double transTime)
        {
            Background.Color.Trans(color, transTime);
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            Layer = 0;

            tex = new Texture2D(Resources.mmw_bg2);
            tex.Load();

            Background = new BGDrawer(tex);
            Background.Speed = new Vector2(5.0f, 10.0f);
            //Background.Color.Set(new Color4(148, 212, 222, 255));
            Background.Color.Set(Color4.White);
        }

        protected override void Update(double deltaTime)
        {
            Background.Update(deltaTime);
        }

        protected override void Draw(double deltaTime, Camera camera)
        {
            Background.Draw(deltaTime);
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            tex.Unload();
        }

        public override GameComponent Clone()
        {
            throw new NotImplementedException();
        }
    }
}
