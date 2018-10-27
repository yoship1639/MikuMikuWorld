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
    public class ColorCollect : ImageEffect
    {
        public RenderTexture RenderTexture { get; set; }
        public Camera Camera { get; set; }

        private int loc_con;
        private int loc_sat;
        private int loc_brt;
        private int loc_resolution;
        private int loc_mvp;

        public float Contrast { get; set; } = 1.0f;
        public float Saturation { get; set; } = 1.0f;
        public float Brightness { get; set; } = 1.0f;

        private RenderTexture renderTexture;
        private GLSLShader ccShader;
        private Matrix4 orthoMatrix;

        public ColorCollect() { }
        public ColorCollect(float contrast, float saturation, float brightness)
        {
            Contrast = contrast;
            Saturation = saturation;
            Brightness = brightness;
        }

        protected internal override void OnLoad()
        {
            base.OnLoad();

            renderTexture = new RenderTexture(MMW.RenderResolution);
            renderTexture.MagFilter = TextureMagFilter.Linear;
            renderTexture.MinFilter = TextureMinFilter.Linear;
            renderTexture.ColorFormat0 = MMW.Configuration.DefaultPixelFormat;
            renderTexture.Load();

            ccShader = (GLSLShader)MMW.GetAsset<Shader>("Color Collect");
            if (ccShader == null)
            {
                ccShader = new ColorCollectShader();
                MMW.RegistAsset(ccShader);
            }

            loc_con = ccShader.GetUniformLocation("contrast");
            loc_sat = ccShader.GetUniformLocation("saturation");
            loc_brt = ccShader.GetUniformLocation("brightness");
            loc_resolution = ccShader.GetUniformLocation("resolutionInverse");
            loc_mvp = ccShader.GetUniformLocation("MVP");

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
            ccShader.UseShader();
            ccShader.SetParameter(loc_resolution, MMW.RenderResolution.ToVector2().Inverse());
            ccShader.SetParameter(loc_mvp, ref orthoMatrix, false);
            ccShader.SetParameter(loc_con, Contrast);
            ccShader.SetParameter(loc_sat, Saturation);
            ccShader.SetParameter(loc_brt, Brightness);
            ccShader.SetParameter(TextureUnit.Texture0, rt.ColorDst0);
            Drawer.DrawTextureMesh();
            ccShader.UnuseShader();

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
            return new ColorCollect()
            {
                Contrast = Contrast,
                Saturation = Saturation,
                Brightness = Brightness,
            };
        }
    }
}
