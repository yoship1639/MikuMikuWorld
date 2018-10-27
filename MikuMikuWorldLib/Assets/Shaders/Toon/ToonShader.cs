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
    public class ToonShader : GLSLShader
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

        public ToonShader() : base("Toon")
        {
            VertexCode = Resources.Toon_vert;
            FragmentCode = Resources.Toon_frag;

            RegistShaderParam<Matrix4>("M", "Model");
            RegistShaderParam<Matrix4>("MIT", "ModelInverseTranspose");
            RegistShaderParam<Matrix4>("MVP", "ModelViewProjection");

            RegistShaderParam<Color4>("diffuse", "Diffuse");
            RegistShaderParam<Color4>("specular", "Specular");
            RegistShaderParam<float>("shininess", "Shininess");

            RegistShaderParam<Vector3>("wCamDir", "WorldCameraDir");
            RegistShaderParam<Vector3>("wCamPos", "WorldCameraPosition");

            RegistShaderParam<Vector3>("wDirLight.dir", "DirectionalLightDir");
            RegistShaderParam<Vector3>("wDirLight.color", "DirectionalLightColor");
            RegistShaderParam<Vector3>("wDirLight.intensity", "DirectionalLightIntensity");

            RegistShaderParam<Color4>("gAmbient", "GlobalAmbient");
            

            RegistShaderParam("albedoMap", "AlbedoMap", TextureUnit.Texture0);
            RegistShaderParam("toonMap", "ToonMap", TextureUnit.Texture1);

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
            }
            else
            {
                SetParameter(loc_camPos, ref param.cameraPos);
                SetParameter(loc_camDir, ref param.cameraDir);
                SetParameter(loc_lightDir, param.dirLight.WorldDirection);
                SetParameter(loc_lightColor, param.dirLight.Color);
                SetParameter(loc_lightIntensity, param.dirLight.Intensity);
                SetParameter(loc_gAmbient, MMW.GlobalAmbient);
            }
        }
    }
}
