using MikuMikuWorld.GameComponents;
using MikuMikuWorld.GameComponents.Lights;
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
    public class DeferredBackgroundShader : GLSLShader
    {
        internal int loc_mvp;
        internal int loc_oldmvp;
        internal int loc_albedo;
        internal int loc_fog;
        internal int loc_fogcolor;

        public DeferredBackgroundShader(string name = "Deferred Background") : base(name)
        {
            VertexCode = Resources.DeferredBackground_vert;
            FragmentCode = Resources.DeferredBackground_frag;

            RegistShaderParam<Matrix4>("MVP", "ModelViewProjection");
            RegistShaderParam<Matrix4>("OldMVP", "OldModelViewProjection");

            RegistShaderParam<Color4>("albedo", "Albedo");

            RegistShaderParam("envMap", "EnvironmentMap", TextureUnit.Texture0);
            RegistShaderParam<float>("fogStrength");
            RegistShaderParam<Color4>("fogColor");
        }

        public override Result Load()
        {
            var res = base.Load();
            if (res == Result.Success)
            {
                loc_mvp = GetUniformLocation("MVP");
                loc_oldmvp = GetUniformLocation("OldMVP");
                loc_albedo = GetUniformLocation("albedo");
                loc_fog = GetUniformLocation("fogStrength");
                loc_fogcolor = GetUniformLocation("fogColor");
            }
            return res;
        }

        public override void SetUniqueParameter(ShaderUniqueParameter param, bool global)
        {
            if (!global)
            {
                var mvp = param.world * param.viewProj;
                var oldmvp = param.oldWorld * param.oldViewProj;
                SetParameter(loc_mvp, ref mvp, false);
                SetParameter(loc_oldmvp, ref oldmvp, false);
            }
        }
    }
}
