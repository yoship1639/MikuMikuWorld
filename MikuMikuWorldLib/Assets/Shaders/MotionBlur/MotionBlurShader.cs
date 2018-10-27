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
    public class MotionBlurShader : GLSLShader
    {
        internal int loc_resolution;
        internal int loc_length;
        internal int loc_mvp;

        public MotionBlurShader() : base("Motion Blur")
        {
            VertexCode = Resources.ImageEffect_vert;
            FragmentCode = Resources.MotionBlur_frag;

            RegistShaderParam("samplerSrc", TextureUnit.Texture0);
            RegistShaderParam("samplerVelocity", TextureUnit.Texture1);
            RegistShaderParam("samplerDepth", TextureUnit.Texture2);

            RegistShaderParam<Vector2>("resolutionInverse", "ResolutionInverse");
            RegistShaderParam<float>("length");
            RegistShaderParam<Matrix4>("MVP", "ModelViewProjection");
        }

        public override Result Load()
        {
            var res = base.Load();
            if (res == Result.Success)
            {
                loc_mvp = GetUniformLocation("MVP");
                loc_length = GetUniformLocation("length");
                loc_resolution = GetUniformLocation("resolutionInverse");
            }
            return res;
        }
    }
}
