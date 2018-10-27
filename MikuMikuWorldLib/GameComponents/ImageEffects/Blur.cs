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
    public class Blur : ImageEffect
    {
        public override bool ComponentDupulication { get { return true; } }

        public float Radius { get; set; } = 12.0f;

        private int reduct = 4;
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

        public Texture2D SrcTexture { get; set; }
        public RenderTexture RenderTexture { get; set; }
        public Camera Camera { get; set; }
        public PixelInternalFormat PixelFormat { get; set; } = MMW.Configuration.DefaultPixelFormat;

        private RenderTexture renderTextureV;
        private RenderTexture renderTextureH;
        private BlurVShader blurVShader;
        private BlurHShader blurHShader;
        private Matrix4 orthoMatrix;

        public Blur() { }
        public Blur(float radius)
        {
            Radius = radius;
        }
        public Blur(float radius, int reduct)
        {
            Radius = radius;
            this.reduct = reduct;
        }

        protected internal override void OnLoad()
        {
            base.OnLoad();

            var size = MMW.RenderResolution.Mul(1.0f / Reduct);
            if (RenderTexture != null) size = RenderTexture.Size.Mul(1.0f / Reduct);

            renderTextureV = new RenderTexture(size);
            renderTextureV.MagFilter = TextureMagFilter.Linear;
            renderTextureV.MinFilter = TextureMinFilter.Linear;
            renderTextureV.WrapMode = TextureWrapMode.Repeat;
            renderTextureV.ColorFormat0 = PixelFormat;
            renderTextureV.Load();

            renderTextureH = new RenderTexture(size);
            renderTextureH.MagFilter = TextureMagFilter.Linear;
            renderTextureH.MinFilter = TextureMinFilter.Linear;
            renderTextureH.WrapMode = TextureWrapMode.Repeat;
            renderTextureH.ColorFormat0 = PixelFormat;
            renderTextureH.Load();

            blurVShader = (BlurVShader)MMW.GetAsset<Shader>("Blur Vertical");
            if (blurVShader == null)
            {
                blurVShader = new BlurVShader();
                MMW.RegistAsset(blurVShader);
            }

            blurHShader = (BlurHShader)MMW.GetAsset<Shader>("Blur Horizontal");
            if (blurHShader == null)
            {
                blurHShader = new BlurHShader();
                MMW.RegistAsset(blurHShader);
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

            renderTextureV.Bind(Color4.Black);
            blurVShader.UseShader();
            blurVShader.SetParameter(blurVShader.loc_resolution, renderTextureV.Size.ToVector2().Inverse());
            blurVShader.SetParameter(blurVShader.loc_radius, Radius / Reduct);
            if (SrcTexture == null) blurVShader.SetParameter(TextureUnit.Texture0, rt.ColorDst0);
            else blurVShader.SetParameter(TextureUnit.Texture0, SrcTexture);
            blurVShader.SetParameter(blurVShader.loc_mvp, ref orthoMatrix, false);
            Drawer.DrawTextureMesh();

            renderTextureH.Bind(Color4.Black);
            blurHShader.UseShader();
            blurHShader.SetParameter(blurHShader.loc_resolution, renderTextureH.Size.ToVector2().Inverse());
            blurHShader.SetParameter(blurHShader.loc_radius, Radius / Reduct);
            blurHShader.SetParameter(TextureUnit.Texture0, renderTextureV.ColorDst0);
            blurHShader.SetParameter(blurHShader.loc_mvp, ref orthoMatrix, false);
            Drawer.DrawTextureMesh();
            blurHShader.UnuseShader();

            rt.Bind();
            Drawer.DrawTexture(renderTextureH.ColorDst0);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void ResizeRenderTexture()
        {
            var size = MMW.RenderResolution.Mul(1.0f / Reduct);
            if (RenderTexture != null) size = RenderTexture.Size.Mul(1.0f / Reduct);
            renderTextureV.Size = size;
            renderTextureH.Size = size;
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

            renderTextureV.Unload();
            renderTextureH.Unload();
        }

        public override GameComponent Clone()
        {
            return new Blur() { Radius = Radius };
        }
    }
}
