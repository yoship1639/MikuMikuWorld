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
    public class SSDOBlurShader : GLSLShader
    {
        internal int loc_mvp;
        internal int loc_resolution;
        internal int loc_nearFar;

        internal int loc_strength;

        public SSDOBlurShader() : base("SSDO Blur")
        {
            VertexCode = Resources.ImageEffect_vert;
            FragmentCode = Resources.SSDOBlur_frag;

            RegistShaderParam<Vector2>("resolutionInverse", "ResolutionInverse");
            RegistShaderParam<Matrix4>("MVP", "ModelViewProjection");

            RegistShaderParam("samplerSrc", TextureUnit.Texture0);
            RegistShaderParam("samplerAO", TextureUnit.Texture1);
            RegistShaderParam<Vector2>("nearFar", "NearFar");
            RegistShaderParam<float>("strength");
        }

        public override Result Load()
        {
            var res = base.Load();
            if (res == Result.Success)
            {
                loc_mvp = GetUniformLocation("MVP");
                loc_resolution = GetUniformLocation("resolutionInverse");
                loc_nearFar = GetUniformLocation("nearFar");
                loc_strength = GetUniformLocation("strength");
            }
            return res;
        }
    }
}
