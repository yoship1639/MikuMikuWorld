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
    public class PhysicalShader : GLSLShader
    {
        private Texture2D whiteMap;
        private Texture2D defaultNormalMap;
        private TextureCube environmentMap;

        private int loc_m;
        private int loc_mit;
        private int loc_mvp;
        private int loc_camDir;
        private int loc_camPos;
        private int loc_resolution;
        private LightLocation loc_dirLight;
        private LightLocation[] loc_pointLights;
        private LightLocation[] loc_spotLights;
        private int loc_gAmbient;
        private int loc_ibl;
        private int loc_shadowMV1;
        private int loc_shadowMV2;
        private int loc_shadowMV3;

        private int loc_skinning;
        private int loc_morphing;

        private class LightLocation
        {
            public int dir;
            public int color;
            public int intensity;
            public int radius;
            public int pos;
            public int min;
            public int max;
            public int innerAngle;
            public int outerAngle;
        }

        public int PointLightNum { get; set; } = 3;
        public int SpotLightNum { get; set; } = 3;

        public PhysicalShader(string name = "Physical") : base(name)
        {
            VertexCode = Resources.Physical_vert;
            FragmentCode = Resources.Physical_frag;

            FragmentCode = FragmentCode.Replace("replace(POINT_LIGHT_NUM)", "#define POINT_LIGHT_NUM " + PointLightNum);
            FragmentCode = FragmentCode.Replace("replace(SPOT_LIGHT_NUM)", "#define SPOT_LIGHT_NUM " + SpotLightNum);

            RegistShaderParam<int>("skinning");
            RegistShaderParam<int>("morphing");

            RegistShaderParam<Matrix4>("M", "Model");
            RegistShaderParam<Matrix4>("MIT", "ModelInverseTranspose");
            RegistShaderParam<Matrix4>("MVP", "ModelViewProjection");
            RegistShaderParam<Matrix4>("shadowMV1", "ShadowModelView1");
            RegistShaderParam<Matrix4>("shadowMV2", "ShadowModelView2");
            RegistShaderParam<Matrix4>("shadowMV3", "ShadowModelView3");

            RegistShaderParam<Color4>("albedo", "Albedo");
            RegistShaderParam<Color4>("emissive", "Emissive");
            RegistShaderParam<float>("metallic", "Metallic");
            RegistShaderParam<float>("roughness", "Roughness");
            RegistShaderParam<Color4>("f0", "F0");

            RegistShaderParam<Vector3>("wCamDir", "WorldCameraDir");
            RegistShaderParam<Vector3>("wCamPos", "WorldCameraPosition");
            RegistShaderParam<Vector2>("resolutionInverse", "ResolutionInverse");

            RegistShaderParam<Vector3>("wDirLight.dir", "DirectionalLightDir");
            RegistShaderParam<Color4>("wDirLight.color", "DirectionalLightColor");
            RegistShaderParam<float>("wDirLight.intensity", "DirectionalLightIntensity");
            RegistShaderParam<float>("wDirLight.radius", "DirectionalLightRadius");
            RegistShaderParam<Vector3>("wDirLight.pos", "DirectionalLightPosition");

            for (var i = 0; i < PointLightNum; i++)
            {
                RegistShaderParam<Color4>(string.Format("wPointLight[{0}].color", i));
                RegistShaderParam<float>(string.Format("wPointLight[{0}].intensity", i));
                RegistShaderParam<float>(string.Format("wPointLight[{0}].radius", i));
                RegistShaderParam<Vector3>(string.Format("wPointLight[{0}].pos", i));
                RegistShaderParam<Vector3>(string.Format("wPointLight[{0}].min", i));
                RegistShaderParam<Vector3>(string.Format("wPointLight[{0}].max", i));
            }

            for (var i = 0; i < SpotLightNum; i++)
            {
                RegistShaderParam<Vector3>(string.Format("wSpotLight[{0}].dir", i));
                RegistShaderParam<Color4>(string.Format("wSpotLight[{0}].color", i));
                RegistShaderParam<float>(string.Format("wSpotLight[{0}].intensity", i));
                RegistShaderParam<float>(string.Format("wSpotLight[{0}].radius", i));
                RegistShaderParam<Vector3>(string.Format("wSpotLight[{0}].pos", i));
                RegistShaderParam<Vector3>(string.Format("wSpotLight[{0}].min", i));
                RegistShaderParam<Vector3>(string.Format("wSpotLight[{0}].max", i));
                RegistShaderParam<float>(string.Format("wSpotLight[{0}].innerAngle", i));
                RegistShaderParam<float>(string.Format("wSpotLight[{0}].outerAngle", i));
            }

            RegistShaderParam<Color4>("gAmbient", "GlobalAmbient");
            RegistShaderParam<float>("iblIntensity", "IBLIntensity");

            RegistShaderParam("albedoMap", "AlbedoMap", TextureUnit.Texture0);
            RegistShaderParam("normalMap", "NormalMap", TextureUnit.Texture1);
            RegistShaderParam("environmentMap", "EnvironmentMap", TextureUnit.Texture2);
            RegistShaderParam("shadowMap1", "ShadowMap1", TextureUnit.Texture3);
            RegistShaderParam("shadowMap2", "ShadowMap2", TextureUnit.Texture4);
            RegistShaderParam("shadowMap3", "ShadowMap3", TextureUnit.Texture5);

            whiteMap = MMW.GetAsset<Texture2D>("WhiteMap");
            defaultNormalMap = MMW.GetAsset<Texture2D>("DefaultNormalMap");
            environmentMap = MMW.GetAsset<TextureCube>("DefaultSkyBox");
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
                loc_resolution = GetUniformLocation("resolutionInverse");
                loc_gAmbient = GetUniformLocation("gAmbient");
                loc_ibl = GetUniformLocation("iblIntensity");

                loc_dirLight = new LightLocation()
                {
                    dir = GetUniformLocation("wDirLight.dir"),
                    color = GetUniformLocation("wDirLight.color"),
                    intensity = GetUniformLocation("wDirLight.intensity"),
                };

                loc_pointLights = new LightLocation[PointLightNum];
                for (var i = 0; i < PointLightNum; i++)
                {
                    loc_pointLights[i] = new LightLocation()
                    {
                        color = GetUniformLocation(string.Format("wPointLights[{0}].color", i)),
                        intensity = GetUniformLocation(string.Format("wPointLights[{0}].intensity", i)),
                        radius = GetUniformLocation(string.Format("wPointLights[{0}].radius", i)),
                        pos = GetUniformLocation(string.Format("wPointLights[{0}].pos", i)),
                        min = GetUniformLocation(string.Format("wPointLights[{0}].min", i)),
                        max = GetUniformLocation(string.Format("wPointLights[{0}].max", i)),
                    };
                }

                loc_spotLights = new LightLocation[SpotLightNum];
                for (var i = 0; i < SpotLightNum; i++)
                {
                    loc_spotLights[i] = new LightLocation()
                    {
                        dir = GetUniformLocation(string.Format("wSpotLights[{0}].dir", i)),
                        color = GetUniformLocation(string.Format("wSpotLights[{0}].color", i)),
                        intensity = GetUniformLocation(string.Format("wSpotLights[{0}].intensity", i)),
                        radius = GetUniformLocation(string.Format("wSpotLights[{0}].radius", i)),
                        pos = GetUniformLocation(string.Format("wSpotLights[{0}].pos", i)),
                        min = GetUniformLocation(string.Format("wSpotLights[{0}].min", i)),
                        max = GetUniformLocation(string.Format("wSpotLights[{0}].max", i)),
                        innerAngle = GetUniformLocation(string.Format("wSpotLights[{0}].innerAngle", i)),
                        outerAngle = GetUniformLocation(string.Format("wSpotLights[{0}].outerAngle", i)),
                    };
                }

                loc_shadowMV1 = GetUniformLocation("shadowMV1");
                loc_shadowMV2 = GetUniformLocation("shadowMV2");
                loc_shadowMV3 = GetUniformLocation("shadowMV3");

                loc_skinning = GetUniformLocation("skinning");
                loc_morphing = GetUniformLocation("morphing");
            }
            return res;
        }

        internal override void InitMaterialParameter(Material mat)
        {
            if (!mat.HasParam<Texture2D>("albedoMap")) mat.AddParam("albedoMap", whiteMap);
            if (!mat.HasParam<Texture2D>("normalMap")) mat.AddParam("normalMap", defaultNormalMap);
            if (!mat.HasParam<TextureCube>("environmentMap")) mat.AddParam("environmentMap", environmentMap);

            if (mat.GetParam<Texture2D>("albedoMap") == null) mat.SetParam("albedoMap", whiteMap);
            if (mat.GetParam<Texture2D>("normalMap") == null) mat.SetParam("normalMap", defaultNormalMap);
            if (mat.GetParam<Texture2D>("environmentMap") == null) mat.SetParam("environmentMap", environmentMap);
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

                if (param.pointLights != null)
                {
                    for (var i = 0; i < PointLightNum; i++)
                    {
                        if (i >= param.pointLights.Count)
                        {
                            SetParameter(loc_pointLights[i].intensity, 0.0f);
                            break;
                        }
                        SetPointLightParameter(loc_pointLights[i], param.pointLights[i]);
                    }
                }

                if (param.spotLights != null)
                {
                    for (var i = 0; i < SpotLightNum; i++)
                    {
                        if (i >= param.spotLights.Count)
                        {
                            SetParameter(loc_spotLights[i].intensity, 0.0f);
                            break;
                        }
                        SetSpotLightParameter(loc_spotLights[i], param.spotLights[i]);
                    }
                }
                
            }
            else
            {
                SetParameter(loc_camPos, ref param.cameraPos);
                SetParameter(loc_camDir, ref param.cameraDir);
                SetParameter(loc_gAmbient, MMW.GlobalAmbient);
                SetParameter(loc_ibl, MMW.IBLIntensity);
                SetParameter(loc_resolution, MMW.RenderResolution.ToVector2().Inverse());

                SetDirectionalLightParameter(loc_dirLight, param.dirLight);

                if (param.environmentMap != null) SetParameter(TextureUnit.Texture2, param.environmentMap);
                else SetParameter(TextureUnit.Texture2, environmentMap);

                if (param.shadowDepthMap1 != null) SetParameter(TextureUnit.Texture3, param.shadowDepthMap1);
                else SetParameter(TextureUnit.Texture3, whiteMap);

                if (param.shadowDepthMap2 != null) SetParameter(TextureUnit.Texture4, param.shadowDepthMap2);
                else SetParameter(TextureUnit.Texture4, whiteMap);

                if (param.shadowDepthMap3 != null) SetParameter(TextureUnit.Texture5, param.shadowDepthMap3);
                else SetParameter(TextureUnit.Texture5, whiteMap);

                SetParameter(loc_skinning, param.skinning ? 1 : 0);
                SetParameter(loc_morphing, param.morphing ? 1 : 0);
            }
        }

        private void SetDirectionalLightParameter(LightLocation ll, DirectionalLight light)
        {
            SetParameter(ll.dir, light.WorldDirection);
            SetParameter(ll.color, light.Color);
            SetParameter(ll.intensity, light.Intensity);
        }

        private void SetPointLightParameter(LightLocation ll, PointLight light)
        {
            SetParameter(ll.pos, light.GameObject.Transform.WorldPosition);
            SetParameter(ll.color, light.Color);
            SetParameter(ll.intensity, light.Intensity);
            SetParameter(ll.radius, light.Radius);
            SetParameter(ll.min, light.ClipBounds.Min);
            SetParameter(ll.max, light.ClipBounds.Max);
        }

        private void SetSpotLightParameter(LightLocation ll, SpotLight light)
        {
            SetParameter(ll.dir, light.WorldDirection);
            SetParameter(ll.pos, light.GameObject.Transform.WorldPosition);
            SetParameter(ll.color, light.Color);
            SetParameter(ll.intensity, light.Intensity);
            SetParameter(ll.radius, light.Radius);
            SetParameter(ll.min, light.ClipBounds.Min);
            SetParameter(ll.max, light.ClipBounds.Max);
            SetParameter(ll.innerAngle, light.InnerDot);
            SetParameter(ll.outerAngle, light.OuterDot);
        }
    }
}
