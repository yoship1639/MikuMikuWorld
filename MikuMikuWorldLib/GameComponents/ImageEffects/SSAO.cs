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
    public class SSAO : ImageEffect
    {
        public override RequireMap[] RequireMaps => new RequireMap[] { RequireMap.Depth };

        public float Radius { get; set; } = 24.0f;
        public float IgnoreDistance { get; set; } = 0.4f;
        public float AttenuationPower { get; set; } = 6.0f;

        public RenderTexture RenderTexture { get; set; }
        public Camera Camera { get; set; }

        private RenderTexture renderTexture;
        private SSAOShader ssaoShader;
        private Matrix4 orthoMatrix;

        public SSAO() { }
        public SSAO(float radius)
        {
            Radius = radius;
        }

        protected internal override void OnLoad()
        {
            base.OnLoad();

            renderTexture = new RenderTexture(MMW.RenderResolution);
            renderTexture.ColorFormat0 = MMW.Configuration.DefaultPixelFormat;
            renderTexture.Load();

            ssaoShader = (SSAOShader)MMW.GetAsset<Shader>("SSAO");
            if (ssaoShader == null)
            {
                ssaoShader = new SSAOShader();
                MMW.RegistAsset(ssaoShader);
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
            RenderTexture rt = RenderTexture;
            if (rt == null)
            {
                if (Camera == null) return;
                else rt = Camera.TargetTexture;
            }
            if (!rt.Loaded) return;
            if (Radius <= 0.0f) return;

            renderTexture.Bind(Color4.White);
            ssaoShader.UseShader();
            ssaoShader.SetParameter(ssaoShader.loc_resolution, renderTexture.Size.ToVector2().Inverse());
            ssaoShader.SetParameter(ssaoShader.loc_radius, Radius);
            ssaoShader.SetParameter(ssaoShader.loc_ignoreDist, IgnoreDistance);
            ssaoShader.SetParameter(ssaoShader.loc_attenPower, AttenuationPower);
            ssaoShader.SetParameter(TextureUnit.Texture0, rt.ColorDst0);
            ssaoShader.SetParameter(TextureUnit.Texture1, DepthMap);
            ssaoShader.SetParameter(ssaoShader.loc_mvp, ref orthoMatrix, false);
            Drawer.DrawTextureMesh();
            ssaoShader.UnuseShader();

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
            return new SSAO()
            {
                Radius = Radius,
                DepthMap = DepthMap,
            };
        }
    }
}
