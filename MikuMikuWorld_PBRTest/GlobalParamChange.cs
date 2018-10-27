using MikuMikuWorld.Assets;
using MikuMikuWorld.GameComponents;
using MikuMikuWorld.GameComponents.Lights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    class GlobalParamChange : GameComponent
    {
        public override bool ComponentDupulication { get { return false; } }

        float amb = 0.0f;
        float ibl = 1.0f;
        float intensity = 1.0f;

        PointLight pl;

        protected override void OnLoad()
        {
            base.OnLoad();

            pl = MMW.FindGameComponent<PointLight>();
        }

        protected override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            if (Input.IsKeyDown(OpenTK.Input.Key.F)) amb -= (float)deltaTime * 0.5f;
            if (Input.IsKeyDown(OpenTK.Input.Key.G)) amb += (float)deltaTime * 0.5f;
            if (Input.IsKeyDown(OpenTK.Input.Key.H)) ibl -= (float)deltaTime;
            if (Input.IsKeyDown(OpenTK.Input.Key.J)) ibl += (float)deltaTime;
            if (Input.IsKeyDown(OpenTK.Input.Key.K)) intensity -= (float)deltaTime;
            if (Input.IsKeyDown(OpenTK.Input.Key.L)) intensity += (float)deltaTime;

            MMW.GlobalAmbient = new OpenTK.Graphics.Color4(amb, amb, amb, 1.0f);
            MMW.IBLIntensity = ibl;
            MMW.FindGameComponent<PointLight>().Intensity = intensity;
        }

        public override GameComponent Clone()
        {
            throw new NotImplementedException();
        }
    }
}
