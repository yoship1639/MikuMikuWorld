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
    public class ShrinkShader : GLSLShader
    {
        internal int loc_resolution;
        internal int loc_offset;
        internal int loc_mvp;

        public ShrinkShader() : base("Shrink")
        {
            VertexCode = Resources.ImageEffect_vert;
            FragmentCode = Resources.Shrink_frag;

            RegistShaderParam("samplerSrc", TextureUnit.Texture0);
            RegistShaderParam<Vector2>("resolutionInverse", "ResolutionInverse");
            RegistShaderParam<Vector2>("offset");
            RegistShaderParam<Matrix4>("MVP", "ModelViewProjection");
        }

        public override Result Load()
        {
            var res = base.Load();
            if (res == Result.Success)
            {
                loc_mvp = GetUniformLocation("MVP");
                loc_resolution = GetUniformLocation("resolutionInverse");
                loc_offset = GetUniformLocation("offset");
            }
            return res;
        }
    }
}
