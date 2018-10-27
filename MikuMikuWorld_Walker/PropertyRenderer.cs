using MikuMikuWorld.GameComponents;
using MikuMikuWorld.GameComponents.Coliders;
using MikuMikuWorld.GameComponents.ImageEffects;
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
    class PropertyRenderer : TextRenderer
    {
        Brush backBrush;

        public string UserText { get; set; }

        InitLoading load;
        public bool Visible { get; set; }

        protected override void OnLoad()
        {
            base.OnLoad();
            Layer = LayerUI + 2;

            for (var i = 0; i < 80; i++)　SetText(i, "", 0, i * 16.0f);
            backBrush = new SolidBrush(Color.FromArgb(64, 0, 0, 0));

            load = MMW.FindGameComponent<InitLoading>();

            
        }

        protected override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            if (Input.IsKeyPressed(Key.F3)) Visible = !Visible;

            var idx = 0;
            SetText(idx++, "FPS: " + MMW.FPS.ToString());
            SetText(idx++, "Memory Used: " + Environment.WorkingSet / 1024 / 1024 + "MB");
            SetText(idx++, "Loading State: " + load.State.ToString());
            SetText(idx++, "Dir Intenity: " + MMW.DirectionalLight.Intensity);
            SetText(idx++, "FoV: " + MMW.MainCamera.FoV);
            //SetText(idx++, load.PresetCharacters.Length + "/" + load.PresetCharacterNum);
            //SetText(idx++, load.PresetStages.Length + "/" + load.PresetStageNum);
            //SetText(idx++, load.PresetObjects.Length + "/" + load.PresetObjectNum);
            //SetText(idx++, load.FreeCharacters.Length + "/" + load.FreeCharacterNum);
            //SetText(idx++, load.FreeStages.Length + "/" + load.FreeStageNum);
            //SetText(idx++, load.FreeObjects.Length + "/" + load.FreeObjectNum);
            var objs = MMW.GetAllGameObject();
            foreach (var o in objs)
            {
                SetText(idx++, o.Name);
                if (idx >= 80) return;
            }
            //SetText(idx++, MMW.MainCamera.GameObject.GetComponent<BokehDoF>().Focus.ToString());
            //SetText(idx++, MMW.MainCamera.GameObject.GetComponent<BokehDoF>().Bias.ToString());
            //SetText(idx++, "WindowWidth: " + MMW.ClientSize.Width);
            //SetText(idx++, "WindowWidth: " + MMW.ClientSize.Width);
            //SetText(idx++, "WindowHeight: " + MMW.ClientSize.Height);
            //SetText(idx++, "Intensity: " + MMW.DirectionalLight.Intensity);
            //SetText(idx++, "Contrast: " + cc.Contrast);
            //SetText(idx++, "Saturation: " + cc.Saturation);
            //SetText(idx++, "Brightness: " + cc.Brightness);
            //SetText(idx++, "AmbientColor: " + MMW.GlobalAmbient);
            /*
            var objs = MMW.FindGameObjects((o) => true);
            foreach (var obj in objs)
            {
                SetText(idx++, obj.Name + ": " + obj.Transform.Position + obj.Transform.Rotate);
            }*/
            for (var i = idx; i < 80; i++) SetText(i, "", 0, i * 16.0f);
        }

        protected override void Draw(double deltaTime, Camera camera)
        {
            if (Visible) base.Draw(deltaTime, camera);
        }
    }
}
