using MikuMikuWorld.Properties;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Assets.Shaders
{
    public class SSAOShader : GLSLShader
    {
        internal int loc_mvp;
        internal int loc_resolution;
        internal int loc_radius;
        internal int loc_ignoreDist;
        internal int loc_attenPower;

        public SSAOShader() : base("SSAO")
        {
            VertexCode = Resources.ImageEffect_vert;
            FragmentCode = Resources.SSAO_frag;

            RegistShaderParam<float>("radius", "Radius");
            RegistShaderParam<float>("ignoreDist");
            RegistShaderParam<float>("attenPower");
            RegistShaderParam<Vector2>("resolutionInverse", "ResolutionInverse");
            RegistShaderParam<Matrix4>("MVP", "ModelViewProjection");
            RegistShaderParam("samplerSrc", TextureUnit.Texture0);
            RegistShaderParam("samplerDepth", TextureUnit.Texture1);
            
        }

        public override Result Load()
        {
            var res = base.Load();
            if (res == Result.Success)
            {
                loc_mvp = GetUniformLocation("MVP");
                loc_resolution = GetUniformLocation("resolutionInverse");
                loc_radius = GetUniformLocation("radius");
                loc_ignoreDist = GetUniformLocation("ignoreDist");
                loc_attenPower = GetUniformLocation("attenPower");
            }
            return res;
        }
    }
}
