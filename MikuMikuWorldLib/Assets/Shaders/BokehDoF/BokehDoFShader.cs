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
    public class BokehDoFShader : GLSLShader
    {
        internal int loc_resolution;
        internal int loc_focus;
        internal int loc_bias;
        internal int loc_blurMax;
        internal int loc_mvp;

        public BokehDoFShader() : base("Bokeh DoF")
        {
            VertexCode = Resources.ImageEffect_vert;
            FragmentCode = Resources.BokehDoF_frag;

            RegistShaderParam("samplerSrc", TextureUnit.Texture0);
            RegistShaderParam("samplerDepth", TextureUnit.Texture1);
            RegistShaderParam<float>("focus");
            RegistShaderParam<Vector2>("bias");
            RegistShaderParam<Vector2>("blurMax");
            RegistShaderParam<Matrix4>("MVP", "ModelViewProjection");
        }

        public override Result Load()
        {
            var res = base.Load();
            if (res == Result.Success)
            {
                loc_mvp = GetUniformLocation("MVP");
                loc_resolution = GetUniformLocation("resolutionInverse");
                loc_focus = GetUniformLocation("focus");
                loc_bias = GetUniformLocation("bias");
                loc_blurMax = GetUniformLocation("blurMax");
            }
            return res;
        }
    }
}
