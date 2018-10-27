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
            base(1080, 640, "Around View Test")
        {
            MMW.Configuration.Antialias = MMWConfiguration.AntialiasType.FXAA;
            MMW.Configuration.ShadowQuality = MMWConfiguration.ShadowQualityType.Default;
            MMW.Configuration.AO = MMWConfiguration.AOType.DoNotAO;
            MMW.Configuration.Shader = MMWConfiguration.ShaderType.Toon;
            MMW.Configuration.IBLQuality = MMWConfiguration.IBLQualityType.Default;
            MMW.Configuration.DrawEdge = false;

            WindowState = WindowState.Fullscreen;
            ClientSize = new Size(1366, 768);
        }

        PmdImporter pmdImporter;
        PmxImporter pmxImporter;
        MqoImporter mqoImporter;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // play offline
            // play online

            WindowBorder = WindowBorder.Fixed;
            CursorVisible = false;

            MMW.MainCamera.GameObject.Transform.Position = new Vector3(0.0f, 1.45f, -3.0f);
            MMW.MainCamera.GameObject.AddComponent<AroundViewTest>();
            MMW.MainCamera.GameObject.AddComponent<CameraMoveTest>();
            var cc = MMW.MainCamera.GameObject.AddComponent<ColorCollecter>();
            cc.Saturation = 1.4f;

            MMW.IBLIntensity = 1.0f;

            pmdImporter = new PmdImporter();
            pmxImporter = new PmxImporter();
            mqoImporter = new MqoImporter();

            var miku = CreatePmdObject(@"C:\Users\yoship\Downloads\mmd\Models\vocaloid\初音ミク.pmd");
            miku.AddComponent<ParamChangeTest>();
            MMW.RegistGameObject(miku);

            var rin = CreatePmdObject(@"C:\Users\yoship\Downloads\mmd\Models\vocaloid\鏡音リン.pmd");
            rin.AddComponent<ParamChangeTest>();
            rin.Transform.Position.X = 1.0f;
            MMW.RegistGameObject(rin);

            var meiko = CreatePmdObject(@"C:\Users\yoship\Downloads\mmd\MikuMikuDance_v926x64\UserFile\Model\MEIKO.pmd");
            meiko.AddComponent<ParamChangeTest>();
            meiko.Transform.Position.X = 2.0f;
            MMW.RegistGameObject(meiko);

            var atama = CreatePmxObject(@"C:\Users\yoship\Downloads\mmd\Models\頭の悪い人\頭の悪い人.pmx");
            atama.AddComponent<ParamChangeTest>();
            atama.Transform.Position.X = 3.0f;
            MMW.RegistGameObject(atama);

            //var stage = CreatePmdObject(@"C:\Users\yoship\Downloads\mmd\Models\ドイツ邸001\ドイツ邸セット.pmd");

            var files = new string[]
            {
                @"C:\Users\yoship\Downloads\mmd\Models\kelorin3_forMMD\block_a.pmx",
                @"C:\Users\yoship\Downloads\mmd\Models\kelorin3_forMMD\block_b.pmx",
                @"C:\Users\yoship\Downloads\mmd\Models\kelorin3_forMMD\block_b_k.pmx",
                @"C:\Users\yoship\Downloads\mmd\Models\kelorin3_forMMD\block_c.pmx",
                @"C:\Users\yoship\Downloads\mmd\Models\kelorin3_forMMD\block_canal.pmx",
                @"C:\Users\yoship\Downloads\mmd\Models\kelorin3_forMMD\block_d.pmx",
                @"C:\Users\yoship\Downloads\mmd\Models\kelorin3_forMMD\block_out01.pmx",
                //@"C:\Users\yoship\Downloads\mmd\Models\kelorin3_forMMD\block_out02-1.pmx",
                //@"C:\Users\yoship\Downloads\mmd\Models\kelorin3_forMMD\block_out02-2.pmx",
                @"C:\Users\yoship\Downloads\mmd\Models\kelorin3_forMMD\block_outer.pmx",
            };

            foreach (var file in files)
            {
                var stage = CreatePmxObject(file);
                var mr = stage.GetComponent<MeshRenderer>();
                stage.AddComponent<MeshCollider>(mr.Mesh);
                //stage.AddComponent<ParamChangeTest>();
                MMW.RegistGameObject(stage);
            }

            var probj = new GameObject("Property Renderer");
            probj.AddComponent<PropertyRenderer>();
            MMW.RegistGameObject(probj);

            GC.Collect();
        }

        private GameObject CreateMqoObject(string filename)
        {
            var results = mqoImporter.Import(filename, ImportType.Full);
            foreach (var tex in results[0].Textures) tex.Load();
            foreach (var mesh in results[0].Meshes) mesh.Load();
            foreach (var mat in results[0].Materials)
            {
                mat.Shader = MMW.GetAsset<Shader>("Toon Shadow");
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
                mat.Shader = MMW.GetAsset<Shader>("Toon Shadow");
                mat.AddParam("limCoeff", "LimCoefficient", 0.4f);
                //mat.Shader = MMW.GetAsset<Shader>("Physical");
                //mat.AddParam<float>("metallic", "Metallic", 0.0f);
                //mat.AddParam<float>("roughness", "Roughness", 0.8f);
                //mat.AddParam("f0", new Color4(0.2f, 0.2f, 0.2f, 1.0f));
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
            foreach (var tex in results[0].Textures) tex.Load();
            foreach (var mesh in results[0].Meshes) mesh.Load();
            foreach (var mat in results[0].Materials)
            {
                mat.Shader = MMW.GetAsset<Shader>("Toon Shadow");
                mat.AddParam("limCoeff", "LimCoefficient", 0.0f);
                mat.SetParam("specular", Color4.Black);
            }

            var obj = new GameObject(results[0].Name, Matrix4.Identity, "pmx");
            CreateBoneObject(obj.Transform, new Bone[] { results[0].Bones[0] });

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

        private void CreateBoneObject(Transform parent, Bone[] children)
        {
            foreach (var c in children)
            {
                var obj = new GameObject(c.Name, Matrix4.Identity);
                //obj.Layer = GameObject.LayerUI;
                obj.Transform.Parent = parent;
                obj.Transform.WorldTransform = Matrix4.CreateTranslation(c.Position);
                obj.AddComponent<BoneVisibleTest>(c);

                if (c.Children != null) CreateBoneObject(obj.Transform, c.Children);

                MMW.RegistGameObject(obj);
            }
        }
    }
}
