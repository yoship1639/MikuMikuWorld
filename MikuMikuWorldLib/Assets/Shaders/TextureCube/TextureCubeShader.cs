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
    public class TextureCubeShader : GLSLShader
    {
        internal int loc_con;
        internal int loc_sat;
        internal int loc_brt;
        internal int loc_mvp;
        internal int loc_color;

        public TextureCubeShader() : base("Texture Cube")
        {
            VertexCode = Resources.TextureCube_vert;
            FragmentCode = Resources.TextureCube_frag;

            RegistShaderParam("tex0", TextureUnit.Texture0);
            RegistShaderParam<Color4>("color");
            RegistShaderParam<Matrix4>("MVP", "ModelViewProjection");
            RegistShaderParam<float>("contrast", "Contrast");
            RegistShaderParam<float>("saturation", "Saturation");
            RegistShaderParam<float>("brightness", "Brightness");
        }

        public override Result Load()
        {
            var res = base.Load();
            if (res == Result.Success)
            {
                loc_con = GetUniformLocation("contrast");
                loc_sat = GetUniformLocation("saturation");
                loc_brt = GetUniformLocation("brightness");
                loc_mvp = GetUniformLocation("MVP");
                loc_color = GetUniformLocation("color");
            }
            return res;
        }
    }
}
