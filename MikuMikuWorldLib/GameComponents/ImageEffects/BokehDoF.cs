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
    public class BokehDoF : ImageEffect
    {
        public override RequireMap[] RequireMaps => new RequireMap[] { RequireMap.Depth };
        public RenderTexture RenderTexture { get; set; }
        public Camera Camera { get; set; }

        public float Focus { get; set; } = 1.6f;
        public float NearRadiusMax { get; set; } = 24.0f;
        public float FarRadiusMax { get; set; } = 12.0f;
        public float NearBias { get; set; } = 24.0f;
        public float FarBias { get; set; } = 0.0f;

        private RenderTexture renderTexture;
        private BokehDoFShader dofShader;
        private Matrix4 orthoMatrix;

        public BokehDoF() { }
        public BokehDoF(float focus)
        {
            Focus = focus;
        }

        protected internal override void OnLoad()
        {
            base.OnLoad();

            renderTexture = new RenderTexture(MMW.RenderResolution);
            renderTexture.MagFilter = TextureMagFilter.Linear;
            renderTexture.MinFilter = TextureMinFilter.Linear;
            renderTexture.WrapMode = TextureWrapMode.MirroredRepeat;
            renderTexture.ColorFormat0 = MMW.Configuration.DefaultPixelFormat;
            renderTexture.Load();

            dofShader = (BokehDoFShader)MMW.GetAsset<Shader>("Bokeh DoF");
            if (dofShader == null)
            {
                dofShader = new BokehDoFShader();
                MMW.RegistAsset(dofShader);
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
            if (DepthMap == null) return;
            if (NearBias <= 0.0f && FarBias <= 0.0f) return;
            RenderTexture rt = RenderTexture;
            if (rt == null)
            {
                if (Camera == null) return;
                else rt = Camera.TargetTexture;
            }
            if (!rt.Loaded) return;

            renderTexture.Bind(Color4.White);
            dofShader.UseShader();
            dofShader.SetParameter(dofShader.loc_resolution, MMW.RenderResolution.ToVector2().Inverse());
            dofShader.SetParameter(dofShader.loc_mvp, ref orthoMatrix, false);
            dofShader.SetParameter(dofShader.loc_focus, Focus);
            dofShader.SetParameter(dofShader.loc_bias, new Vector2(NearBias, FarBias));
            dofShader.SetParameter(dofShader.loc_blurMax, new Vector2(NearRadiusMax, FarRadiusMax));
            dofShader.SetParameter(TextureUnit.Texture0, rt.ColorDst0);
            dofShader.SetParameter(TextureUnit.Texture1, DepthMap);
            Drawer.DrawTextureMesh();
            dofShader.UnuseShader();

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
            return new BokehDoF(Focus);
        }
    }
}
