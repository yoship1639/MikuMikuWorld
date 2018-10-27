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
    public class MulShader : GLSLShader
    {
        public MulShader() : base("Mul")
        {
            VertexCode = Resources.ImageEffect_vert;
            FragmentCode = Resources.Mul_frag;

            RegistShaderParam("sampler0", TextureUnit.Texture0);
            RegistShaderParam("sampler1", TextureUnit.Texture1);
            RegistShaderParam<Vector2>("resolutionInverse", "ResolutionInverse");
            RegistShaderParam<Matrix4>("MVP", "ModelViewProjection");
        }
    }
}
