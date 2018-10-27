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
    public class ComputeMorphShader : GLSLShader
    {
        internal int loc_offset;
        internal int loc_size;
        internal int loc_weight;

        public ComputeMorphShader() : base("Compute Morph")
        {
            ComputeCode = Resources.ComputeMorph_comp;

            RegistShaderParam<int[]>("offset");
            RegistShaderParam<int[]>("size");
            RegistShaderParam<Vector4>("weight");
        }

        public override Result Load()
        {
            var res = base.Load();
            if (res == Result.Success)
            {
                loc_offset = GetUniformLocation("offset");
                loc_size = GetUniformLocation("size");
                loc_weight = GetUniformLocation("weight");
            }
            return res;
        }
    }
}
