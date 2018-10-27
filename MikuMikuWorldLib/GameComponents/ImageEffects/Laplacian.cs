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
    public class Laplacian : ImageEffect
    {
        public RenderTexture RenderTexture { get; set; }
        public Camera Camera { get; set; }

        private RenderTexture renderTexture;
        private LaplacianShader laplacianShader;
        private Matrix4 orthoMatrix;

        public Laplacian() { }

        protected internal override void OnLoad()
        {
            base.OnLoad();

            renderTexture = new RenderTexture(MMW.RenderResolution);
            renderTexture.MagFilter = TextureMagFilter.Linear;
            renderTexture.MinFilter = TextureMinFilter.Linear;
            renderTexture.ColorFormat0 = MMW.Configuration.DefaultPixelFormat;
            renderTexture.Load();

            laplacianShader = (LaplacianShader)MMW.GetAsset<Shader>("Laplacian");
            if (laplacianShader == null)
            {
                laplacianShader = new LaplacianShader();
                MMW.RegistAsset(laplacianShader);
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

            renderTexture.Bind(Color4.White);
            laplacianShader.UseShader();
            laplacianShader.SetParameter(laplacianShader.loc_resolution, MMW.RenderResolution.ToVector2().Inverse());
            laplacianShader.SetParameter(laplacianShader.loc_mvp, ref orthoMatrix, false);
            laplacianShader.SetParameter(TextureUnit.Texture0, rt.ColorDst0);
            Drawer.DrawTextureMesh();
            laplacianShader.UnuseShader();

            rt.Bind();
            Drawer.DrawTexture(renderTexture.ColorDst0);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        private void ResizeRenderTexture()
        {
            renderTexture.Size = MMW.RenderResolution;
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

            renderTexture.Unload();
        }

        public override GameComponent Clone()
        {
            return new DoF();
        }
    }
}
