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
    public class Texture2Shader : GLSLShader
    {
        internal int loc_layer;
        internal int loc_flipY;
        internal int loc_color;
        internal int loc_mvp;

        public Texture2Shader() : base("Texture2")
        {
            VertexCode = Resources.Texture2_vert;
            FragmentCode = Resources.Texture_frag;

            RegistShaderParam("tex0", TextureUnit.Texture0);
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
                loc_layer = GetUniformLocation("layer");
                loc_flipY = GetUniformLocation("flipY");
                loc_color = GetUniformLocation("color");
                loc_mvp = GetUniformLocation("MVP");
            }
            return res;
        }
    }
}
