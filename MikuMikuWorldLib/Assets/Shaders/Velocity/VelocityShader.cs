using MikuMikuWorld.Properties;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Assets.Shaders
{
    public class VelocityShader : GLSLShader
    {
        internal int loc_mvp;
        internal int loc_oldmvp;
        internal int loc_mit;
        internal int loc_deltaTime;

        public VelocityShader() : base("Velocity")
        {
            VertexCode = Resources.Velocity_vert;
            FragmentCode = Resources.Velocity_frag;

            RegistShaderParam<Matrix4>("MVP", "ModelViewProjection");
            RegistShaderParam<Matrix4>("OldMVP", "OldModelViewProjection");
            RegistShaderParam<Matrix4>("MIT", "ModelInverseTranspose");
            RegistShaderParam<float>("deltaTime");
        }

        public override Result Load()
        {
            var res = base.Load();
            if (res == Result.Success)
            {
                loc_mvp = GetUniformLocation("MVP");
                loc_oldmvp = GetUniformLocation("OldMVP");
                loc_mit = GetUniformLocation("MIT");
                loc_deltaTime = GetUniformLocation("deltaTime");
            }
            return res;
        }

        public override void SetUniqueParameter(ShaderUniqueParameter param, bool global)
        {
            if (!global)
            {
                var mi = param.world.Inverted();
                var mvp = param.world * param.viewProj;
                var oldMvp = param.oldWorld * param.oldViewProj;
                SetParameter(loc_mit, ref mi, true);
                SetParameter(loc_mvp, ref mvp, false);
                SetParameter(loc_oldmvp, ref oldMvp, false);
            }
            else
            {
                SetParameter(loc_deltaTime, (float)param.deltaTime);
            }
        }
    }
}
