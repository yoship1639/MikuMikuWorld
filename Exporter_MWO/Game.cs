using MikuMikuWorld;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using MikuMikuWorld.Network;
using MikuMikuWorld.Assets;
using MikuMikuWorld.GameComponents;
using OpenTK.Graphics;
using MikuMikuWorld.GameComponents.Coliders;
using MikuMikuWorld.GameComponents.Lights;

namespace Exporter_MMW
{
    class Game : MMWGameWindow
    {
        public GameObject gameObj;

        public Game() : base(720, 720, "Preview")
        {
            MMW.Configuration.Antialias = MMWConfiguration.AntialiasType.SSAAx2;
            WindowBorder = WindowBorder.Fixed;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            MMW.GlobalAmbient = new Color4(0.4f, 0.4f, 0.4f, 0.0f);
            MMW.MainCamera.ClearFlag = ClearFlag.SolidColor;
            MMW.MainCamera.ClearColor = new Color4(0.4f, 0.4f, 0.4f, 1.0f);
            MMW.MainCamera.Transform.Position = new Vector3(0.0f, 0.8f, -1.4f);
            MMW.MainCamera.GameObject.AddComponent<CameraController>();
            MMW.MainCamera.ForceTarget = true;
            MMW.MainCamera.ShadowMapping = false;

            MMW.DirectionalLight.Transform.Parent = MMW.MainCamera.Transform;
            MMW.DirectionalLight.Direction = Vector3.UnitZ;
        }

        public void OnCharacterLoaded(NwCharacter ch)
        {
            var chara = AssetConverter.FromNwCharacter(ch);

            MMW.Invoke(() =>
            {
                chara.Load();

                var go = CreateGameObject(chara, chara.Name, "Deferred Physical");
                go.AddComponent<CollisionRenderer>();
                MMW.RegistGameObject(go);

                gameObj = go;
            });
        }
        public void OnObjectLoaded(NwObject obj)
        {
            var wo = AssetConverter.FromNwObject(obj);

            MMW.Invoke(() =>
            {
                wo.Load();

                var go = CreateGameObject(wo, wo.Name, "Deferred Physical");
                go.AddComponent<CollisionRenderer>();
                MMW.RegistGameObject(go);

                gameObj = go;
            });
        }
        public void OnClear()
        {
            MMW.Invoke(() =>
            {
                MMW.DestroyGameObject(gameObj);
                gameObj = null;
            });
        }

        public static GameObject CreateGameObject(Character ch, string name, string shader = "Deferred Physical Skin")
        {
            if (!ch.Loaded) ch.Load();

            var sh = MMW.GetAsset<Shader>(shader);
            foreach (var mat in ch.Materials) mat.Shader = sh;

            var go = new GameObject(name);

            var mr = go.AddComponent<MeshRenderer>();
            mr.ForceRendering = true;
            mr.Mesh = ch.Mesh;
            mr.Bones = ch.Bones;
            for (var m = 0; m < mr.Mesh.SubMeshCount; m++)
            {
                var matIndex = mr.Mesh.GetMaterialIndex(m);
                mr.SetMaterial(matIndex, ch.Materials[matIndex], false);
            }

            if (ch.Morphs != null)
            {
                var morpher = go.AddComponent<Morpher>();

                foreach (var m in ch.Morphs)
                {
                    if (m == null) continue;
                    morpher.AddMorph(m.Name, m);
                }
            }

            return go;
        }
        public static GameObject CreateGameObject(WorldObject obj, string name, string shader = "Deferred Physical Skin")
        {
            if (!obj.Loaded) obj.Load();

            var sh = MMW.GetAsset<Shader>(shader);
            foreach (var mat in obj.Materials) mat.Shader = sh;

            var go = new GameObject(name);

            var mr = go.AddComponent<MeshRenderer>();
            mr.ForceRendering = true;
            mr.Mesh = obj.Mesh;
            mr.Bones = obj.Bones;
            for (var m = 0; m < mr.Mesh.SubMeshCount; m++)
            {
                var matIndex = mr.Mesh.GetMaterialIndex(m);
                mr.SetMaterial(matIndex, obj.Materials[matIndex], false);
            }

            if (obj.Morphs != null)
            {
                var morpher = go.AddComponent<Morpher>();

                foreach (var m in obj.Morphs)
                {
                    if (m == null) continue;
                    morpher.AddMorph(m.Name, m);
                }
            }

            return go;
        }

        public void SetMaterialParam<T>(int matIndex, string paramName, T value)
        {
            if (gameObj == null) return;

            var mr = gameObj.GetComponent<MeshRenderer>();
            var mat = mr.GetMaterial(matIndex);

            mat.TrySetParam(paramName, value);
        }

        public void SetTexture(int matIndex, string paramName, Bitmap bitmap)
        {
            if (gameObj == null) return;

            var mr = gameObj.GetComponent<MeshRenderer>();
            var mat = mr.GetMaterial(matIndex);
            var bm = (Bitmap)bitmap.Clone();
            MMW.Invoke(() =>
            {
                var tex = new Texture2D(bm, false);
                tex.Load();

                mat.TrySetParam(paramName, tex);
            });
        }

        public void SetPhysics(ICollisionShape shape)
        {
            if (gameObj == null) return;
            gameObj.GetComponent<CollisionRenderer>().Shape = shape;
        }
    }
}
