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
    public class SSDOShader : GLSLShader
    {
        internal int loc_mvp;
        internal int loc_v;
        internal int loc_resolution;
        internal int loc_resolutionIV;
        internal int loc_nearFar;

        internal int loc_mxlength;
        internal int loc_radius;
        internal int loc_raylength;
        internal int loc_aoscatter;
        internal int loc_cdm;
        internal int loc_strength;

        public SSDOShader() : base("SSDO")
        {
            VertexCode = Resources.ImageEffect_vert;
            FragmentCode = Resources.SSDO_frag;

            RegistShaderParam<float>("radius", "Radius");
            RegistShaderParam<Vector2>("resolution", "Resolution");
            RegistShaderParam<Vector2>("resolutionInverse", "ResolutionInverse");
            RegistShaderParam<Matrix4>("MVP", "ModelViewProjection");
            RegistShaderParam<Matrix4>("V", "View");
            RegistShaderParam("samplerSrc", TextureUnit.Texture0);
            RegistShaderParam("samplerDepth", TextureUnit.Texture1);
            RegistShaderParam("samplerPosition", TextureUnit.Texture2);
            RegistShaderParam("samplerNormal", TextureUnit.Texture3);
            RegistShaderParam<Vector2>("nearFar", "NearFar");
            RegistShaderParam<float>("mxlength");
            RegistShaderParam<float>("radius");
            RegistShaderParam<float>("raylength");
            RegistShaderParam<float>("aoscatter");
            RegistShaderParam<float>("cdm");
            RegistShaderParam<float>("strength");
        }

        public override Result Load()
        {
            var res = base.Load();
            if (res == Result.Success)
            {
                loc_mvp = GetUniformLocation("MVP");
                loc_v = GetUniformLocation("V");
                loc_resolution = GetUniformLocation("resolution");
                loc_resolutionIV = GetUniformLocation("resolutionInverse");
                loc_nearFar = GetUniformLocation("nearFar");

                loc_mxlength = GetUniformLocation("mxlength");
                loc_radius = GetUniformLocation("radius");
                loc_raylength = GetUniformLocation("raylength");
                loc_aoscatter = GetUniformLocation("aoscatter");
                loc_cdm = GetUniformLocation("cdm");
                loc_strength = GetUniformLocation("strength");
            }
            return res;
        }
    }
}
