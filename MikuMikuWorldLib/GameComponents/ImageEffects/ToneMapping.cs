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
    public class ToneMapping : ImageEffect
    {
        public RenderTexture RenderTexture { get; set; }
        public Camera Camera { get; set; }

        public float Intensity { get; set; } = 0.4f;

        private double totalTime = 1.01;
        public double ExtractBrightInterval { get; set; } = 1.0;
    
        private RenderTexture aveRT;
        private AverageBrightShader aveShader;

        private RenderTexture[] shrinkRTs;
        private ShrinkShader shrinkShader;

        private RenderTexture toneRT;
        private ToneMappingShader toneShader;

        private Matrix4 orthoMatrix;

        int ssbo_lum;

        protected internal override void OnLoad()
        {
            base.OnLoad();

            // 書き出しRT
            toneRT = new RenderTexture(MMW.RenderResolution);
            toneRT.Load();

            // 平均輝度RT
            aveRT = new RenderTexture(MMW.RenderResolution.Mul(0.5f));
            aveRT.Load();

            // 縮小RT
            var size = 256;
            var rts = new List<RenderTexture>();
            while (size >= 1)
            {
                var rt = new RenderTexture(size, size);
                rt.Load();
                rts.Add(rt);
                size /= 4;
            }
            shrinkRTs = rts.ToArray();

            // 平均輝度抽出シェーダ
            aveShader = (AverageBrightShader)MMW.GetAsset<Shader>("Average Bright");
            if (aveShader == null)
            {
                aveShader = new AverageBrightShader();
                MMW.RegistAsset(aveShader);
            }

            // 縮小シェーダ
            shrinkShader = (ShrinkShader)MMW.GetAsset<Shader>("Shrink");
            if (shrinkShader == null)
            {
                shrinkShader = new ShrinkShader();
                MMW.RegistAsset(shrinkShader);
            }

            // トーンマッピングシェーダ
            toneShader = (ToneMappingShader)MMW.GetAsset<Shader>("Tone Mapping");
            if (toneShader == null)
            {
                toneShader = new ToneMappingShader();
                MMW.RegistAsset(toneShader);
            }

            if (GameObject != null)
            {
                Camera = GameObject.GetComponent<Camera>();
                RenderTexture = Camera.TargetTexture;
            }

            orthoMatrix = Matrix4.CreateOrthographicOffCenter(-1, 1, -1, 1, -1, 1);

            GL.GenBuffers(1, out ssbo_lum);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ssbo_lum);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, 16, new float[] { 1.0f, 0.7f, 1.0f, 0.7f }, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
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

            totalTime += deltaTime;
            if (totalTime > ExtractBrightInterval)
            {
                totalTime -= ExtractBrightInterval;
                // 平均輝度抽出
                var reso = aveRT.Size.ToVector2().Inverse();
                aveRT.Bind(Color4.Black);
                aveShader.UseShader();
                aveShader.SetParameter(aveShader.loc_resolution, ref reso);
                aveShader.SetParameter(aveShader.loc_mvp, ref orthoMatrix, false);
                aveShader.SetParameter(TextureUnit.Texture0, rt.ColorDst0);
                Drawer.DrawTextureMesh();
                aveShader.UnuseShader();

                // 縮小
                shrinkShader.UseShader();
                for (var i = 0; i < shrinkRTs.Length; i++)
                {
                    shrinkRTs[i].Bind(Color4.Black);
                    if (i == 0) shrinkShader.SetParameter(TextureUnit.Texture0, aveRT.ColorDst0);
                    else shrinkShader.SetParameter(TextureUnit.Texture0, shrinkRTs[i - 1].ColorDst0);
                    shrinkShader.SetParameter(shrinkShader.loc_mvp, ref orthoMatrix, false);
                    shrinkShader.SetParameter(shrinkShader.loc_offset, ref reso);
                    reso = shrinkRTs[i].Size.ToVector2().Inverse();
                    shrinkShader.SetParameter(shrinkShader.loc_resolution, ref reso);

                    if (i == shrinkRTs.Length - 1) GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 6, ssbo_lum);
                    Drawer.DrawTextureMesh();
                }
                shrinkShader.UnuseShader();
            }

            // トーンマッピング
            toneRT.Bind(Color4.Black);
            toneShader.UseShader();
            toneShader.SetParameter(toneShader.loc_resolution, toneRT.Size.ToVector2().Inverse());
            toneShader.SetParameter(toneShader.loc_mvp, ref orthoMatrix, false);
            toneShader.SetParameter(toneShader.loc_intensity, Intensity);
            toneShader.SetParameter(toneShader.loc_rate, (float)(totalTime / ExtractBrightInterval));
            toneShader.SetParameter(TextureUnit.Texture0, rt.ColorDst0);
            toneShader.SetParameter(TextureUnit.Texture1, shrinkRTs.Last().ColorDst0);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 6, ssbo_lum);
            Drawer.DrawTextureMesh();
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 6, 0);
            toneShader.UnuseShader();

            // 書き出し
            rt.Bind();
            Drawer.DrawTexture(toneRT.ColorDst0);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        private void ResizeRenderTexture()
        {
            aveRT.Size = MMW.RenderResolution.Mul(0.5f);
            toneRT.Size = MMW.RenderResolution;
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

            aveRT.Unload();
            toneRT.Unload();
            for (var i = 0; i < shrinkRTs.Length; i++)
            {
                shrinkRTs[i].Unload();
            }
        }

        public override GameComponent Clone()
        {
            return new ToneMapping();
        }
    }
}
