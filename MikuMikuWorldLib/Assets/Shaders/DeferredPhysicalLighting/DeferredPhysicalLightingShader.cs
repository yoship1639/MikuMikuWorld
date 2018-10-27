using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikuMikuWorld.Properties;
using OpenTK.Graphics.OpenGL4;
using OpenTK;
using OpenTK.Graphics;
using MikuMikuWorld.GameComponents.Lights;

namespace MikuMikuWorld.Assets.Shaders
{
    class DeferredPhysicalLightingShader : GLSLShader
    {
        internal int loc_mvp;
        internal int loc_resolution;
        internal int loc_resolutionInv;
        internal int loc_nearfar;
        internal int loc_camDir;
        internal int loc_camPos;
        internal int loc_gAmbient;
        internal int loc_ibl;
        internal int loc_fog;
        internal int loc_fogcolor;
        internal LightLocation loc_dirLight;

        internal class LightLocation
        {
            public int dir;
            public int color;
            public int intensity;
        }

        public DeferredPhysicalLightingShader() : base("Deferred Physical Lighting")
        {
            VertexCode = Resources.ImageEffect_vert;
            FragmentCode = Resources.DeferredPhysicalLighting_frag;

            RegistShaderParam<Matrix4>("MVP", "ModelViewProjection");

            RegistShaderParam("samplerAlbedo", TextureUnit.Texture0);
            RegistShaderParam("samplerWorldPos", TextureUnit.Texture1);
            RegistShaderParam("samplerWorldNormal", TextureUnit.Texture2);
            RegistShaderParam("samplerPhysicalParams", TextureUnit.Texture3);
            RegistShaderParam("samplerF0", TextureUnit.Texture4);
            RegistShaderParam("samplerDepth", TextureUnit.Texture5);
            RegistShaderParam("samplerShadow", TextureUnit.Texture6);
            RegistShaderParam("samplerVelocity", TextureUnit.Texture7);
            RegistShaderParam("samplerEnv", "EnvironmentMap", TextureUnit.Texture8);

            RegistShaderParam<Vector2>("resolution", "Resolution");
            RegistShaderParam<Vector2>("resolutionInverse", "ResolutionInverse");
            RegistShaderParam<Vector2>("nearFar", "NearFar");
            RegistShaderParam<Vector3>("wCamDir", "WorldCameraDir");
            RegistShaderParam<Vector3>("wCamPos", "WorldCameraPosition");
            RegistShaderParam<Color4>("gAmbient", "GlobalAmbient");
            RegistShaderParam<float>("iblIntensity", "IBLIntensity");
            RegistShaderParam<float>("fogIntensity", "FogIntensity");

            RegistShaderParam<Vector3>("wDirLight.dir", "DirectionalLightDir");
            RegistShaderParam<Color4>("wDirLight.color", "DirectionalLightColor");
            RegistShaderParam<float>("wDirLight.intensity", "DirectionalLightIntensity");
        }

        public override Result Load()
        {
            var res = base.Load();
            if (res == Result.Success)
            {
                loc_mvp = GetUniformLocation("MVP");
                loc_resolution = GetUniformLocation("resolution");
                loc_resolutionInv = GetUniformLocation("resolutionInverse");
                loc_nearfar = GetUniformLocation("nearFar");
                loc_camDir = GetUniformLocation("wCamDir");
                loc_camPos = GetUniformLocation("wCamPos");
                loc_gAmbient = GetUniformLocation("gAmbient");
                loc_ibl = GetUniformLocation("iblIntensity");
                loc_fog = GetUniformLocation("fogIntensity");
                loc_fogcolor = GetUniformLocation("fogColor");

                loc_dirLight = new LightLocation()
                {
                    dir = GetUniformLocation("wDirLight.dir"),
                    color = GetUniformLocation("wDirLight.color"),
                    intensity = GetUniformLocation("wDirLight.intensity"),
                };
            }
            return res;
        }

        public override void SetUniqueParameter(ShaderUniqueParameter param, bool global)
        {
            if (global)
            {
                SetParameter(loc_mvp, ref param.ortho, false);

                SetParameter(TextureUnit.Texture0, param.deferredAlbedoMap);
                SetParameter(TextureUnit.Texture1, param.deferredWorldPosMap);
                SetParameter(TextureUnit.Texture2, param.deferredWorldNormalMap);
                SetParameter(TextureUnit.Texture3, param.deferredPhysicalParamsMap);
                SetParameter(TextureUnit.Texture4, param.deferredF0Map);
                SetParameter(TextureUnit.Texture5, param.deferredDepthMap);
                SetParameter(TextureUnit.Texture6, param.deferredShadowMap);
                SetParameter(TextureUnit.Texture8, param.environmentMap);

                SetParameter(loc_resolution, param.resolution);
                SetParameter(loc_resolutionInv, param.resolution.Inverse());
                SetParameter(loc_nearfar, new Vector2(param.camera.Near, param.camera.Far));
                SetParameter(loc_camDir, ref param.cameraDir);
                SetParameter(loc_camPos, ref param.cameraPos);
                SetParameter(loc_gAmbient, MMW.GlobalAmbient);
                SetParameter(loc_ibl, MMW.IBLIntensity);
                SetParameter(loc_fog, MMW.FogIntensity);
                SetParameter(loc_fogcolor, param.camera.ClearColor);

                SetDirectionalLightParameter(loc_dirLight, param.dirLight);
            }
        }

        private void SetDirectionalLightParameter(LightLocation ll, DirectionalLight light)
        {
            SetParameter(ll.dir, light.WorldDirection);
            SetParameter(ll.color, light.Color);
            SetParameter(ll.intensity, light.Intensity);
        }
    }
}
