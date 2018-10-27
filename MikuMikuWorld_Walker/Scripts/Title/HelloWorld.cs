using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MikuMikuWorld.Controls;
using MikuMikuWorld.GameComponents;
using OpenTK;
using OpenTK.Graphics;

namespace MikuMikuWorld.Scripts.Title
{
    class HelloWorld : DrawableGameComponent
    {
        private string text;

        protected override void OnLoad()
        {
            base.OnLoad();

            Layer = LayerUI + 3;

            Task.Factory.StartNew(() =>
            {
                //Thread.Sleep(1500);

                //text = "hello";

                Thread.Sleep(1500);

                text = "hello world";

                Thread.Sleep(3000);

                MMW.Invoke(() =>
                {
                    MMW.DestroyGameObject(GameObject);
                    MMW.Window.CursorVisible = true;

                    var objs = MMW.FindGameObjects(g => true);
                    foreach (var obj in objs) obj.Enabled = true;
                });
            });
        }

        protected override void Draw(double deltaTime, Camera camera)
        {
            Drawer.FillRect(Vector2.Zero, MMW.ClientSize.ToVector2(), Color4.Black);
            var g = Drawer.GetGraphics();
            g.DrawString(text, ControlDrawer.fontSmall, Brushes.White, MMW.Width * 0.5f - 70.0f, MMW.Height * 0.5f);
            Drawer.IsGraphicsUsed = true;
        }
    }
}
