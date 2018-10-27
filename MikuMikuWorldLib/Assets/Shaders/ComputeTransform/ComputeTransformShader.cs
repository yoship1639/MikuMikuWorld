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
    public class ComputeTransformShader : GLSLShader
    {
        internal int loc_mvp;

        public ComputeTransformShader() : base("Compute Transform")
        {
            ComputeCode = Resources.ComputeTransform_comp;
        }
    }
}
