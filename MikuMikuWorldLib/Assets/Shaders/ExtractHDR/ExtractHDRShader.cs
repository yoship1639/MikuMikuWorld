﻿using MikuMikuWorld.Properties;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Assets.Shaders
{
    public class ExtractHDRShader : GLSLShader
    {
        internal int loc_resolution;
        internal int loc_threshold;
        internal int loc_mvp;

        public ExtractHDRShader() : base("Extract HDR")
        {
            VertexCode = Resources.ImageEffect_vert;
            FragmentCode = Resources.ExtractHDR_frag;

            RegistShaderParam("samplerSrc", TextureUnit.Texture0);
            RegistShaderParam<Vector2>("resolutionInverse", "ResolutionInverse");
            RegistShaderParam<float>("threshold");
            RegistShaderParam<Matrix4>("MVP", "ModelViewProjection");
        }

        public override Result Load()
        {
            var res = base.Load();
            if (res == Result.Success)
            {
                loc_mvp = GetUniformLocation("MVP");
                loc_resolution = GetUniformLocation("resolutionInverse");
                loc_threshold = GetUniformLocation("threshold");
            }
            return res;
        }
    }
}
