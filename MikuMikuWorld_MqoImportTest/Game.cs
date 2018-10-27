using MikuMikuWorld.Assets;
using MikuMikuWorld.Assets.Shaders;
using MikuMikuWorld.GameComponents;
using MikuMikuWorld.GameComponents.Coliders;
using MikuMikuWorld.GameComponents.ImageEffects;
using MikuMikuWorld.Importers;
using MikuMikuWorld.Properties;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    class Game : MMWGameWindow
    {
        public Game() : 
            base(1280, 720, "MQO Import Test")
        {
            MMW.Configuration.Antialias = MMWConfiguration.AntialiasType.FXAA;
        }
        
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            WindowBorder = WindowBorder.Fixed;
            MMW.MainCamera.DebugDraw = true;

            var t = MMW.MainCamera.GameObject.Transform;
            t.Position = new Vector3(0.0f, 2.0f, -5.0f);
            t.Rotate.X = -0.3f;
            MMW.MainCamera.Near = 0.01f;
            MMW.MainCamera.Far = 100.0f;

            //MMW.MainCamera.GameObject.AddComponent<Blur>(20.0f, 2);

            var importer = new MqoImporter();
            //var results = importer.Import(@"../../mqo/brick.mqo");
            var results = importer.Import(@"C:\Users\yoship\Downloads\mqo\Old_Station\Old Station\Old_station.mqo", ImportType.Full);

            foreach (var tex in results[0].Textures) tex.Load();
            foreach (var mesh in results[0].Meshes) mesh.Load();
            foreach (var mat in results[0].Materials)
            {
                mat.Shader = MMW.GetAsset<Shader>("Test");
            }

            var obj = new GameObject(results[0].Name, Matrix4.Identity, "mqo");
            for (var i = 0; i < results[0].Meshes.Length; i++)
            {
                var mr = obj.AddComponent<MeshRenderer>();
                mr.Mesh = results[0].Meshes[i];
                for (var m = 0; m < mr.Mesh.SubMeshCount; m++)
                {
                    var matIndex = mr.Mesh.GetMaterialIndex(m);
                    mr.SetMaterial(matIndex, results[0].Materials[matIndex], true);
                }
            }
            
            obj.AddComponent<RotateTest>();
            MMW.RegistGameObject(obj);

            var probj = new GameObject("Property Renderer") { Layer = 31 };
            probj.AddComponent<PropertyRenderer>();
            MMW.RegistGameObject(probj);
        }
    }
}
