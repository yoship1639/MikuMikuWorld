using MikuMikuWorld.Assets;
using MikuMikuWorld.GameComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    class LightParamChange : GameComponent
    {
        public override bool ComponentDupulication { get { return false; } }

        Light light;

        protected override void OnLoad()
        {
            base.OnLoad();

            light = GameObject.GetComponent<Light>();
        }

        protected override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            if (Input.IsKeyDown(OpenTK.Input.Key.E)) light.Intensity -= (float)deltaTime;
            if (Input.IsKeyDown(OpenTK.Input.Key.R)) light.Intensity += (float)deltaTime;
            if (Input.IsKeyDown(OpenTK.Input.Key.T)) light.Radius -= (float)deltaTime * 10.0f;
            if (Input.IsKeyDown(OpenTK.Input.Key.Y)) light.Radius += (float)deltaTime * 10.0f;
        }

        public override GameComponent Clone()
        {
            throw new NotImplementedException();
        }
    }
}
