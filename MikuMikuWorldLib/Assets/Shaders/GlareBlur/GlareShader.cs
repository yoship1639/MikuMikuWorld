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
    public class GlareLineShader : GLSLShader
    {
        internal int loc_resolution;
        internal int loc_radius;
        internal int loc_direction;
        internal int loc_mvp;

        public GlareLineShader() : base("Glare Line")
        {
            VertexCode = Resources.ImageEffect_vert;
            FragmentCode = Resources.GlareLine_frag;

            RegistShaderParam("sampler0", TextureUnit.Texture0);
            RegistShaderParam<Vector2>("resolutionInverse", "ResolutionInverse");
            RegistShaderParam<float>("radius");
            RegistShaderParam<Vector2>("direction");
            RegistShaderParam<Matrix4>("MVP", "ModelViewProjection");
        }

        public override Result Load()
        {
            var res = base.Load();
            if (res == Result.Success)
            {
                loc_mvp = GetUniformLocation("MVP");
                loc_radius = GetUniformLocation("radius");
                loc_direction = GetUniformLocation("direction");
                loc_resolution = GetUniformLocation("resolutionInverse");
            }
            return res;
        }
    }

    public class GlarePlusShader : GLSLShader
    {
        internal int loc_resolution;
        internal int loc_radius;
        internal int loc_direction;
        internal int loc_mvp;

        public GlarePlusShader() : base("Glare Plus")
        {
            VertexCode = Resources.ImageEffect_vert;
            FragmentCode = Resources.GlarePlus_frag;

            RegistShaderParam("sampler0", TextureUnit.Texture0);
            RegistShaderParam<Vector2>("resolutionInverse", "ResolutionInverse");
            RegistShaderParam<float>("radius");
            RegistShaderParam<Vector2>("direction");
            RegistShaderParam<Matrix4>("MVP", "ModelViewProjection");
        }

        public override Result Load()
        {
            var res = base.Load();
            if (res == Result.Success)
            {
                loc_mvp = GetUniformLocation("MVP");
                loc_radius = GetUniformLocation("radius");
                loc_direction = GetUniformLocation("direction");
                loc_resolution = GetUniformLocation("resolutionInverse");
            }
            return res;
        }
    }

    public class GlareStarShader : GLSLShader
    {
        internal int loc_resolution;
        internal int loc_radius;
        internal int loc_direction;
        internal int loc_mvp;

        public GlareStarShader() : base("Glare Star")
        {
            VertexCode = Resources.ImageEffect_vert;
            FragmentCode = Resources.GlareStar_frag;

            RegistShaderParam("sampler0", TextureUnit.Texture0);
            RegistShaderParam<Vector2>("resolutionInverse", "ResolutionInverse");
            RegistShaderParam<float>("radius");
            RegistShaderParam<Vector2>("direction");
            RegistShaderParam<Matrix4>("MVP", "ModelViewProjection");
        }

        public override Result Load()
        {
            var res = base.Load();
            if (res == Result.Success)
            {
                loc_mvp = GetUniformLocation("MVP");
                loc_radius = GetUniformLocation("radius");
                loc_direction = GetUniformLocation("direction");
                loc_resolution = GetUniformLocation("resolutionInverse");
            }
            return res;
        }
    }
}
