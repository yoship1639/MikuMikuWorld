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
    public class Glare : ImageEffect
    {
        private static readonly int BlurNum = 3;

        public RenderTexture RenderTexture { get; set; }
        public Camera Camera { get; set; }

        public float Intensity { get; set; } = 2.0f;

        private RenderTexture hdrRT;
        private ExtractHDRShader hdrShader;
        public float HDRThreshold { get; set; } = 1.2f;

        public float Radius
        {
            get { return blurs[0].Radius; }
            set
            {
                var reduct = 1.0f;
                for (var i = 0; i < BlurNum; i++)
                {
                    blurs[i].Radius = value * reduct;
                    reduct *= 2.0f;
                } 
            }
        }
        public Vector2 Direction { get; set; } = Vector2.One;
        public GlareBlur.GlareType GlareType { get; set; } = GlareBlur.GlareType.Plus;

        private RenderTexture[] blurRT1s;
        private RenderTexture[] blurRT2s;
        private RenderTexture[] blurRT3s;
        private GlareBlur[] blurs;

        private GlareShader glareShader;
        private RenderTexture glareRT;
        private Matrix4 orthoMatrix;

        protected internal override void OnLoad()
        {
            base.OnLoad();

            // 書き出しRT
            glareRT = new RenderTexture(MMW.RenderResolution);
            glareRT.MagFilter = TextureMagFilter.Linear;
            glareRT.MinFilter = TextureMinFilter.Linear;
            glareRT.ColorFormat0 = MMW.Configuration.DefaultPixelFormat;
            glareRT.Load();

            // HDR抽出RT
            hdrRT = new RenderTexture(MMW.RenderResolution);
            hdrRT.MagFilter = TextureMagFilter.Linear;
            hdrRT.MinFilter = TextureMinFilter.Linear;
            hdrRT.ColorFormat0 = MMW.Configuration.DefaultPixelFormat;
            hdrRT.Load();

            // Blur適用RT
            blurRT1s = new RenderTexture[BlurNum];
            blurRT2s = new RenderTexture[BlurNum];
            blurRT3s = new RenderTexture[BlurNum];
            blurs = new GlareBlur[BlurNum];
            var blurSizes = new Size[]
            {
                new Size(MMW.RenderResolution.Width / 4, MMW.RenderResolution.Height / 4),
                new Size(MMW.RenderResolution.Width / 8, MMW.RenderResolution.Height / 8),
                new Size(MMW.RenderResolution.Width / 16, MMW.RenderResolution.Height / 16),
            };
            var reduct = 1.0f;
            for (var i = 0; i < BlurNum; i++)
            {
                var size = blurSizes[i];
                if (size.Width < 128) size.Width = 128;
                if (size.Height < 128) size.Height = 128;

                blurRT1s[i] = new RenderTexture(size);
                blurRT1s[i].MagFilter = TextureMagFilter.Linear;
                blurRT1s[i].MinFilter = TextureMinFilter.Linear;
                blurRT1s[i].WrapMode = TextureWrapMode.ClampToEdge;
                blurRT1s[i].ColorFormat0 = MMW.Configuration.DefaultPixelFormat;
                blurRT1s[i].Load();

                blurRT2s[i] = new RenderTexture(size);
                blurRT2s[i].MagFilter = TextureMagFilter.Linear;
                blurRT2s[i].MinFilter = TextureMinFilter.Linear;
                blurRT2s[i].WrapMode = TextureWrapMode.ClampToEdge;
                blurRT2s[i].ColorFormat0 = MMW.Configuration.DefaultPixelFormat;
                blurRT2s[i].Load();

                blurRT3s[i] = new RenderTexture(size);
                blurRT3s[i].MagFilter = TextureMagFilter.Linear;
                blurRT3s[i].MinFilter = TextureMinFilter.Linear;
                blurRT3s[i].WrapMode = TextureWrapMode.ClampToEdge;
                blurRT3s[i].ColorFormat0 = MMW.Configuration.DefaultPixelFormat;
                blurRT3s[i].Load();

                blurs[i] = new GlareBlur(3.0f * reduct, Direction, GlareBlur.GlareType.Line);
                blurs[i].RenderTexture = blurRT1s[i];
                blurs[i].OnLoad();
                reduct *= 2.0f;
            }

            // HDR抽出シェーダ
            hdrShader = (ExtractHDRShader)MMW.GetAsset<Shader>("Extract HDR");
            if (hdrShader == null)
            {
                hdrShader = new ExtractHDRShader();
                MMW.RegistAsset(hdrShader);
            }

            // Bloomシェーダ
            glareShader = (GlareShader)MMW.GetAsset<Shader>("Glare");
            if (glareShader == null)
            {
                glareShader = new GlareShader();
                MMW.RegistAsset(glareShader);
            }

            if (GameObject != null)
            {
                Camera = GameObject.GetComponent<Camera>();
                RenderTexture = Camera.TargetTexture;
            }

            orthoMatrix = Matrix4.CreateOrthographicOffCenter(-1, 1, -1, 1, -1, 1);
        }

        private float cos60 = (float)Math.Cos(MathHelper.PiOver3);
        private float sin60 = (float)Math.Sin(MathHelper.PiOver3);
        private float cos120 = (float)Math.Cos(MathHelper.PiOver3 * 2.0f);
        private float sin120 = (float)Math.Sin(MathHelper.PiOver3 * 2.0f);

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
            hdrShader.SetParameter(hdrShader.loc_threshold, HDRThreshold);
            hdrShader.SetParameter(hdrShader.loc_mvp, ref orthoMatrix, false);
            hdrShader.SetParameter(TextureUnit.Texture0, rt.ColorDst0);
            Drawer.DrawTextureMesh();
            hdrShader.UnuseShader();
            
            // HDR画像のぼかし画像を生成
            // line
            for (var i = 0; i < BlurNum; i++)
            {
                blurs[i].Direction = Direction;
                blurs[i].RenderTexture = blurRT1s[i];
                if (i == 0) blurs[i].SrcTexture = hdrRT.ColorDst0;
                else blurs[i].SrcTexture = blurRT1s[i - 1].ColorDst0;
                blurs[i].Draw(deltaTime);
            }
            if (GlareType == GlareBlur.GlareType.Plus)
            {
                var dir = new Vector2(-Direction.Y, Direction.X);
                for (var i = 0; i < BlurNum; i++)
                {
                    blurs[i].Direction = dir;
                    blurs[i].RenderTexture = blurRT2s[i];
                    if (i == 0) blurs[i].SrcTexture = hdrRT.ColorDst0;
                    else blurs[i].SrcTexture = blurRT2s[i - 1].ColorDst0;
                    blurs[i].Draw(deltaTime);
                }
            }
            else if (GlareType == GlareBlur.GlareType.Star)
            {
                var dir1 = new Vector2((Direction.X * cos60) - (Direction.Y * sin60), (Direction.X * sin60) + (Direction.Y * cos60));
                var dir2 = new Vector2((Direction.X * cos120) - (Direction.Y * sin120), (Direction.X * sin120) + (Direction.Y * cos120));
                for (var i = 0; i < BlurNum; i++)
                {
                    blurs[i].Direction = dir1;
                    blurs[i].RenderTexture = blurRT2s[i];
                    if (i == 0) blurs[i].SrcTexture = hdrRT.ColorDst0;
                    else blurs[i].SrcTexture = blurRT2s[i - 1].ColorDst0;
                    blurs[i].Draw(deltaTime);
                }
                for (var i = 0; i < BlurNum; i++)
                {
                    blurs[i].Direction = dir2;
                    blurs[i].RenderTexture = blurRT3s[i];
                    if (i == 0) blurs[i].SrcTexture = hdrRT.ColorDst0;
                    else blurs[i].SrcTexture = blurRT3s[i - 1].ColorDst0;
                    blurs[i].Draw(deltaTime);
                }
            }

            // Glare処理
            glareRT.Bind(Color4.Black);
            glareShader.UseShader();
            glareShader.SetParameter(glareShader.loc_resolution, glareRT.Size.ToVector2().Inverse());
            glareShader.SetParameter(glareShader.loc_mvp, ref orthoMatrix, false);
            glareShader.SetParameter(glareShader.loc_intensity, Intensity);
            glareShader.SetParameter(TextureUnit.Texture0, rt.ColorDst0);
            var units = new TextureUnit[][]
            {
                new TextureUnit[]
                {
                    TextureUnit.Texture1,
                    TextureUnit.Texture2,
                    TextureUnit.Texture3,
                },
                new TextureUnit[]
                {
                    TextureUnit.Texture4,
                    TextureUnit.Texture5,
                    TextureUnit.Texture6,
                },
                new TextureUnit[] 
                {
                    TextureUnit.Texture7,
                    TextureUnit.Texture8,
                    TextureUnit.Texture9,
                },
            };
            // line
            for (var i = 0; i < 3; i++)
            {
                if (i < BlurNum) glareShader.SetParameter(units[0][i], blurRT1s[i].ColorDst0);
                else glareShader.SetParameter(units[0][i], glareShader.black);
            }
            // plus
            for (var i = 0; i < 3; i++)
            {
                if (i < BlurNum && GlareType > GlareBlur.GlareType.Line) glareShader.SetParameter(units[1][i], blurRT2s[i].ColorDst0);
                else glareShader.SetParameter(units[1][i], glareShader.black);
            }
            // star
            for (var i = 0; i < 3; i++)
            {
                if (i < BlurNum && GlareType > GlareBlur.GlareType.Plus) glareShader.SetParameter(units[2][i], blurRT3s[i].ColorDst0);
                else glareShader.SetParameter(units[2][i], glareShader.black);
            }
            Drawer.DrawTextureMesh();
            glareShader.UnuseShader();
            
            // 書き出し
            rt.Bind();
            Drawer.DrawTexture(glareRT.ColorDst0);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        private void ResizeRenderTexture()
        {
            hdrRT.Size = MMW.RenderResolution;
            glareRT.Size = MMW.RenderResolution;
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

                blurRT1s[i].Size = size;
                blurRT2s[i].Size = size;
                blurRT3s[i].Size = size;
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
            glareRT.Unload();
            for (var i = 0; i < BlurNum; i++)
            {
                blurRT1s[i].Unload();
                blurRT2s[i].Unload();
                blurRT3s[i].Unload();
                blurs[i].OnUnload();
            }
        }

        public override GameComponent Clone()
        {
            return new Bloom();
        }
    }
}
