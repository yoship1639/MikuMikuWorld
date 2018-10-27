using MikuMikuWorld.Assets;
using MikuMikuWorld.GameComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    class ParamChangeTest : GameComponent
    {
        public override bool ComponentDupulication { get { return false; } }

        public float Roughness { get; set; } = 0.8f;
        public float Metallic { get; set; } = 0.0f;

        Material[] materials;

        protected override void OnLoad()
        {
            base.OnLoad();

            var mr = GameObject.GetComponent<MeshRenderer>();
            materials = mr.Materials;
        }

        protected override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            if (Input.IsKeyDown(OpenTK.Input.Key.Z)) Roughness -= (float)deltaTime * 0.5f;
            if (Input.IsKeyDown(OpenTK.Input.Key.X)) Roughness += (float)deltaTime * 0.5f;
            if (Input.IsKeyDown(OpenTK.Input.Key.C)) Metallic -= (float)deltaTime * 0.5f;
            if (Input.IsKeyDown(OpenTK.Input.Key.V)) Metallic += (float)deltaTime * 0.5f;

            if (Input.IsKeyDown(OpenTK.Input.Key.Left)) GameObject.Transform.Rotate.Y += (float)deltaTime;
            if (Input.IsKeyDown(OpenTK.Input.Key.Right)) GameObject.Transform.Rotate.Y -= (float)deltaTime;

            foreach (var m in materials)
            {
                m.SetParam("roughness", Roughness);
                m.SetParam("metallic", Metallic);
            }
        }

        public override GameComponent Clone()
        {
            throw new NotImplementedException();
        }
    }
}
