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
    public class Mul : ImageEffect
    {
        public Camera Camera { get; set; }
        public Texture2D MulTexture { get; set; }

        private RenderTexture renderTexture;
        private Shader mulShader;
        private Matrix4 orthoMatrix;

        public Mul() { }

        protected internal override void OnLoad()
        {
            base.OnLoad();

            renderTexture = new RenderTexture(MMW.RenderResolution);
            renderTexture.MagFilter = TextureMagFilter.Linear;
            renderTexture.MinFilter = TextureMinFilter.Linear;
            renderTexture.ColorFormat0 = MMW.Configuration.DefaultPixelFormat;
            renderTexture.Load();

            mulShader = new MulShader();
            mulShader.Load();

            if (GameObject != null)
            {
                Camera = GameObject.GetComponent<Camera>();
            }

            orthoMatrix = Matrix4.CreateOrthographicOffCenter(-1, 1, -1, 1, -1, 1);
        }

        public override void Draw(double deltaTime)
        {
            if (Camera == null || MulTexture == null) return;
            if (!Camera.TargetTexture.Loaded) return;

            renderTexture.Bind(Color4.White);
            mulShader.UseShader();
            mulShader.SetParameterByName("resolutionInverse", new Vector2(1.0f / MMW.RenderResolution.Width, 1.0f / MMW.RenderResolution.Height));
            mulShader.SetParameter(TextureUnit.Texture0, Camera.TargetTexture.ColorDst0);
            mulShader.SetParameter(TextureUnit.Texture1, MulTexture);
            mulShader.SetParameterByName("MVP", orthoMatrix, false);
            Drawer.DrawTextureMesh();
            mulShader.UnuseShader();

            Camera.TargetTexture.Bind();
            Drawer.DrawTexture(renderTexture.ColorDst0, new RectangleF(0, 0, 1, 1), new RectangleF(0, 0, 1, 1));
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
            mulShader.Unload();
        }

        public override GameComponent Clone()
        {
            return new FXAA();
        }
    }
}
