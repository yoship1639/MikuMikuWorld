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
    public class SSAOUE4Shader : GLSLShader
    {
        internal int loc_mvp;
        internal int loc_resolution;
        internal int loc_radius;
        internal int loc_depthBias;
        internal int loc_strength;
        internal int loc_nearFar;

        public SSAOUE4Shader() : base("SSAO UE4")
        {
            VertexCode = Resources.ImageEffect_vert;
            FragmentCode = Resources.SSAOUE4_frag;

            RegistShaderParam<float>("radius", "Radius");
            RegistShaderParam<float>("depthBias", "DepthBias");
            RegistShaderParam<float>("strength", "Strength");
            RegistShaderParam<Vector2>("resolutionInverse", "ResolutionInverse");
            RegistShaderParam<Matrix4>("MVP", "ModelViewProjection");
            RegistShaderParam("samplerSrc", TextureUnit.Texture0);
            RegistShaderParam("samplerDepth", TextureUnit.Texture1);
            RegistShaderParam<Vector2>("nearFar", "NearFar");
        }

        public override Result Load()
        {
            var res = base.Load();
            if (res == Result.Success)
            {
                loc_mvp = GetUniformLocation("MVP");
                loc_resolution = GetUniformLocation("resolutionInverse");
                loc_radius = GetUniformLocation("radius");
                loc_depthBias = GetUniformLocation("depthBias");
                loc_strength = GetUniformLocation("strength");
                loc_nearFar = GetUniformLocation("nearFar");
            }
            return res;
        }
    }
}
