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
    public class DepthShader : GLSLShader
    {
        private int loc_mvp;
        internal int loc_skin;

        public DepthShader() : base("Depth")
        {
            VertexCode = Resources.Depth_vert;
            FragmentCode = Resources.Depth_frag;

            RegistShaderParam<Matrix4>("MVP", "ModelViewProjection");
            RegistShaderParam<int>("skinFlag");
        }

        public override Result Load()
        {
            var res = base.Load();
            if (res == Result.Success)
            {
                loc_mvp = GetUniformLocation("MVP");
                loc_skin = GetUniformLocation("skinFlag");
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
