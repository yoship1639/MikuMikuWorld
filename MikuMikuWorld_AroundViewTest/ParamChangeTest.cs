using MikuMikuWorld.Assets;
using MikuMikuWorld.GameComponents;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    class ParamChangeTest : GameComponent
    {
        public override bool ComponentDupulication
        {
            get
            {
                return true;
            }
        }

        private float metallic = 0.0f;
        private float roughness = 0.8f;

        private Material[] materials;

        protected override void OnLoad()
        {
            base.OnLoad();

            var mr = GameObject.GetComponent<MeshRenderer>();
            materials = mr.Materials;
        }

        protected override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            if (Input.IsKeyDown(OpenTK.Input.Key.F)) metallic = MathHelper.Clamp(metallic + (float)deltaTime, 0.0f, 1.0f);
            if (Input.IsKeyDown(OpenTK.Input.Key.G)) metallic = MathHelper.Clamp(metallic - (float)deltaTime, 0.0f, 1.0f);
            if (Input.IsKeyDown(OpenTK.Input.Key.H)) roughness = MathHelper.Clamp(roughness - (float)deltaTime, 0.0f, 1.0f);
            if (Input.IsKeyDown(OpenTK.Input.Key.J)) roughness = MathHelper.Clamp(roughness + (float)deltaTime, 0.0f, 1.0f);

            if (Input.IsKeyDown(OpenTK.Input.Key.F)) GameObject.Transform.Rotate.Y -= (float)deltaTime;
            if (Input.IsKeyDown(OpenTK.Input.Key.G)) GameObject.Transform.Rotate.Y += (float)deltaTime;

            foreach (var m in materials)
            {
                m.TrySetParam("metallic", metallic);
                m.TrySetParam("roughness", roughness);
            }
        }

        public override GameComponent Clone()
        {
            throw new NotImplementedException();
        }
    }
}
