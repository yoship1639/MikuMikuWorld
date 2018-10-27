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
        public ParamChangeTest pct = null;
        public Light light = null;

        protected override void OnLoad()
        {
            base.OnLoad();

            for (var i = 0; i < 128; i++)　SetText(i, "", 0, i * 16.0f);
            backBrush = new SolidBrush(Color.FromArgb(64, 0, 0, 0));
        }

        protected override void Update(double deltaTime)
        {
            base.Update(deltaTime);
            var idx = 0;
            SetText(idx++, "FPS: " + (int)MMW.FPS);
            SetText(idx++, "WindowWidth: " + MMW.ClientSize.Width);
            SetText(idx++, "WindowHeight: " + MMW.ClientSize.Height);
            SetText(idx++, "Ambient: " + MMW.GlobalAmbient);
            SetText(idx++, "IBL Intensity: " + MMW.IBLIntensity);
            SetText(idx++, "Dir Intensity: " + MMW.DirectionalLight.Intensity);
            if (pct != null)
            {
                SetText(idx++, "Roughness: " + pct.Roughness);
                SetText(idx++, "Metallic: " + pct.Metallic);
            }
            if (light != null)
            {
                SetText(idx++, "Light Intensity: " + light.Intensity);
                SetText(idx++, "Light Radius: " + light.Radius);
            }
        }
    }
}
