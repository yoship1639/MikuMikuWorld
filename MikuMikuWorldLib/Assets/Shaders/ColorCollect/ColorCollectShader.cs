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
    public class ColorCollectShader : GLSLShader
    {
        public ColorCollectShader() : base("Color Collect")
        {
            VertexCode = Resources.ImageEffect_vert;
            FragmentCode = Resources.ColorCollect_frag;

            RegistShaderParam("sampler0", TextureUnit.Texture0);
            RegistShaderParam<Vector2>("resolutionInverse", "ResolutionInverse");
            RegistShaderParam<Matrix4>("MVP", "ModelViewProjection");

            RegistShaderParam<float>("contrast", "Contrast");
            RegistShaderParam<float>("brightness", "Brightness");
            RegistShaderParam<float>("saturation", "Saturation");
        }
    }
}
