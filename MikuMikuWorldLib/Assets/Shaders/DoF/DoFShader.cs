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
    public class DoFShader : GLSLShader
    {
        internal int loc_resolution;
        internal int loc_nearFar;
        internal int loc_baseDepth;
        internal int loc_startDist;
        internal int loc_transDist;
        internal int loc_mvp;

        public DoFShader() : base("DoF")
        {
            VertexCode = Resources.ImageEffect_vert;
            FragmentCode = Resources.DoF_frag;

            RegistShaderParam("samplerSrc", TextureUnit.Texture0);
            RegistShaderParam("samplerBlur", TextureUnit.Texture1);
            RegistShaderParam("samplerDepth", TextureUnit.Texture2);
            RegistShaderParam<Vector2>("resolutionInverse", "ResolutionInverse");
            RegistShaderParam<Vector2>("nearFar", "NearFar");
            RegistShaderParam<float>("baseDepth");
            RegistShaderParam<float>("startDist");
            RegistShaderParam<float>("transDist");
            RegistShaderParam<Matrix4>("MVP", "ModelViewProjection");
        }

        public override Result Load()
        {
            var res = base.Load();
            if (res == Result.Success)
            {
                loc_mvp = GetUniformLocation("MVP");
                loc_resolution = GetUniformLocation("resolutionInverse");
                loc_nearFar = GetUniformLocation("nearFar");
                loc_baseDepth = GetUniformLocation("baseDepth");
                loc_startDist = GetUniformLocation("startDist");
                loc_transDist = GetUniformLocation("transDist");
            }
            return res;
        }
    }
}
