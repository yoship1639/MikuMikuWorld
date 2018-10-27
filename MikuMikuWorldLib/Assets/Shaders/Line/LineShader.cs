using MikuMikuWorld.Properties;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Assets.Shaders
{
    public class LineShader : GLSLShader
    {
        internal int loc_mvp;
        internal int loc_color;

        public LineShader() : base("Line")
        {
            this.VertexCode = Resources.Line_vert;
            this.FragmentCode = Resources.Line_frag;

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
    }
}
