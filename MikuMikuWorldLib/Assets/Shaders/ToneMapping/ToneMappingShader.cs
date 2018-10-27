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
    public class ToneMappingShader : GLSLShader
    {
        internal int loc_resolution;
        internal int loc_intensity;
        internal int loc_mvp;
        internal int loc_rate;

        public ToneMappingShader() : base("Tone Mapping")
        {
            VertexCode = Resources.ImageEffect_vert;
            FragmentCode = Resources.ToneMapping_frag;

            RegistShaderParam("samplerSrc", TextureUnit.Texture0);
            RegistShaderParam("samplerInfo", TextureUnit.Texture1);
            RegistShaderParam<float>("intensity");
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
                loc_intensity = GetUniformLocation("intensity");
                loc_rate = GetUniformLocation("rate");
            }
            return res;
        }
    }
}
