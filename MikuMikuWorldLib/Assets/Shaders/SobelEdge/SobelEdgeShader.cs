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
    public class SobelEdgeShader : GLSLShader
    {
        internal int loc_resolution;
        internal int loc_mvp;
        internal int loc_edgeWidth;

        public SobelEdgeShader() : base("Sobel Edge")
        {
            VertexCode = Resources.ImageEffect_vert;
            FragmentCode = Resources.SobelEdge_frag;

            RegistShaderParam("samplerSrc", TextureUnit.Texture0);
            RegistShaderParam("samplerColor", TextureUnit.Texture1);
            RegistShaderParam<Vector2>("resolutionInverse", "ResolutionInverse");
            RegistShaderParam<Matrix4>("MVP", "ModelViewProjection");
            RegistShaderParam<float>("edgeWidth");
        }

        public override Result Load()
        {
            var res = base.Load();
            if (res == Result.Success)
            {
                loc_mvp = GetUniformLocation("MVP");
                loc_resolution = GetUniformLocation("resolutionInverse");
                loc_edgeWidth = GetUniformLocation("edgeWidth");
            }
            return res;
        }
    }
}
