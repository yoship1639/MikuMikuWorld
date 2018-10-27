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
    public class LightCullingShader : GLSLShader
    {
        internal int loc_mvp;
        internal int loc_index;
        internal int loc_resolution;

        public LightCullingShader() : base("Light Culling")
        {
            VertexCode = Resources.LightCulling_vert;
            FragmentCode = Resources.LightCulling_frag;

            RegistShaderParam<Matrix4>("MVP", "ModelViewProjection");
            RegistShaderParam<int>("index", "Index");
            RegistShaderParam<Vector2>("resolution", "Resolution");
        }

        public override Result Load()
        {
            var res = base.Load();
            if (res == Result.Success)
            {
                loc_mvp = GetUniformLocation("MVP");
                loc_index = GetUniformLocation("index");
                loc_resolution = GetUniformLocation("resolution");
            }
            return res;
        }
    }
}
