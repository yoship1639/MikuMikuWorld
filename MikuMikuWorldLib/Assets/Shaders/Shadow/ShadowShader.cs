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
    public class ShadowShader : GLSLShader
    {
        private int loc_m;
        private int loc_mvp;
        private int loc_camPos;

        private int loc_shadowMV1;
        private int loc_shadowMV2;
        private int loc_shadowMV3;

        private Texture2D whiteMap;

        public ShadowShader() : base("Shadow")
        {
            VertexCode = Resources.Shadow_vert;
            FragmentCode = Resources.Shadow_frag;

            RegistShaderParam<Matrix4>("M", "Model");
            RegistShaderParam<Matrix4>("MVP", "ModelViewProjection");
            RegistShaderParam<Matrix4>("shadowMV1", "ShadowModelView1");
            RegistShaderParam<Matrix4>("shadowMV2", "ShadowModelView2");
            RegistShaderParam<Matrix4>("shadowMV3", "ShadowModelView3");

            RegistShaderParam<Vector3>("wCamPos", "WorldCameraPosition");

            RegistShaderParam("shadowMap1", "ShadowMap1", TextureUnit.Texture0);
            RegistShaderParam("shadowMap2", "ShadowMap2", TextureUnit.Texture1);
            RegistShaderParam("shadowMap3", "ShadowMap3", TextureUnit.Texture2);

            RegistShaderParam<float>("shadowAtten", "ShadowAttenuation");

            whiteMap = MMW.GetAsset<Texture2D>("WhiteMap");
        }

        public override Result Load()
        {
            var res = base.Load();
            if (res == Result.Success)
            {
                loc_m = GetUniformLocation("M");
                loc_mvp = GetUniformLocation("MVP");
                loc_camPos = GetUniformLocation("wCamPos");
                loc_shadowMV1 = GetUniformLocation("shadowMV1");
                loc_shadowMV2 = GetUniformLocation("shadowMV2");
                loc_shadowMV3 = GetUniformLocation("shadowMV3");
            }
            return res;
        }

        public override void SetUniqueParameter(ShaderUniqueParameter param, bool global)
        {
            if (!global)
            {
                var mi = param.world.Inverted();
                var mvp = param.world * param.viewProj;
                SetParameter(loc_m, ref param.world, false);
                SetParameter(loc_mvp, ref mvp, false);
                SetParameter(loc_shadowMV1, param.world * param.shadowDepthBias1, false);
                SetParameter(loc_shadowMV2, param.world * param.shadowDepthBias2, false);
                SetParameter(loc_shadowMV3, param.world * param.shadowDepthBias3, false);
            }
            else
            {
                SetParameter(loc_camPos, ref param.cameraPos);

                if (param.shadowDepthMap1 != null) SetParameter(TextureUnit.Texture0, param.shadowDepthMap1);
                else SetParameter(TextureUnit.Texture0, whiteMap);

                if (param.shadowDepthMap2 != null) SetParameter(TextureUnit.Texture1, param.shadowDepthMap2);
                else SetParameter(TextureUnit.Texture1, whiteMap);

                if (param.shadowDepthMap3 != null) SetParameter(TextureUnit.Texture2, param.shadowDepthMap3);
                else SetParameter(TextureUnit.Texture2, whiteMap);
            }
        }
    }
}
