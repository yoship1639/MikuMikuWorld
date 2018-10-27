using MikuMikuWorld.Assets;
using MikuMikuWorld.GameComponents;
using MikuMikuWorld.GameComponents.Lights;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    class SimpleDrawableObject
    {
        private GameObject go;
        private GameObject light;
        public MeshRenderer MeshRenderer;
        private Shader sh;

        public Matrix4 Model { get; set; }
        public Matrix4 View { get; set; }
        public Matrix4 Projection { get; set; }
        public DirectionalLight DirLight { get; set; }

        public void Load(Mesh mesh, Material[] mats, string shader = "Physical")
        {
            if (!mesh.Loaded) mesh.Load();

            sh = MMW.GetAsset<Shader>(shader);
            foreach (var mat in mats) mat.Shader = sh;

            go = new GameObject();

            //  mesh renderer
            MeshRenderer = go.AddComponent<MeshRenderer>();
            MeshRenderer.Mesh = mesh;
            for (var m = 0; m < MeshRenderer.Mesh.SubMeshCount; m++)
            {
                var matIndex = MeshRenderer.Mesh.GetMaterialIndex(m);
                MeshRenderer.SetMaterial(matIndex, mats[matIndex], true);
            }

            light = new GameObject();
            DirLight = light.AddComponent<DirectionalLight>();
            DirLight.Intensity = 4.0f;
            DirLight.Direction = Vector3.UnitZ;
        }

        public void Draw(double deltaTime, Camera camera)
        {
            if (MeshRenderer == null) return;

            var drawMeshDic = new Dictionary<Material, List<SubMesh>>();

            for (var i = 0; i < MeshRenderer.MaterialCount; i++)
            {
                var mat = MeshRenderer.GetMaterialAt(i);
                drawMeshDic.Add(mat, new List<SubMesh>());
            }
            foreach (var sm in MeshRenderer.Mesh.subMeshes)
            {
                var mat = MeshRenderer.GetMaterial(sm.materialIndex);
                drawMeshDic[mat].Add(sm);
            }

            var sp = new ShaderUniqueParameter()
            {
                camera = camera,
                cameraDir = camera.WorldDirection,
                cameraPos = camera.Transform.WorldPosition,
                deltaTime = deltaTime,
                dirLight = DirLight,
                environmentMap = camera.EnvironmentMap,
                proj = Projection,
                resolution = new Vector2(MMW.Width, MMW.Height),
                projInverse = Projection.Inverted(),
                view = View,
                viewInverse = View.Inverted(),
                viewProj = View * Projection,
                viewProjInverse = (View * Projection).Inverted(),
                world = Model,
                worldInv = Model.Inverted(),
                skinning = false,
                morphing = false,
            };

            sh.UseShader();
            sh.SetUniqueParameter(sp, true);
            foreach (var mat in drawMeshDic)
            {
                mat.Key.ApplyShaderParam();
                foreach (var sub in mat.Value)
                {
                    sh.SetUniqueParameter(sp, false);
                    Drawer.DrawSubMesh(sub);
                }
            }
            sh.UnuseShader();
        }
    }
}
