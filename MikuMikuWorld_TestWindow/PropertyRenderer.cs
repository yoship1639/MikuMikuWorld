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

        public string UserText { get; set; }

        protected override void OnLoad()
        {
            base.OnLoad();

            for (var i = 0; i < 3; i++)　SetText(i, "", 0, i * 16.0f);
            backBrush = new SolidBrush(Color.FromArgb(64, 0, 0, 0));
        }

        protected override void Update(double deltaTime)
        {
            base.Update(deltaTime);
            var idx = 0;
            SetText(idx++, "FPS: " + (int)MMW.FPS);
            SetText(idx++, "WindowWidth: " + MMW.ClientSize.Width);
            SetText(idx++, "WindowHeight: " + MMW.ClientSize.Height);
        }
    }
}
