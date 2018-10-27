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

            SetText(0, "FPS: " + MMW.FPS, 0.0f, 0.0f);
            SetText(1, "[I] UP Impulse", 0.0f, 16.0f);
            SetText(2, "[T] Y-asix Torque", 0.0f, 32.0f);
            backBrush = new SolidBrush(Color.FromArgb(64, 0, 0, 0));
        }

        protected override void Update(double deltaTime)
        {
            base.Update(deltaTime);
            SetText(0, "FPS: " + MMW.FPS);
        }
    }
}
