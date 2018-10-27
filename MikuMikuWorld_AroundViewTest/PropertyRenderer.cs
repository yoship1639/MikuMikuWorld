using MikuMikuWorld.GameComponents;
using MikuMikuWorld.GameComponents.Coliders;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    class PropertyRenderer : TextRenderer
    {
        Brush backBrush;
        ColorCollecter cc;

        public string UserText { get; set; }

        protected override void OnLoad()
        {
            base.OnLoad();

            for (var i = 0; i < 128; i++)　SetText(i, "", 0, i * 16.0f);
            backBrush = new SolidBrush(Color.FromArgb(64, 0, 0, 0));

            cc = MMW.FindGameComponents<ColorCollecter>()[0];
        }

        protected override void Update(double deltaTime)
        {
            base.Update(deltaTime);
            var idx = 0;
            SetText(idx++, "FPS: " + (int)MMW.FPS);
            SetText(idx++, "WindowWidth: " + MMW.ClientSize.Width);
            SetText(idx++, "WindowHeight: " + MMW.ClientSize.Height);
            SetText(idx++, "Intensity: " + MMW.DirectionalLight.Intensity);
            SetText(idx++, "Contrast: " + cc.Contrast);
            SetText(idx++, "Saturation: " + cc.Saturation);
            SetText(idx++, "Brightness: " + cc.Brightness);
            SetText(idx++, "AmbientColor: " + MMW.GlobalAmbient);
            /*
            var objs = MMW.FindGameObjects((o) => true);
            foreach (var obj in objs)
            {
                SetText(idx++, obj.Name + ": " + obj.Transform.Position + obj.Transform.Rotate);
            }*/
        }

        protected override void Draw(double deltaTime, Camera camera)
        {
            base.Draw(deltaTime, camera);

            var mvp = camera.View * camera.Projection;
            Drawer.DrawLine(Vector3.Zero, Vector3.UnitX * 10.0f, mvp, Color4.Red, 3.0f);
            Drawer.DrawLine(Vector3.Zero, Vector3.UnitY * 10.0f, mvp, Color4.Green, 3.0f);
            Drawer.DrawLine(Vector3.Zero, Vector3.UnitZ * 10.0f, mvp, Color4.Blue, 3.0f);
        }
    }
}
