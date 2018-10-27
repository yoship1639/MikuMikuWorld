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
    public class Bloom : ImageEffect
    {
        private static readonly int BlurNum = 5;

        public RenderTexture RenderTexture { get; set; }
        public Camera Camera { get; set; }

        public float Intensity { get; set; } = 2.4f;
        public float Threshold { get; set; } = 0.9f;

        private RenderTexture hdrRT;
        private ExtractHDRShader hdrShader;

        public float Radius { get; set; } = 6.0f;
        private RenderTexture[] blurRTs;
        private Blur[] blurs;

        private BloomShader bloomShader;
        private RenderTexture bloomRT;
        private Matrix4 orthoMatrix;

        public Bloom() { }
        public Bloom(float radius = 6.0f, float intensity = 0.4f)
        {
            Radius = radius;
            Intensity = intensity;
        }

        protected internal override void OnLoad()
        {
            base.OnLoad();

            // 書き出しRT
            bloomRT = new RenderTexture(MMW.RenderResolution);
            bloomRT.MagFilter = TextureMagFilter.Linear;
            bloomRT.MinFilter = TextureMinFilter.Linear;
            bloomRT.ColorFormat0 = MMW.Configuration.DefaultPixelFormat;
            bloomRT.Load();

            // HDR抽出RT
            hdrRT = new RenderTexture(MMW.RenderResolution);
            hdrRT.MagFilter = TextureMagFilter.Linear;
            hdrRT.MinFilter = TextureMinFilter.Linear;
            hdrRT.ColorFormat0 = MMW.Configuration.DefaultPixelFormat;
            hdrRT.Load();

            // Blur適用RT
            blurRTs = new RenderTexture[BlurNum];
            blurs = new Blur[BlurNum];
            var blurSizes = new Size[]
            {
                new Size(MMW.RenderResolution.Width / 4, MMW.RenderResolution.Height / 4),
                new Size(MMW.RenderResolution.Width / 8, MMW.RenderResolution.Height / 8),
                new Size(MMW.RenderResolution.Width / 16, MMW.RenderResolution.Height / 16),
                new Size(MMW.RenderResolution.Width / 32, MMW.RenderResolution.Height / 32),
                new Size(MMW.RenderResolution.Width / 64, MMW.RenderResolution.Height / 64),
            };
            for (var i = 0; i < BlurNum; i++)
            {
                var size = blurSizes[i];
                if (size.Width < 128) size.Width = 128;
                if (size.Height < 128) size.Height = 128;

                blurRTs[i] = new RenderTexture(size);
                blurRTs[i].MagFilter = TextureMagFilter.Linear;
                blurRTs[i].MinFilter = TextureMinFilter.Linear;
                blurRTs[i].WrapMode = TextureWrapMode.ClampToEdge;
                blurRTs[i].ColorFormat0 = MMW.Configuration.DefaultPixelFormat;
                blurRTs[i].Load();

                blurs[i] = new Blur(Radius, 1);
                blurs[i].RenderTexture = blurRTs[i];
                blurs[i].OnLoad();
            }

            // HDR抽出シェーダ
            hdrShader = (ExtractHDRShader)MMW.GetAsset<Shader>("Extract HDR");
            if (hdrShader == null)
            {
                hdrShader = new ExtractHDRShader();
                MMW.RegistAsset(hdrShader);
            }

            // Bloomシェーダ
            bloomShader = (BloomShader)MMW.GetAsset<Shader>("Bloom");
            if (bloomShader == null)
            {
                bloomShader = new BloomShader();
                MMW.RegistAsset(bloomShader);
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

            // HDR抽出
            hdrRT.Bind(Color4.Black);
            hdrShader.UseShader();
            hdrShader.SetParameter(hdrShader.loc_resolution, hdrRT.Size.ToVector2().Inverse());
            hdrShader.SetParameter(hdrShader.loc_mvp, ref orthoMatrix, false);
            hdrShader.SetParameter(hdrShader.loc_threshold, Threshold);
            hdrShader.SetParameter(TextureUnit.Texture0, rt.ColorDst0);
            Drawer.DrawTextureMesh();
            hdrShader.UnuseShader();
            
            // HDR画像のぼかし画像を生成
            for (var i = 0; i < BlurNum; i++)
            {
                if (i == 0) blurs[i].SrcTexture = hdrRT.ColorDst0;
                else blurs[i].SrcTexture = blurRTs[i - 1].ColorDst0;
                blurs[i].Draw(deltaTime);
            }

            // Bloom処理
            bloomRT.Bind(Color4.Black);
            bloomShader.UseShader();
            bloomShader.SetParameter(bloomShader.loc_resolution, bloomRT.Size.ToVector2().Inverse());
            bloomShader.SetParameter(bloomShader.loc_mvp, ref orthoMatrix, false);
            bloomShader.SetParameter(bloomShader.loc_intensity, Intensity);
            bloomShader.SetParameter(TextureUnit.Texture0, rt.ColorDst0);
            var units = new TextureUnit[]
            {
                TextureUnit.Texture1,
                TextureUnit.Texture2,
                TextureUnit.Texture3,
                TextureUnit.Texture4,
                TextureUnit.Texture5,
            };
            for (var i = 0; i < 5; i++)
            {
                if (i < BlurNum) bloomShader.SetParameter(units[i], blurRTs[i].ColorDst0);
                else bloomShader.SetParameter(units[i], bloomShader.black);
            } 
            Drawer.DrawTextureMesh();
            bloomShader.UnuseShader();
            
            // 書き出し
            rt.Bind();
            Drawer.DrawTexture(bloomRT.ColorDst0);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        private void ResizeRenderTexture()
        {
            hdrRT.Size = MMW.RenderResolution;
            bloomRT.Size = MMW.RenderResolution;
            var blurSizes = new Size[]
            {
                new Size(MMW.RenderResolution.Width / 4, MMW.RenderResolution.Height / 4),
                new Size(MMW.RenderResolution.Width / 8, MMW.RenderResolution.Height / 8),
                new Size(MMW.RenderResolution.Width / 16, MMW.RenderResolution.Height / 16),
                new Size(MMW.RenderResolution.Width / 32, MMW.RenderResolution.Height / 32),
                new Size(MMW.RenderResolution.Width / 64, MMW.RenderResolution.Height / 64),
            };
            for (var i = 0; i < BlurNum; i++)
            {
                var size = blurSizes[i];
                if (size.Width < 128) size.Width = 128;
                if (size.Height < 128) size.Height = 128;

                blurRTs[i].Size = size;
                blurs[i].ResizeRenderTexture();
            }
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

            hdrRT.Unload();
            bloomRT.Unload();
            for (var i = 0; i < BlurNum; i++)
            {
                blurRTs[i].Unload();
                blurs[i].OnUnload();
            }
        }

        public override GameComponent Clone()
        {
            return new Bloom();
        }
    }
}
