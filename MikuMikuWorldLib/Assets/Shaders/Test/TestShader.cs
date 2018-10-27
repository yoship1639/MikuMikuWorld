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
    public class TestShader : GLSLShader
    {
        private Texture2D whiteMap;

        private int loc_mvp;
        private int loc_mit;

        public TestShader() : base("Test")
        {
            VertexCode = Resources.Test_vert;
            FragmentCode = Resources.Test_frag;

            RegistShaderParam<Matrix4>("MVP", "ModelViewProjection");
            RegistShaderParam<Matrix4>("MIT", "ModelInverseTranspose");
            RegistShaderParam<Vector3>("lightDir", "DirectionalLightDir");
            RegistShaderParam<Color4>("diffuse", "Diffuse");
            RegistShaderParam<Color4>("ambient", "Ambient");
            RegistShaderParam("albedoMap", "AlbedoMap", TextureUnit.Texture0);

            whiteMap = MMW.GetAsset<Texture2D>("WhiteMap");
        }

        internal override void InitMaterialParameter(Material mat)
        {
            if (!mat.HasParam<Texture2D>("albedoMap")) mat.AddParam("albedoMap", whiteMap);
        }

        public override Result Load()
        {
            var res = base.Load();
            if (res == Result.Success)
            {
                loc_mvp = GetUniformLocation("MVP");
                loc_mit = GetUniformLocation("MIT");
            }
            return res;
        }

        public override void SetUniqueParameter(ShaderUniqueParameter param, bool global)
        {
            if (!global)
            {
                var mi = param.world.Inverted();
                var mvp = param.world * param.viewProj;
                SetParameter(loc_mvp, ref mvp, false);
                SetParameter(loc_mit, ref mi, true);
                SetParameterByName("lightDir", param.dirLight.WorldDirection);
            }
        }
    }
}
