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
    public class ErrorShader : GLSLShader
    {
        private int loc_color;
        private int loc_mvp;

        private Color4 color = new Color4(1.0f, 0.0f, 1.0f, 1.0f);

        public ErrorShader() : base("Error")
        {
            VertexCode = Resources.SolidColor_vert;
            FragmentCode = Resources.SolidColor_frag;

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
                var mi = param.world.Inverted();
                var mvp = param.world * param.viewProj;
                SetParameter(loc_mvp, ref mvp, false);
            }
            else SetParameter(loc_color, ref color);
        }
    }
}
