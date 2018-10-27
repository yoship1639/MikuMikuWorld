using MikuMikuWorld.GameComponents;
using MikuMikuWorld.GameComponents.Lights;
using MikuMikuWorld.Properties;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Assets.Shaders
{
    public class DeferredPhysicalShader : GLSLShader
    {
        private Texture2D whiteMap;
        private Texture2D defaultNormalMap;

        private int loc_m;
        private int loc_mit;
        private int loc_mvp;
        private int loc_oldmvp;
        //private int loc_camDir;
        private int loc_camPos;
        private int loc_resolution;
        //private int loc_shadowMV1;
        private int loc_shadowMV2;
        private int loc_shadowMV3;
        //private int loc_deltaTime;

        public DeferredPhysicalShader(string name = "Deferred Physical") : base(name)
        {
            VertexCode = Resources.DeferredPhysical_vert;
            FragmentCode = Resources.DeferredPhysical_frag;

            RegistShaderParam<Matrix4>("M", "Model");
            RegistShaderParam<Matrix4>("MIT", "ModelInverseTranspose");
            RegistShaderParam<Matrix4>("MVP", "ModelViewProjection");
            RegistShaderParam<Matrix4>("OldMVP", "OldModelViewProjection");
            RegistShaderParam<Matrix4>("shadowMV1", "ShadowModelView1");
            RegistShaderParam<Matrix4>("shadowMV2", "ShadowModelView2");
            RegistShaderParam<Matrix4>("shadowMV3", "ShadowModelView3");

            RegistShaderParam<Color4>("albedo", "Albedo");
            RegistShaderParam<float>("metallic", "Metallic");
            RegistShaderParam<float>("roughness", "Roughness");
            RegistShaderParam<Color4>("f0", "F0");
            RegistShaderParam<Color4>("uniqueColor", "UniqueColor");
            RegistShaderParam<Color4>("emissive", "Emissive");
            RegistShaderParam<Color4>("multColor", "MultColor");

            //RegistShaderParam<Vector3>("wCamDir", "WorldCameraDir");
            RegistShaderParam<Vector3>("wCamPos", "WorldCameraPosition");
            RegistShaderParam<Vector2>("resolutionInverse", "ResolutionInverse");
            //RegistShaderParam<float>("deltaTime", "DeltaTime");

            RegistShaderParam("albedoMap", "AlbedoMap", TextureUnit.Texture0);
            RegistShaderParam("normalMap", "NormalMap", TextureUnit.Texture1);
            //RegistShaderParam("shadowMap1", "ShadowMap1", TextureUnit.Texture3);
            RegistShaderParam("shadowMap2", "ShadowMap2", TextureUnit.Texture4);
            RegistShaderParam("shadowMap3", "ShadowMap3", TextureUnit.Texture5);

            whiteMap = MMW.GetAsset<Texture2D>("WhiteMap");
            defaultNormalMap = MMW.GetAsset<Texture2D>("DefaultNormalMap");
        }

        public override Result Load()
        {
            var res = base.Load();
            if (res == Result.Success)
            {
                loc_m = GetUniformLocation("M");
                loc_mit = GetUniformLocation("MIT");
                loc_mvp = GetUniformLocation("MVP");
                loc_oldmvp = GetUniformLocation("OldMVP");
                //loc_camDir = GetUniformLocation("wCamDir");
                loc_camPos = GetUniformLocation("wCamPos");
                loc_resolution = GetUniformLocation("resolutionInverse");
                //loc_deltaTime = GetUniformLocation("deltaTime");

                //loc_shadowMV1 = GetUniformLocation("shadowMV1");
                loc_shadowMV2 = GetUniformLocation("shadowMV2");
                loc_shadowMV3 = GetUniformLocation("shadowMV3");
            }
            return res;
        }

        internal override void InitMaterialParameter(Material mat)
        {
            if (!mat.HasParam<Texture2D>("albedoMap")) mat.AddParam("albedoMap", whiteMap);
            if (!mat.HasParam<Texture2D>("normalMap")) mat.AddParam("normalMap", defaultNormalMap);

            if (mat.GetParam<Texture2D>("albedoMap") == null) mat.SetParam("albedoMap", whiteMap);
            if (mat.GetParam<Texture2D>("normalMap") == null) mat.SetParam("normalMap", defaultNormalMap);
        }

        public override void SetUniqueParameter(ShaderUniqueParameter param, bool global)
        {
            if (!global)
            {
                //var mvi = (param.world * param.view).Inverted();
                var mi = param.world.Inverted();
                var mvp = param.world * param.viewProj;
                var oldmvp = param.oldWorld * param.oldViewProj;
                SetParameter(loc_m, ref param.world, false);
                SetParameter(loc_mit, ref mi, true);
                SetParameter(loc_mvp, ref mvp, false);
                SetParameter(loc_oldmvp, ref oldmvp, false);
                //SetParameter(loc_shadowMV1, param.world * param.shadowDepthBias1, false);
                SetParameter(loc_shadowMV2, param.world * param.shadowDepthBias2, false);
                SetParameter(loc_shadowMV3, param.world * param.shadowDepthBias3, false);
            }
            else
            {
                SetParameter(loc_camPos, ref param.cameraPos);
                //SetParameter(loc_camDir, ref param.cameraDir);
                SetParameter(loc_resolution, MMW.RenderResolution.ToVector2().Inverse());
                //SetParameter(loc_deltaTime, param.deltaTime);

                //if (param.shadowDepthMap1 != null) SetParameter(TextureUnit.Texture3, param.shadowDepthMap1);
                //else SetParameter(TextureUnit.Texture3, whiteMap);

                if (param.shadowDepthMap2 != null) SetParameter(TextureUnit.Texture4, param.shadowDepthMap2);
                else SetParameter(TextureUnit.Texture4, whiteMap);

                if (param.shadowDepthMap3 != null) SetParameter(TextureUnit.Texture5, param.shadowDepthMap3);
                else SetParameter(TextureUnit.Texture5, whiteMap);
            }
        }
    }
}
