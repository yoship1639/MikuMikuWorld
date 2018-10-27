using MikuMikuWorld.Assets;
using MikuMikuWorld.Assets.Shaders;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.GameComponents.ImageEffects
{
    public class GlareBlur : ImageEffect
    {
        public float Radius { get; set; } = 12.0f;

        private int reduct = 1;
        public int Reduct
        {
            get { return reduct; }
            set
            {
                var prev = reduct;
                reduct = value;
                if (prev != value) ResizeRenderTexture();
            }
        }

        public enum GlareType
        {
            Line = 1,
            Plus = 2,
            Star = 3,
        }

        public Vector2 Direction { get; set; } = Vector2.UnitX;
        public GlareType Glare { get; set; } = GlareType.Line;
        public Texture2D SrcTexture { get; set; }
        public RenderTexture RenderTexture { get; set; }
        public Camera Camera { get; set; }

        private RenderTexture glareRT;
        private GlareLineShader glareLineShader;
        private GlarePlusShader glarePlusShader;
        private GlareStarShader glareStarShader;
        private Matrix4 orthoMatrix;

        public GlareBlur() { }
        public GlareBlur(float radius, Vector2 direction, GlareType glare)
        {
            Radius = radius;
            Direction = direction;
            Glare = glare;
        }

        protected internal override void OnLoad()
        {
            base.OnLoad();

            var size = MMW.RenderResolution.Mul(1.0f / Reduct);
            if (RenderTexture != null) size = RenderTexture.Size.Mul(1.0f / Reduct);

            glareRT = new RenderTexture(size);
            glareRT.MagFilter = TextureMagFilter.Linear;
            glareRT.MinFilter = TextureMinFilter.Linear;
            glareRT.WrapMode = TextureWrapMode.ClampToEdge;
            glareRT.ColorFormat0 = MMW.Configuration.DefaultPixelFormat;
            glareRT.Load();

            glareLineShader = (GlareLineShader)MMW.GetAsset<Shader>("Glare Line");
            if (glareLineShader == null)
            {
                glareLineShader = new GlareLineShader();
                MMW.RegistAsset(glareLineShader);
            }

            glarePlusShader = (GlarePlusShader)MMW.GetAsset<Shader>("Glare Plus");
            if (glarePlusShader == null)
            {
                glarePlusShader = new GlarePlusShader();
                MMW.RegistAsset(glarePlusShader);
            }

            glareStarShader = (GlareStarShader)MMW.GetAsset<Shader>("Glare Star");
            if (glareStarShader == null)
            {
                glareStarShader = new GlareStarShader();
                MMW.RegistAsset(glareStarShader);
            }

            if (GameObject != null)
            {
                Camera = GameObject.GetComponent<Camera>();
                RenderTexture = Camera.TargetTexture;
            }

            orthoMatrix = Matrix4.CreateOrthographicOffCenter(-1, 1, -1, 1, -1, 1);
        }

        public override void Draw(double deltaTime)
        {
            RenderTexture rt = RenderTexture;
            if (rt == null)
            {
                if (Camera == null) return;
                else rt = Camera.TargetTexture;
            } 
            if (!rt.Loaded) return;
            if (Radius <= 0.0f) return;

            glareRT.Bind(Color4.Black);
            if (Glare == GlareType.Line)
            {
                glareLineShader.UseShader();
                glareLineShader.SetParameter(glareLineShader.loc_resolution, glareRT.Size.ToVector2().Inverse());
                glareLineShader.SetParameter(glareLineShader.loc_radius, Radius / Reduct);
                glareLineShader.SetParameter(glareLineShader.loc_direction, Direction);
                glareLineShader.SetParameter(glareLineShader.loc_mvp, ref orthoMatrix, false);
                if (SrcTexture == null) glareLineShader.SetParameter(TextureUnit.Texture0, rt.ColorDst0);
                else glareLineShader.SetParameter(TextureUnit.Texture0, SrcTexture);
            }
            else if (Glare == GlareType.Plus)
            {
                glarePlusShader.UseShader();
                glarePlusShader.SetParameter(glarePlusShader.loc_resolution, glareRT.Size.ToVector2().Inverse());
                glarePlusShader.SetParameter(glarePlusShader.loc_radius, Radius / Reduct);
                glarePlusShader.SetParameter(glarePlusShader.loc_direction, Direction);
                glarePlusShader.SetParameter(glarePlusShader.loc_mvp, ref orthoMatrix, false);
                if (SrcTexture == null) glarePlusShader.SetParameter(TextureUnit.Texture0, rt.ColorDst0);
                else glarePlusShader.SetParameter(TextureUnit.Texture0, SrcTexture);
            }
            else if (Glare == GlareType.Star)
            {
                glareStarShader.UseShader();
                glareStarShader.SetParameter(glareStarShader.loc_resolution, glareRT.Size.ToVector2().Inverse());
                glareStarShader.SetParameter(glareStarShader.loc_radius, Radius / Reduct);
                glareStarShader.SetParameter(glareStarShader.loc_direction, Direction);
                glareStarShader.SetParameter(glareStarShader.loc_mvp, ref orthoMatrix, false);
                if (SrcTexture == null) glareStarShader.SetParameter(TextureUnit.Texture0, rt.ColorDst0);
                else glareStarShader.SetParameter(TextureUnit.Texture0, SrcTexture);
            }
            Drawer.DrawTextureMesh();
            glareLineShader.UnuseShader();

            rt.Bind();
            Drawer.DrawTexture(glareRT.ColorDst0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void ResizeRenderTexture()
        {
            var size = MMW.RenderResolution.Mul(1.0f / Reduct);
            if (RenderTexture != null) size = RenderTexture.Size.Mul(1.0f / Reduct);
            glareRT.Size = size;
        }

        protected internal override void OnReceivedMessage(string message, params object[] args)
        {
            if (message == MMWSettings.Default.Message_WindowResize)
            {
                ResizeRenderTexture();
            }
        }

        protected internal override void OnUnload()
        {
            base.OnUnload();

            glareRT.Unload();
        }

        public override GameComponent Clone()
        {
            return new GlareBlur()
            {
                Radius = Radius,
                Reduct = Reduct,
                Direction = Direction,
                Glare = Glare,
            };
        }
    }
}
