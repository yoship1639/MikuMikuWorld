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
    public class SobelEdge : ImageEffect
    {
        public RenderTexture RenderTexture { get; set; }
        public Camera Camera { get; set; }
        public Texture2D ColorTexture { get; set; }

        public float EdgeWidth { get; set; } = 2.0f;
        private RenderTexture renderTexture;
        private SobelEdgeShader shader;
        private Matrix4 orthoMatrix;

        public SobelEdge() { }

        protected internal override void OnLoad()
        {
            base.OnLoad();

            renderTexture = new RenderTexture(MMW.RenderResolution);
            renderTexture.MagFilter = TextureMagFilter.Linear;
            renderTexture.MinFilter = TextureMinFilter.Linear;
            renderTexture.ColorFormat0 = MMW.Configuration.DefaultPixelFormat;
            renderTexture.Load();

            shader = (SobelEdgeShader)MMW.GetAsset<Shader>("Sobel Edge");
            if (shader == null)
            {
                shader = new SobelEdgeShader();
                MMW.RegistAsset(shader);
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
            if (ColorTexture == null) return;
            RenderTexture rt = RenderTexture;
            if (rt == null)
            {
                if (Camera == null) return;
                else rt = Camera.TargetTexture;
            }
            if (!rt.Loaded) return;

            renderTexture.Bind(Color4.White);
            shader.UseShader();
            shader.SetParameter(shader.loc_resolution, MMW.RenderResolution.ToVector2().Inverse());
            shader.SetParameter(shader.loc_mvp, ref orthoMatrix, false);
            shader.SetParameter(shader.loc_edgeWidth, EdgeWidth);
            shader.SetParameter(TextureUnit.Texture0, rt.ColorDst0);
            shader.SetParameter(TextureUnit.Texture1, ColorTexture);
            Drawer.DrawTextureMesh();
            shader.UnuseShader();

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
