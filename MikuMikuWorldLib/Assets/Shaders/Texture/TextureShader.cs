using MikuMikuWorld.Properties;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Assets.Shaders
{
    public class TextureShader : GLSLShader
    {
        internal int loc_srcRect;
        internal int loc_dstRect;
        internal int loc_layer;
        internal int loc_flipY;
        internal int loc_color;
        internal int loc_mvp;

        public TextureShader() : base("Texture")
        {
            VertexCode = Resources.Texture_vert;
            FragmentCode = Resources.Texture_frag;

            RegistShaderParam("tex0", TextureUnit.Texture0);
            RegistShaderParam<Vector4>("srcRect");
            RegistShaderParam<Vector4>("dstRect");
            RegistShaderParam<float>("layer");
            RegistShaderParam<float>("flipY");
            RegistShaderParam<Color4>("color");
            RegistShaderParam<Matrix4>("MVP", "ModelViewProjection");
        }

        public override Result Load()
        {
            var res = base.Load();
            if (res == Result.Success)
            {
                loc_srcRect = GetUniformLocation("srcRect");
                loc_dstRect = GetUniformLocation("dstRect");
                loc_layer = GetUniformLocation("layer");
                loc_flipY = GetUniformLocation("flipY");
                loc_color = GetUniformLocation("color");
                loc_mvp = GetUniformLocation("MVP");
            }
            return res;
        }
    }
}
