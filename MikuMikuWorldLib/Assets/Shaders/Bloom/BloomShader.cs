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
    public class BloomShader : GLSLShader
    {
        internal int loc_resolution;
        internal int loc_intensity;
        internal int loc_mvp;

        internal Texture2D black;

        public BloomShader() : base("Bloom")
        {
            VertexCode = Resources.ImageEffect_vert;
            FragmentCode = Resources.Bloom_frag;

            RegistShaderParam("samplerSrc", TextureUnit.Texture0);
            RegistShaderParam("samplerBlur1", TextureUnit.Texture1);
            RegistShaderParam("samplerBlur2", TextureUnit.Texture2);
            RegistShaderParam("samplerBlur3", TextureUnit.Texture3);
            RegistShaderParam("samplerBlur4", TextureUnit.Texture4);
            RegistShaderParam("samplerBlur5", TextureUnit.Texture5);
            RegistShaderParam<Vector2>("resolutionInverse", "ResolutionInverse");
            RegistShaderParam<float>("intensity");
            RegistShaderParam<Matrix4>("MVP", "ModelViewProjection");

            black = MMW.GetAsset<Texture2D>("BlackMap");
        }

        public override Result Load()
        {
            var res = base.Load();
            if (res == Result.Success)
            {
                loc_mvp = GetUniformLocation("MVP");
                loc_resolution = GetUniformLocation("resolutionInverse");
                loc_intensity = GetUniformLocation("intensity");
            }
            return res;
        }
    }
}
