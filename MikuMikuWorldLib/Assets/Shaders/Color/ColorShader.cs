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
    public class ColorShader : GLSLShader
    {
        internal int loc_mvp;
        internal int loc_color;

        public ColorShader() : base("Color")
        {
            VertexCode = Resources.Color_vert;
            FragmentCode = Resources.Color_frag;

            RegistShaderParam<Matrix4>("MVP", "ModelViewProjection");
            RegistShaderParam<Color4>("color", "Color");
        }

        public override Result Load()
        {
            var res = base.Load();
            if (res == Result.Success)
            {
                loc_mvp = GetUniformLocation("MVP");
                loc_color = GetUniformLocation("color");
            }
            return res;
        }

        public override void SetUniqueParameter(ShaderUniqueParameter param, bool global)
        {
            if (!global)
            {
                var mvp = param.world * param.viewProj;
                SetParameter(loc_mvp, ref mvp, false);
            }
        }
    }
}
