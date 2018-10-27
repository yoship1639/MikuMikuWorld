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
    public class FXAAShader : GLSLShader
    {
        internal int loc_mvp;
        internal int loc_resolution;

        public FXAAShader() : base("FXAA")
        {
            VertexCode = Resources.ImageEffect_vert;
            FragmentCode = Resources.FXAA_frag;

            RegistShaderParam("sampler0", TextureUnit.Texture0);
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
