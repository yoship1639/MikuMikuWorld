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
    public class WireframeShader : GLSLShader
    {
        internal int loc_mvp;
        internal int loc_color;

        public WireframeShader() : base("Wireframe")
        {
            VertexCode = Resources.Wireframe_vert;
            FragmentCode = Resources.Wireframe_frag;

            RegistShaderParam<Matrix4>("MVP", "ModelViewProjection");
            RegistShaderParam<Color4>("color");
        }

        public override Result Load()
        {
            var res = base.Load();
            if (res == Result.Success)
            {
                loc_color = GetUniformLocation("color");
                loc_mvp = GetUniformLocation("MVP");
            }
            return res;
        }
    }
}
