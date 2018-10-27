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
    public class SobelShader : GLSLShader
    {
        internal int loc_resolution;
        internal int loc_mvp;

        public SobelShader() : base("Sobel")
        {
            VertexCode = Resources.ImageEffect_vert;
            FragmentCode = Resources.Sobel_frag;

            RegistShaderParam("samplerSrc", TextureUnit.Texture0);
            RegistShaderParam<Vector2>("resolutionInverse", "ResolutionInverse");
            RegistShaderParam<Matrix4>("MVP", "ModelViewProjection");
        }

        public override Result Load()
        {
            var res = base.Load();
            if (res == Result.Success)
            {
                loc_mvp = GetUniformLocation("MVP");
                loc_resolution = GetUniformLocation("resolutionInverse");
            }
            return res;
        }
    }
}
