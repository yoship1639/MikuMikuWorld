using MikuMikuWorld.GameComponents;
using MikuMikuWorld.GameComponents.Coliders;
using MikuMikuWorld.GameComponents.ImageEffects;
using MikuMikuWorld.GameComponents.Lights;
using MikuMikuWorld.Scripts;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    class Debugger : TextRenderer
    {
        Brush backBrush;

        public bool Visible { get; set; }

        int index = 0;

        protected override void OnLoad()
        {
            base.OnLoad();
            Layer = LayerUI + 2;

            for (var i = 0; i < 128; i++)　SetText(i, "", 0, i * 16.0f);
            backBrush = new SolidBrush(Color.FromArgb(64, 0, 0, 0));
        }

        protected override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            if (Input.IsKeyPressed(Key.F1)) Visible = !Visible;

            if (!Visible) return;

            //var pl = MMW.FindGameComponent<PointLight>();

            if (Input.IsKeyPressed(Key.Up)) index = MMWMath.Repeat(index - 1, 0, 13);
            if (Input.IsKeyPressed(Key.Down)) index = MMWMath.Repeat(index + 1, 0, 13);

            if (Input.IsKeyDown(Key.Right))
            {
                if (index == 0) MMW.Contrast += (float)deltaTime;
                if (index == 1) MMW.Saturation += (float)deltaTime;
                if (index == 2) MMW.Brightness += (float)deltaTime;
                if (index == 3) MMW.IBLIntensity += (float)deltaTime;
                if (index == 4) MMW.GlobalAmbient = new Color4(MMW.GlobalAmbient.R + (float)deltaTime, MMW.GlobalAmbient.G + (float)deltaTime, MMW.GlobalAmbient.B + (float)deltaTime, 0.0f);
                if (index == 5) MMW.MainCamera.FoV = MMWMath.Clamp(MMW.MainCamera.FoV + (float)deltaTime, 0.01f, 3.0f);
                if (index == 6) MMW.DirectionalLight.Intensity += (float)deltaTime;
                if (index == 7) MMW.FogIntensity += (float)deltaTime;
            }
            if (Input.IsKeyDown(Key.Left))
            {
                if (index == 0) MMW.Contrast -= (float)deltaTime;
                if (index == 1) MMW.Saturation -= (float)deltaTime;
                if (index == 2) MMW.Brightness -= (float)deltaTime;
                if (index == 3) MMW.IBLIntensity -= (float)deltaTime;
                if (index == 4) MMW.GlobalAmbient = new Color4(MMW.GlobalAmbient.R - (float)deltaTime, MMW.GlobalAmbient.G - (float)deltaTime, MMW.GlobalAmbient.B - (float)deltaTime, 0.0f);
                if (index == 5) MMW.MainCamera.FoV = MMWMath.Clamp(MMW.MainCamera.FoV - (float)deltaTime, 0.01f, 3.0f);
                if (index == 6) MMW.DirectionalLight.Intensity -= (float)deltaTime;
                if (index == 7) MMW.FogIntensity -= (float)deltaTime;
            }

            var texts = new string[]
            {
                "Contrast : " + MMW.Contrast,
                "Saturation : " + MMW.Saturation,
                "Brightness : " + MMW.Brightness,
                "IBL Intensity : " + MMW.IBLIntensity,
                "Ambient : " + MMW.GlobalAmbient.R,
                "FoV : " + MMW.MainCamera.FoV,
                "Intensity : " + MMW.DirectionalLight.Intensity,
                "Fog : " + MMW.FogIntensity,
                "Dir : " + MMW.DirectionalLight.Direction,
                "Local Dir : " + MMW.DirectionalLight.LocalDirection,
                "World Dir : " + MMW.DirectionalLight.WorldDirection,
            };

            for (var i = 0; i < texts.Length; i++)
            {
                if (index == i) texts[i] = texts[i].Insert(0, "> ");
                else texts[i] = texts[i].Insert(0, "  ");
            }

            var idx = 0;
            for (var i = 0; i < texts.Length; i++) SetText(idx++, texts[i]);

            for (var i = idx; i < 128; i++) SetText(i, "", 0, i * 16.0f);
        }

        protected override void Draw(double deltaTime, Camera camera)
        {
            if (Visible) base.Draw(deltaTime, camera);
        }
    }
}
