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
    public class DoF : ImageEffect
    {
        public RenderTexture RenderTexture { get; set; }
        public Camera Camera { get; set; }

        public Texture2D DepthTexture { get; set; }
        public Texture2D BlurTexture { get; set; }
        public int Reduct
        {
            get { return blur.Reduct; }
            set { blur.Reduct = value; }
        }
        public float Radius
        {
            get { return blur.Radius; }
            set { blur.Radius = value; }
        }
        public float BaseDepth { get; set; } = 0.0f;
        public float StartDist { get; set; } = 0.0f;
        public float TransDist { get; set; } = 60.0f;

        private RenderTexture blurRT;
        private RenderTexture renderTexture;
        private DoFShader dofShader;
        private Matrix4 orthoMatrix;
        private Blur blur;

        public DoF() { }
        public DoF(float radius, float baseDepth, float startDist, float transDist)
        {
            Radius = radius;
            BaseDepth = baseDepth;
            StartDist = startDist;
            TransDist = transDist;
        }

        protected internal override void OnLoad()
        {
            base.OnLoad();

            renderTexture = new RenderTexture(MMW.RenderResolution);
            renderTexture.MagFilter = TextureMagFilter.Linear;
            renderTexture.MinFilter = TextureMinFilter.Linear;
            renderTexture.ColorFormat0 = MMW.Configuration.DefaultPixelFormat;
            renderTexture.Load();

            dofShader = (DoFShader)MMW.GetAsset<Shader>("DoF");
            if (dofShader == null)
            {
                dofShader = new DoFShader();
                MMW.RegistAsset(dofShader);
            }

            if (GameObject != null)
            {
                Camera = GameObject.GetComponent<Camera>();
                RenderTexture = Camera.TargetTexture;
            }

            blurRT = new RenderTexture(MMW.RenderResolution);
            blurRT.ColorFormat0 = MMW.Configuration.DefaultPixelFormat;
            blurRT.Load();

            blur = new Blur(4.0f, 2);
            blur.OnLoad();
            blur.RenderTexture = blurRT;

            orthoMatrix = Matrix4.CreateOrthographicOffCenter(-1, 1, -1, 1, -1, 1);
        }

        public override void Draw(double deltaTime)
        {
            if (DepthTexture == null) return;
            RenderTexture rt = RenderTexture;
            if (rt == null)
            {
                if (Camera == null) return;
                else rt = Camera.TargetTexture;
            }
            if (!rt.Loaded) return;

            if (BlurTexture == null)
            {
                blurRT.Bind(Color4.White);
                Drawer.DrawTexture(rt.ColorDst0);
                blur.Draw(deltaTime);
            }

            renderTexture.Bind(Color4.White);
            dofShader.UseShader();
            dofShader.SetParameter(dofShader.loc_resolution, MMW.RenderResolution.ToVector2().Inverse());
            dofShader.SetParameter(dofShader.loc_mvp, ref orthoMatrix, false);
            dofShader.SetParameter(dofShader.loc_nearFar, new Vector2(Camera.Near, Camera.Far));
            dofShader.SetParameter(dofShader.loc_baseDepth, BaseDepth);
            dofShader.SetParameter(dofShader.loc_startDist, StartDist);
            dofShader.SetParameter(dofShader.loc_transDist, TransDist);
            dofShader.SetParameter(TextureUnit.Texture0, rt.ColorDst0);
            dofShader.SetParameter(TextureUnit.Texture1, blurRT.ColorDst0);
            dofShader.SetParameter(TextureUnit.Texture2, DepthTexture);
            Drawer.DrawTextureMesh();
            dofShader.UnuseShader();

            rt.Bind();
            Drawer.DrawTexture(renderTexture.ColorDst0);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        private void ResizeRenderTexture()
        {
            renderTexture.Size = MMW.RenderResolution;
            blurRT.Size = MMW.RenderResolution;
            blur.ResizeRenderTexture();
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
