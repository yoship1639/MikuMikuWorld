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
            base(1280, 700, "TestWindow")
        { }
        
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            WindowBorder = WindowBorder.Fixed;
            MMW.MainCamera.DebugDraw = true;

            var t = MMW.MainCamera.GameObject.Transform;
            t.Position = new Vector3(0.0f, 1.0f, -3.0f);
            t.Rotate.X = -0.3f;

            var boxmesh = Mesh.CreateSimpleBoxMesh(new Vector3(0.5f));
            boxmesh.Colors = new Color4[]
            {
                new Color4(  0,   0, 255, 255),
                new Color4(  0, 255, 255, 255),
                new Color4(255, 255, 255, 255),
                new Color4(255,   0, 255, 255),
                new Color4(  0,   0,   0, 255),
                new Color4(  0, 255,   0, 255),
                new Color4(255, 255,   0, 255),
                new Color4(255,   0,   0, 255),
            };
            boxmesh.Load();

            var box = new GameObject("Box");
            var renderer = box.AddComponent<MeshRenderer>(boxmesh);
            renderer.GetMaterial(0).AddParam("diffuse", Color4.White);
            box.AddComponent<RotateTest>();
            MMW.RegistGameObject(box);

            var probj = new GameObject("Property Renderer") { Layer = 31 };
            probj.AddComponent<PropertyRenderer>();
            MMW.RegistGameObject(probj);
        }
    }
}
