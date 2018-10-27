using MikuMikuWorld.Assets;
using MikuMikuWorld.Assets.Shaders;
using MikuMikuWorld.GameComponents;
using MikuMikuWorld.GameComponents.Coliders;
using MikuMikuWorld.GameComponents.ImageEffects;
using MikuMikuWorld.GameComponents.Lights;
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
            base(1080, 640, "Physical Based Rendering Test")
        {
            MMW.Configuration.Antialias = MMWConfiguration.AntialiasType.FXAA;
            MMW.Configuration.ShadowQuality = MMWConfiguration.ShadowQualityType.Default;
            MMW.Configuration.Shader = MMWConfiguration.ShaderType.Toon;
            MMW.Configuration.IBLQuality = MMWConfiguration.IBLQualityType.High;
            MMW.Configuration.AO = MMWConfiguration.AOType.SSAO;
            MMW.Configuration.Bloom = MMWConfiguration.BloomType.Bloom;

            WindowState = WindowState.Fullscreen;
            ClientSize = new Size(1366, 768);
        }

        MqoImporter mqoImporter;
        PmdImporter pmdImporter;
        PmxImporter pmxImporter;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // play offline
            // play online

            WindowBorder = WindowBorder.Fixed;
            CursorVisible = false;

            MMW.MainCamera.GameObject.Transform.Position = new Vector3(0.0f, 1.5f, -4.0f);
            MMW.MainCamera.GameObject.Transform.Rotate = new Vector3(0.2f, 0.0f, 0.0f);
            MMW.MainCamera.GameObject.AddComponent<GlobalParamChange>();
            MMW.MainCamera.ClearSkyBox = MMW.GetAsset<TextureCube>("DefaultSkyBox");

            MMW.DirectionalLight.Intensity = 1.0f;
            MMW.DirectionalLight.Transform.Rotate.X *= -1.0f;
            MMW.DirectionalLight.Color = Color4.White;

            mqoImporter = new MqoImporter();
            pmdImporter = new PmdImporter();
            pmxImporter = new PmxImporter();

            var sphere = Mesh.CreateSimpleSphereMesh(0.36f, 24, 20);
            var mat = new Material("pbr", MMW.GetAsset<Shader>("Physical"));
            mat.AddParam("diffuse", Color4.White);
            mat.AddParam("roughness", 0.5f);
            mat.AddParam("metallic", 0.0f);
            mat.AddParam("f0", new Color4(0.8f, 0.8f, 0.8f, 1.0f));

            var roughnesses = new float[]
            {
                0.9f,
                0.6f,
                0.4f,
                0.25f,
                0.1f
            };

            var metallics = new float[]
            {
                0.0f,
                0.0f,
                1.0f,
                1.0f,
                1.0f,
            };

            var diffuses = new Color4[]
            {
                new Color4(1.0f, 1.0f, 0.9f, 1.0f),
                Material.IronColor,
                Material.CopperColor,
                Material.SilverColor,
                Material.GoldColor,
            };

            var f0s = new Color4[]
            {
                Material.WaterF0Color,
                Material.PlasticF0Color,
                Material.CopperF0Color,
                Material.SilverF0Color,
                Material.GoldF0Color,
            };

            for (var x = 0; x < 5; x++)
            {
                var obj = new GameObject("Sphere", Matrix4.CreateTranslation(new Vector3(x - 2, 0, 0)));
                var mr = obj.AddComponent<MeshRenderer>(sphere);
                mat.SetParam("roughness", roughnesses[x]);
                mat.SetParam("metallic", metallics[x]);
                mat.SetParam("diffuse", diffuses[x]);
                mat.SetParam("f0", f0s[x]);

                mr.SetMaterial(0, mat, false);
                MMW.RegistGameObject(obj);
            }

            var plane = new GameObject("plane", Matrix4.CreateTranslation(new Vector3(0.0f, -0.36f, 0.0f)));
            var pmr = plane.AddComponent<MeshRenderer>(Mesh.CreateSimplePlaneMesh(5.0f, 5.0f));
            pmr.SetMaterial(0, mat, false);
            MMW.RegistGameObject(plane);

            var pl = new GameObject();
            pl.AddComponent<PointLight>();
            pl.UpdateAction += (s, ev) =>
            {
                pl.Transform.Position = new Vector3((float)Math.Sin(MMW.TotalElapsedTime) * 2.0f, 2.0f, 0.0f);
            };
            MMW.RegistGameObject(pl);

            //var miku = CreatePmxObject(@"C:\Users\yoship\Downloads\mmd\Models\MikuV4X_Digitrevx\MikuV4X.pmx");
            //var miku = CreatePmdObject(@"C:\Users\yoship\Downloads\mmd\Models\vocaloid\頭の悪い人.pmd");
            //var miku = CreatePmdObject(@"C:\Users\yoship\Downloads\mmd\Models\ドイツ邸001\ドイツ邸セット.pmd");
            //miku.AddComponent<ParamChangeTest>();
            //MMW.RegistGameObject(miku);


            var probj = new GameObject("Property Renderer") { Layer = GameObject.LayerUI };
            var pr = probj.AddComponent<PropertyRenderer>();
            //pr.pct = miku.GetComponent<ParamChangeTest>();
            //pr.light = point.GetComponent<PointLight>();
            MMW.RegistGameObject(probj);
        }

        private GameObject CreateMqoObject(string filename)
        {
            var results = mqoImporter.Import(filename, ImportType.Full);
            foreach (var tex in results[0].Textures) tex.Load();
            foreach (var mesh in results[0].Meshes) mesh.Load();
            foreach (var mat in results[0].Materials)
            {
                mat.Shader = MMW.GetAsset<Shader>("Physical");
                mat.AddParam("roughness", "Roughness", 0.7f);
                mat.AddParam("metallic", "Metallic", 0.0f);
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

            return obj;
        }

        private GameObject CreatePmdObject(string filename)
        {
            var results = pmdImporter.Import(filename, ImportType.Full);
            foreach (var tex in results[0].Textures) tex.Load();
            foreach (var mesh in results[0].Meshes) mesh.Load();
            foreach (var mat in results[0].Materials)
            {
                mat.Shader = MMW.GetAsset<Shader>("Physical");
                mat.AddParam("roughness", "Roughness", 0.7f);
                mat.AddParam("metallic", "Metallic", 0.0f);
            }

            var obj = new GameObject(results[0].Name, Matrix4.Identity, "pmd");
            
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

            return obj;
        }

        private GameObject CreatePmxObject(string filename)
        {
            var results = pmxImporter.Import(filename, ImportType.Full);
            foreach (var tex in results[0].Textures) if (tex != null) tex.Load();
            foreach (var mesh in results[0].Meshes) mesh.Load();
            foreach (var mat in results[0].Materials)
            {
                //mat.Shader = MMW.GetAsset<Shader>("Toon Shadow");
                mat.Shader = MMW.GetAsset<Shader>("Physical");
                mat.AddParam("roughness", "Roughness", 0.7f);
                mat.AddParam("metallic", "Metallic", 0.0f);
            }

            var obj = new GameObject(results[0].Name, Matrix4.Identity, "pmx");

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

            return obj;
        }
    }
}
