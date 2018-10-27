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
    public class MotionBlur : ImageEffect
    {
        public override RequireMap[] RequireMaps => new RequireMap[] { RequireMap.Depth, RequireMap.Velocity };
        public float Length { get; set; } = 16.0f;

        public Texture2D SrcTexture { get; set; }
        public RenderTexture RenderTexture { get; set; }
        public Camera Camera { get; set; }
        public PixelInternalFormat PixelFormat { get; set; } = MMW.Configuration.DefaultPixelFormat;

        private RenderTexture renderTexture;
        private MotionBlurShader blurShader;
        private Matrix4 orthoMatrix;

        protected internal override void OnLoad()
        {
            base.OnLoad();

            renderTexture = new RenderTexture(MMW.RenderResolution);
            renderTexture.MagFilter = TextureMagFilter.Linear;
            renderTexture.MinFilter = TextureMinFilter.Linear;
            renderTexture.WrapMode = TextureWrapMode.MirroredRepeat;
            renderTexture.ColorFormat0 = PixelFormat;
            renderTexture.Load();

            blurShader = (MotionBlurShader)MMW.GetAsset<Shader>("Motion Blur");
            if (blurShader == null)
            {
                blurShader = new MotionBlurShader();
                MMW.RegistAsset(blurShader);
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
            if (Length <= 0.0f) return;
            if (VelocityMap == null) return;

            renderTexture.Bind(Color4.Black);

            blurShader.UseShader();
            blurShader.SetParameter(blurShader.loc_resolution, renderTexture.Size.ToVector2().Inverse());
            blurShader.SetParameter(blurShader.loc_length, Length);
            blurShader.SetParameter(blurShader.loc_mvp, ref orthoMatrix, false);
            if (SrcTexture == null) blurShader.SetParameter(TextureUnit.Texture0, rt.ColorDst0);
            else blurShader.SetParameter(TextureUnit.Texture0, SrcTexture);
            blurShader.SetParameter(TextureUnit.Texture1, VelocityMap);
            blurShader.SetParameter(TextureUnit.Texture2, DepthMap);
            Drawer.DrawTextureMesh();
            blurShader.UnuseShader();

            rt.Bind();
            Drawer.DrawTexture(renderTexture.ColorDst0);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void ResizeRenderTexture()
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
            return new MotionBlur() { Length = Length };
        }
    }
}
