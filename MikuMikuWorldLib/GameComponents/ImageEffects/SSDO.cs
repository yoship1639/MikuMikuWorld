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
    public class SSDO : ImageEffect
    {
        public override RequireMap[] RequireMaps => new RequireMap[]
        {
            RequireMap.Depth,
            RequireMap.Normal,
            RequireMap.Position,
        };

        public float MxLength { get; set; } = 50.0f;
        public float Radius { get; set; } = 0.25f;
        public float RayLength { get; set; } = 1.0f;
        public float AOScatter { get; set; } = 1.0f;
        public float CDM { get; set; } = 0.25f;
        public float Strength { get; set; } = 1.0f;

        public RenderTexture RenderTexture { get; set; }
        public Camera Camera { get; set; }

        private RenderTexture renderTexture;
        private RenderTexture renderTexture2;
        private SSDOShader ssaoShader;
        private SSDOBlurShader blurShader;
        private Matrix4 orthoMatrix;

        public SSDO() { }

        protected internal override void OnLoad()
        {
            base.OnLoad();

            renderTexture = new RenderTexture(MMW.RenderResolution);
            renderTexture.ColorFormat0 = MMW.Configuration.DefaultPixelFormat;
            renderTexture.WrapMode = TextureWrapMode.ClampToBorder;
            renderTexture.Load();

            renderTexture2 = new RenderTexture(MMW.RenderResolution);
            renderTexture2.ColorFormat0 = MMW.Configuration.DefaultPixelFormat;
            renderTexture2.WrapMode = TextureWrapMode.ClampToBorder;
            renderTexture2.Load();

            ssaoShader = (SSDOShader)MMW.GetAsset<Shader>("SSDO");
            if (ssaoShader == null)
            {
                ssaoShader = new SSDOShader();
                MMW.RegistAsset(ssaoShader);
            }

            blurShader = (SSDOBlurShader)MMW.GetAsset<Shader>("SSDO Blur");
            if (blurShader == null)
            {
                blurShader = new SSDOBlurShader();
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
            if (DepthMap == null) return;
            RenderTexture rt = RenderTexture;
            if (rt == null)
            {
                if (Camera == null) return;
                else rt = Camera.TargetTexture;
            }
            if (!rt.Loaded) return;

            renderTexture.Bind(Color4.White);
            ssaoShader.UseShader();
            ssaoShader.SetParameter(ssaoShader.loc_resolution, renderTexture.Size.ToVector2());
            ssaoShader.SetParameter(ssaoShader.loc_resolutionIV, renderTexture.Size.ToVector2().Inverse());
            ssaoShader.SetParameter(ssaoShader.loc_mxlength, MxLength);
            ssaoShader.SetParameter(ssaoShader.loc_radius, Radius);
            ssaoShader.SetParameter(ssaoShader.loc_raylength, RayLength);
            ssaoShader.SetParameter(ssaoShader.loc_aoscatter, AOScatter);
            ssaoShader.SetParameter(ssaoShader.loc_cdm, CDM);
            ssaoShader.SetParameter(ssaoShader.loc_strength, Strength);
            ssaoShader.SetParameter(TextureUnit.Texture0, rt.ColorDst0);
            ssaoShader.SetParameter(TextureUnit.Texture1, DepthMap);
            ssaoShader.SetParameter(TextureUnit.Texture2, PositionMap);
            ssaoShader.SetParameter(TextureUnit.Texture3, NormalMap);
            ssaoShader.SetParameter(ssaoShader.loc_v, Camera.View, false);
            ssaoShader.SetParameter(ssaoShader.loc_mvp, ref orthoMatrix, false);
            ssaoShader.SetParameter(ssaoShader.loc_nearFar, new Vector2(Camera.Near, Camera.Far));
            Drawer.DrawTextureMesh();
            ssaoShader.UnuseShader();
            
            renderTexture2.Bind(Color4.White);
            blurShader.UseShader();
            blurShader.SetParameter(blurShader.loc_resolution, MMW.RenderResolution.ToVector2().Inverse());
            blurShader.SetParameter(blurShader.loc_mvp, ref orthoMatrix, false);
            blurShader.SetParameter(blurShader.loc_strength, 8.0f);
            blurShader.SetParameter(TextureUnit.Texture0, rt.ColorDst0);
            blurShader.SetParameter(TextureUnit.Texture1, renderTexture.ColorDst0);
            Drawer.DrawTextureMesh();
            blurShader.UnuseShader();
            
            rt.Bind();
            Drawer.DrawTexture(renderTexture2.ColorDst0);

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
            return new SSDO()
            {
                DepthMap = DepthMap,
            };
        }
    }
}
