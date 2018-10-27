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
    public class BlurVShader : GLSLShader
    {
        internal int loc_resolution;
        internal int loc_radius;
        internal int loc_mvp;

        public BlurVShader() : base("Blur Vertical")
        {
            VertexCode = Resources.ImageEffect_vert;
            FragmentCode = Resources.BlurV_frag;

            RegistShaderParam("sampler0", TextureUnit.Texture0);
            RegistShaderParam<Vector2>("resolutionInverse", "ResolutionInverse");
            RegistShaderParam<float>("radius");
            RegistShaderParam<Matrix4>("MVP", "ModelViewProjection");
        }

        public override Result Load()
        {
            var res = base.Load();
            if (res == Result.Success)
            {
                loc_mvp = GetUniformLocation("MVP");
                loc_radius = GetUniformLocation("radius");
                loc_resolution = GetUniformLocation("resolutionInverse");
            }
            return res;
        }
    }

    public class BlurHShader : GLSLShader
    {
        internal int loc_resolution;
        internal int loc_radius;
        internal int loc_mvp;

        public BlurHShader() : base("Blur Horizontal")
        {
            VertexCode = Resources.ImageEffect_vert;
            FragmentCode = Resources.BlurH_frag;

            RegistShaderParam("sampler0", TextureUnit.Texture0);
            RegistShaderParam<Vector2>("resolutionInverse", "ResolutionInverse");
            RegistShaderParam<float>("radius");
            RegistShaderParam<Matrix4>("MVP", "ModelViewProjection");
        }

        public override Result Load()
        {
            var res = base.Load();
            if (res == Result.Success)
            {
                loc_mvp = GetUniformLocation("MVP");
                loc_radius = GetUniformLocation("radius");
                loc_resolution = GetUniformLocation("resolutionInverse");
            }
            return res;
        }
    }
}
