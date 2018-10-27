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
    public class ToonShadowShader : GLSLShader
    {
        private Texture2D whiteMap;
        private Texture2D toonMap;

        private int loc_m;
        private int loc_mit;
        private int loc_mvp;
        private int loc_camDir;
        private int loc_camPos;
        private int loc_lightDir;
        private int loc_lightColor;
        private int loc_lightIntensity;
        private int loc_gAmbient;
        private int loc_con;
        private int loc_sat;
        private int loc_brt;
        private int loc_shadowMV1;
        private int loc_shadowMV2;
        private int loc_shadowMV3;
        private int loc_uniqueColor;

        public ToonShadowShader() : base("Toon Shadow")
        {
            VertexCode = Resources.ToonShadow_vert;
            FragmentCode = Resources.ToonShadow_frag;

            RegistShaderParam<Matrix4>("M", "Model");
            RegistShaderParam<Matrix4>("MIT", "ModelInverseTranspose");
            RegistShaderParam<Matrix4>("MVP", "ModelViewProjection");
            RegistShaderParam<Matrix4>("shadowMV1", "ShadowModelView1");
            RegistShaderParam<Matrix4>("shadowMV2", "ShadowModelView2");
            RegistShaderParam<Matrix4>("shadowMV3", "ShadowModelView3");

            RegistShaderParam<Color4>("diffuse", "Diffuse");
            RegistShaderParam<Color4>("specular", "Specular");
            RegistShaderParam<float>("shininess", "Shininess");
            RegistShaderParam<Color4>("uniqueColor", "UniqueColor");

            RegistShaderParam<Vector3>("wCamDir", "WorldCameraDir");
            RegistShaderParam<Vector3>("wCamPos", "WorldCameraPosition");

            RegistShaderParam<Vector3>("wDirLight.dir", "DirectionalLightDir");
            RegistShaderParam<Vector3>("wDirLight.color", "DirectionalLightColor");
            RegistShaderParam<Vector3>("wDirLight.intensity", "DirectionalLightIntensity");

            RegistShaderParam<Color4>("gAmbient", "GlobalAmbient");
            RegistShaderParam<float>("limCoeff", "LimLightCoefficient");

            RegistShaderParam<float>("contrast", "Contrast");
            RegistShaderParam<float>("saturation", "Saturation");
            RegistShaderParam<float>("brightness", "Brightness");

            RegistShaderParam("albedoMap", "AlbedoMap", TextureUnit.Texture0);
            RegistShaderParam("toonMap", "ToonMap", TextureUnit.Texture1);
            RegistShaderParam("shadowMap1", "ShadowMap1", TextureUnit.Texture2);
            RegistShaderParam("shadowMap2", "ShadowMap2", TextureUnit.Texture3);
            RegistShaderParam("shadowMap3", "ShadowMap3", TextureUnit.Texture4);

            whiteMap = MMW.GetAsset<Texture2D>("WhiteMap");
            //toonMap = MMW.GetAsset<Texture2D>("ToonMap");
            toonMap = new Texture2D(Resources.toon2);
            toonMap.Load();
        }

        public override Result Load()
        {
            var res = base.Load();
            if (res == Result.Success)
            {
                loc_m = GetUniformLocation("M");
                loc_mit = GetUniformLocation("MIT");
                loc_mvp = GetUniformLocation("MVP");
                loc_camDir = GetUniformLocation("wCamDir");
                loc_camPos = GetUniformLocation("wCamPos");
                loc_lightDir = GetUniformLocation("wDirLight.dir");
                loc_lightColor = GetUniformLocation("wDirLight.color");
                loc_lightIntensity = GetUniformLocation("wDirLight.intensity");
                loc_gAmbient = GetUniformLocation("gAmbient");
                loc_con = GetUniformLocation("contrast");
                loc_sat = GetUniformLocation("saturation");
                loc_brt = GetUniformLocation("brightness");
                loc_shadowMV1 = GetUniformLocation("shadowMV1");
                loc_shadowMV2 = GetUniformLocation("shadowMV2");
                loc_shadowMV3 = GetUniformLocation("shadowMV3");
                loc_uniqueColor = GetUniformLocation("uniqueColor");
            }
            return res;
        }

        internal override void InitMaterialParameter(Material mat)
        {
            if (!mat.HasParam<Texture2D>("albedoMap")) mat.AddParam("albedoMap", whiteMap);
            if (!mat.HasParam<Texture2D>("toonMap")) mat.AddParam("toonMap", toonMap);
        }

        public override void SetUniqueParameter(ShaderUniqueParameter param, bool global)
        {
            if (!global)
            {
                var mi = param.world.Inverted();
                var mvp = param.world * param.viewProj;
                SetParameter(loc_m, ref param.world, false);
                SetParameter(loc_mit, ref mi, true);
                SetParameter(loc_mvp, ref mvp, false);
                SetParameter(loc_shadowMV1, param.world * param.shadowDepthBias1, false);
                SetParameter(loc_shadowMV2, param.world * param.shadowDepthBias2, false);
                SetParameter(loc_shadowMV3, param.world * param.shadowDepthBias3, false);
                SetParameter(loc_uniqueColor, ref param.uniqueColor);
            }
            else
            {
                SetParameter(loc_camPos, ref param.cameraPos);
                SetParameter(loc_camDir, ref param.cameraDir);
                SetParameter(loc_lightDir, param.dirLight.WorldDirection);
                SetParameter(loc_lightColor, param.dirLight.Color);
                SetParameter(loc_lightIntensity, param.dirLight.Intensity);
                SetParameter(loc_gAmbient, MMW.GlobalAmbient);

                if (param.shadowDepthMap1 != null) SetParameter(TextureUnit.Texture2, param.shadowDepthMap1);
                else SetParameter(TextureUnit.Texture2, whiteMap);

                if (param.shadowDepthMap2 != null) SetParameter(TextureUnit.Texture3, param.shadowDepthMap2);
                else SetParameter(TextureUnit.Texture3, whiteMap);

                if (param.shadowDepthMap3 != null) SetParameter(TextureUnit.Texture4, param.shadowDepthMap3);
                else SetParameter(TextureUnit.Texture4, whiteMap);
            }
        }
    }
}
