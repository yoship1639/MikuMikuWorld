using MikuMikuWorld.Assets;
using MikuMikuWorld.Assets.Shaders;
using MikuMikuWorld.GameComponents;
using MikuMikuWorld.GameComponents.Coliders;
using MikuMikuWorld.GameComponents.ImageEffects;
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
    class Game : MMWGameWindow
    {
        public Game() : 
            base(1280, 700, "PhysicsTest")
        { }
        
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            WindowBorder = WindowBorder.Fixed;
            MMW.MainCamera.DebugDraw = true;

            var t = MMW.MainCamera.GameObject.Transform;
            t.Position = new Vector3(1.0f, 2.0f, -20.0f);

            var rand = new Random();
            for (var i = 0; i < 50; i++)
            {
                var x = rand.Next(20) - 10;
                var z = rand.Next(20) - 10;
                var y = rand.Next(3, 10);
                var b = new GameObject("Box" + i, Matrix4.CreateTranslation(x, y, z));
                b.AddComponent<BoxCollider>();
                var r = b.AddComponent<RigidBody>();
                b.AddComponent<PhysicsTest>();
                MMW.RegistGameObject(b);
            }

            {
                var plane = new GameObject("Plane", Matrix4.CreateTranslation(new Vector3(0.0f, -2.0f, 0.0f)));
                var planeMesh = Mesh.CreateSimplePlaneMesh(30.0f, 30.0f);
                var mr = plane.AddComponent<MeshRenderer>(planeMesh);
                var mat = new Material("Test", MMW.GetAsset<Shader>("Test"));
                mat.AddParam("diffuse", Color4.Gray);
                mr.SetMaterial(0, mat, false);
                plane.AddComponent<PlaneCollider>(30.0f, 30.0f);
                MMW.RegistGameObject(plane);
            }

            var probj = new GameObject("Property Renderer") { Layer = 31 };
            probj.AddComponent<PropertyRenderer>();
            MMW.RegistGameObject(probj);
        }
    }
}
