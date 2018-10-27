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
    public class FXAA : ImageEffect
    {
        public RenderTexture DstRenderTexture { get; set; }
        public Camera Camera { get; set; }
        public PixelInternalFormat PixelFormat { get; set; } = MMW.Configuration.DefaultPixelFormat;

        private RenderTexture renderTexture;
        private FXAAShader fxaaShader;
        private Matrix4 orthoMatrix;

        public FXAA() { }

        protected internal override void OnLoad()
        {
            base.OnLoad();

            renderTexture = new RenderTexture(MMW.RenderResolution);
            renderTexture.MagFilter = TextureMagFilter.Linear;
            renderTexture.MinFilter = TextureMinFilter.Linear;
            renderTexture.ColorFormat0 = PixelFormat;
            renderTexture.Load();

            fxaaShader = (FXAAShader)MMW.GetAsset<Shader>("FXAA");
            if (fxaaShader == null)
            {
                fxaaShader = new FXAAShader();
                MMW.RegistAsset(fxaaShader);
            }

            if (GameObject != null)
            {
                Camera = GameObject.GetComponent<Camera>();
                DstRenderTexture = Camera.TargetTexture;
            }

            orthoMatrix = Matrix4.CreateOrthographicOffCenter(-1, 1, -1, 1, -1, 1);
        }

        public override void Draw(double deltaTime)
        {
            RenderTexture rt = DstRenderTexture;
            if (rt == null)
            {
                if (Camera == null) return;
                else rt = Camera.TargetTexture;
            }
            if (!rt.Loaded) return;

            renderTexture.Bind(Color4.White);
            fxaaShader.UseShader();
            fxaaShader.SetParameterByName("resolutionInverse", renderTexture.Size.ToVector2().Inverse());
            fxaaShader.SetParameter(TextureUnit.Texture0, rt.ColorDst0);
            fxaaShader.SetParameterByName("MVP", ref orthoMatrix, false);
            Drawer.DrawTextureMesh();
            fxaaShader.UnuseShader();

            rt.Bind();
            Drawer.DrawTexture(renderTexture.ColorDst0);
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
            fxaaShader.Unload();
        }

        public override GameComponent Clone()
        {
            return new FXAA();
        }
    }
}
