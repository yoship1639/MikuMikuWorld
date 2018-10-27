//
// Miku Miku World License
//
// Copyright (c) 2017 Miku Miku World.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Assets
{
    /// <summary>
    /// 描画用に使用されるテクスチャ
    /// </summary>
    public class RenderTexture : IAsset
    {
        public bool Loaded { get; private set; } = false;
        public string Name { get; set; }

        private Size size;
        public Size Size
        {
            get { return size; }
            set { size = value; if (Loaded) Load(); }
        }

        private PixelInternalFormat colorFormat0 = MMW.Configuration.DefaultPixelFormat;
        public PixelInternalFormat ColorFormat0
        {
            get { return colorFormat0; }
            set { colorFormat0 = value; if (Loaded) Load(); }
        }

        private PixelInternalFormat colorFormat1 = MMW.Configuration.DefaultPixelFormat;
        public PixelInternalFormat ColorFormat1
        {
            get { return colorFormat1; }
            set { colorFormat1 = value; if (Loaded) Load(); }
        }

        private PixelInternalFormat colorFormat2 = MMW.Configuration.DefaultPixelFormat;
        public PixelInternalFormat ColorFormat2
        {
            get { return colorFormat2; }
            set { colorFormat2 = value; if (Loaded) Load(); }
        }

        private PixelInternalFormat colorFormat3 = MMW.Configuration.DefaultPixelFormat;
        public PixelInternalFormat ColorFormat3
        {
            get { return colorFormat3; }
            set { colorFormat3 = value; if (Loaded) Load(); }
        }

        private PixelInternalFormat colorFormat4 = MMW.Configuration.DefaultPixelFormat;
        public PixelInternalFormat ColorFormat4
        {
            get { return colorFormat4; }
            set { colorFormat4 = value; if (Loaded) Load(); }
        }

        private PixelInternalFormat colorFormat5 = MMW.Configuration.DefaultPixelFormat;
        public PixelInternalFormat ColorFormat5
        {
            get { return colorFormat5; }
            set { colorFormat5 = value; if (Loaded) Load(); }
        }

        private PixelInternalFormat colorFormat6 = MMW.Configuration.DefaultPixelFormat;
        public PixelInternalFormat ColorFormat6
        {
            get { return colorFormat6; }
            set { colorFormat6 = value; if (Loaded) Load(); }
        }

        private PixelInternalFormat colorFormat7 = MMW.Configuration.DefaultPixelFormat;
        public PixelInternalFormat ColorFormat7
        {
            get { return colorFormat7; }
            set { colorFormat7 = value; if (Loaded) Load(); }
        }

        private PixelInternalFormat colorFormat8 = MMW.Configuration.DefaultPixelFormat;
        public PixelInternalFormat ColorFormat8
        {
            get { return colorFormat8; }
            set { colorFormat8 = value; if (Loaded) Load(); }
        }

        private PixelInternalFormat colorFormat9 = MMW.Configuration.DefaultPixelFormat;
        public PixelInternalFormat ColorFormat9
        {
            get { return colorFormat9; }
            set { colorFormat9 = value; if (Loaded) Load(); }
        }

        private PixelInternalFormat colorFormat10 = MMW.Configuration.DefaultPixelFormat;
        public PixelInternalFormat ColorFormat10
        {
            get { return colorFormat10; }
            set { colorFormat10 = value; if (Loaded) Load(); }
        }

        private PixelInternalFormat colorFormat11 = MMW.Configuration.DefaultPixelFormat;
        public PixelInternalFormat ColorFormat11
        {
            get { return colorFormat11; }
            set { colorFormat11 = value; if (Loaded) Load(); }
        }

        private int multiBuffer = 1;

        /// <summary>
        /// マルチバッファ出力数
        /// </summary>
        public int MultiBuffer
        {
            get { return multiBuffer; }
            set { multiBuffer = OpenTK.MathHelper.Clamp(value, 1, 12); if (Loaded) Load(); }
        }

        public TextureMinFilter MinFilter { get; set; } = TextureMinFilter.Linear;
        public TextureMagFilter MagFilter { get; set; } = TextureMagFilter.Nearest;
        public TextureWrapMode WrapMode { get; set; } = TextureWrapMode.ClampToBorder;

        public Texture2D ColorDst0 { get; internal set; }
        public Texture2D ColorDst1 { get; internal set; }
        public Texture2D ColorDst2 { get; internal set; }
        public Texture2D ColorDst3 { get; internal set; }
        public Texture2D ColorDst4 { get; internal set; }
        public Texture2D ColorDst5 { get; internal set; }
        public Texture2D ColorDst6 { get; internal set; }
        public Texture2D ColorDst7 { get; internal set; }
        public Texture2D ColorDst8 { get; internal set; }
        public Texture2D ColorDst9 { get; internal set; }
        public Texture2D ColorDst10 { get; internal set; }
        public Texture2D ColorDst11 { get; internal set; }

        internal int rbo_depth = -1;
        internal int fbo = -1;

        public RenderTexture(int width, int height) : this(new Size(width, height), "RenderTexture") { }
        public RenderTexture(int width, int height, string name) :this(new Size(width, height), name) { }
        public RenderTexture(Size size) : this(size, "RenderTexture") { }
        public RenderTexture(Size size, string name)
        {
            Name = name;
            Size = size;
        }

        /// <summary>
        /// アセットをロードする
        /// </summary>
        /// <returns></returns>
        public Result Load()
        {
            Unload();

            if (ColorDst0 == null)
            {
                ColorDst0 = new Texture2D(Size.Width, Size.Height, colorFormat0);
                ColorDst0.MagFilter = MagFilter;
                ColorDst0.MinFilter = MinFilter;
                ColorDst0.WrapMode = WrapMode;
                ColorDst0.Load();
            }
            if (MultiBuffer >= 2)
            {
                ColorDst1 = new Texture2D(Size.Width, Size.Height, colorFormat1);
                ColorDst1.MagFilter = MagFilter;
                ColorDst1.MinFilter = MinFilter;
                ColorDst1.WrapMode = WrapMode;
                ColorDst1.Load();
            }
            if (MultiBuffer >= 3)
            {
                ColorDst2 = new Texture2D(Size.Width, Size.Height, colorFormat2);
                ColorDst2.MagFilter = MagFilter;
                ColorDst2.MinFilter = MinFilter;
                ColorDst2.WrapMode = WrapMode;
                ColorDst2.Load();
            }
            if (MultiBuffer >= 4)
            {
                ColorDst3 = new Texture2D(Size.Width, Size.Height, colorFormat3);
                ColorDst3.MagFilter = MagFilter;
                ColorDst3.MinFilter = MinFilter;
                ColorDst3.WrapMode = WrapMode;
                ColorDst3.Load();
            }
            if (MultiBuffer >= 5)
            {
                ColorDst4 = new Texture2D(Size.Width, Size.Height, colorFormat4);
                ColorDst4.MagFilter = MagFilter;
                ColorDst4.MinFilter = MinFilter;
                ColorDst4.WrapMode = WrapMode;
                ColorDst4.Load();
            }
            if (MultiBuffer >= 6)
            {
                ColorDst5 = new Texture2D(Size.Width, Size.Height, colorFormat5);
                ColorDst5.MagFilter = MagFilter;
                ColorDst5.MinFilter = MinFilter;
                ColorDst5.WrapMode = WrapMode;
                ColorDst5.Load();
            }
            if (MultiBuffer >= 7)
            {
                ColorDst6 = new Texture2D(Size.Width, Size.Height, colorFormat6);
                ColorDst6.MagFilter = MagFilter;
                ColorDst6.MinFilter = MinFilter;
                ColorDst6.WrapMode = WrapMode;
                ColorDst6.Load();
            }
            if (MultiBuffer >= 8)
            {
                ColorDst7 = new Texture2D(Size.Width, Size.Height, colorFormat7);
                ColorDst7.MagFilter = MagFilter;
                ColorDst7.MinFilter = MinFilter;
                ColorDst7.WrapMode = WrapMode;
                ColorDst7.Load();
            }
            if (MultiBuffer >= 9)
            {
                ColorDst8 = new Texture2D(Size.Width, Size.Height, colorFormat8);
                ColorDst8.MagFilter = MagFilter;
                ColorDst8.MinFilter = MinFilter;
                ColorDst8.WrapMode = WrapMode;
                ColorDst8.Load();
            }
            if (MultiBuffer >= 10)
            {
                ColorDst9 = new Texture2D(Size.Width, Size.Height, colorFormat9);
                ColorDst9.MagFilter = MagFilter;
                ColorDst9.MinFilter = MinFilter;
                ColorDst9.WrapMode = WrapMode;
                ColorDst9.Load();
            }
            if (MultiBuffer >= 11)
            {
                ColorDst10 = new Texture2D(Size.Width, Size.Height, colorFormat10);
                ColorDst10.MagFilter = MagFilter;
                ColorDst10.MinFilter = MinFilter;
                ColorDst10.WrapMode = WrapMode;
                ColorDst10.Load();
            }
            if (MultiBuffer >= 12)
            {
                ColorDst11 = new Texture2D(Size.Width, Size.Height, colorFormat11);
                ColorDst11.MagFilter = MagFilter;
                ColorDst11.MinFilter = MinFilter;
                ColorDst11.WrapMode = WrapMode;
                ColorDst11.Load();
            }

            rbo_depth = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rbo_depth);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, Size.Width, Size.Height);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

            fbo = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, ColorDst0.texture, 0);
            if (MultiBuffer >= 2) GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, ColorDst1.texture, 0);
            if (MultiBuffer >= 3) GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment2, TextureTarget.Texture2D, ColorDst2.texture, 0);
            if (MultiBuffer >= 4) GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment3, TextureTarget.Texture2D, ColorDst3.texture, 0);
            if (MultiBuffer >= 5) GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment4, TextureTarget.Texture2D, ColorDst4.texture, 0);
            if (MultiBuffer >= 6) GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment5, TextureTarget.Texture2D, ColorDst5.texture, 0);
            if (MultiBuffer >= 7) GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment6, TextureTarget.Texture2D, ColorDst6.texture, 0);
            if (MultiBuffer >= 8) GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment7, TextureTarget.Texture2D, ColorDst7.texture, 0);
            if (MultiBuffer >= 9) GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment8, TextureTarget.Texture2D, ColorDst8.texture, 0);
            if (MultiBuffer >= 10) GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment9, TextureTarget.Texture2D, ColorDst9.texture, 0);
            if (MultiBuffer >= 11) GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment10, TextureTarget.Texture2D, ColorDst10.texture, 0);
            if (MultiBuffer >= 12) GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment11, TextureTarget.Texture2D, ColorDst11.texture, 0);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, rbo_depth);

            
            DrawBuffersEnum[] attachments = new DrawBuffersEnum[]
            {
                DrawBuffersEnum.ColorAttachment0,
                DrawBuffersEnum.ColorAttachment1,
                DrawBuffersEnum.ColorAttachment2,
                DrawBuffersEnum.ColorAttachment3,
                DrawBuffersEnum.ColorAttachment4,
                DrawBuffersEnum.ColorAttachment5,
                DrawBuffersEnum.ColorAttachment6,
                DrawBuffersEnum.ColorAttachment7,
                DrawBuffersEnum.ColorAttachment8,
                DrawBuffersEnum.ColorAttachment9,
                DrawBuffersEnum.ColorAttachment10,
                DrawBuffersEnum.ColorAttachment11,
            };
            if (MultiBuffer >= 2) GL.DrawBuffers(MultiBuffer, attachments);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            Loaded = true;
            return Result.Success;
        }

        public void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
            GL.Viewport(Size);
        }

        public void Bind(Color4 clearColor)
        {
            Bind(fbo, Size, clearColor);
        }

        public static void Bind(int fbo, Size size, Color4 clearColor)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
            GL.Viewport(size);
            GL.ClearColor(clearColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        }

        /// <summary>
        /// アセットをアンロードする
        /// </summary>
        /// <returns></returns>
        public Result Unload()
        {
            if (!Loaded) return Result.NotLoaded;

            GL.DeleteFramebuffer(fbo);
            fbo = -1;
            if (ColorDst0 != null)
            {
                ColorDst0.Unload();
                ColorDst0 = null;
            }
            if (ColorDst1 != null)
            {
                ColorDst1.Unload();
                ColorDst1 = null;
            }
            if (ColorDst2 != null)
            {
                ColorDst2.Unload();
                ColorDst2 = null;
            }
            if (ColorDst3 != null)
            {
                ColorDst3.Unload();
                ColorDst3 = null;
            }
            if (ColorDst4 != null)
            {
                ColorDst4.Unload();
                ColorDst4 = null;
            }
            if (ColorDst5 != null)
            {
                ColorDst5.Unload();
                ColorDst5 = null;
            }
            if (ColorDst6 != null)
            {
                ColorDst6.Unload();
                ColorDst6 = null;
            }
            if (ColorDst7 != null)
            {
                ColorDst7.Unload();
                ColorDst7 = null;
            }
            if (ColorDst8 != null)
            {
                ColorDst8.Unload();
                ColorDst8 = null;
            }
            if (ColorDst9 != null)
            {
                ColorDst9.Unload();
                ColorDst9 = null;
            }
            if (ColorDst10 != null)
            {
                ColorDst10.Unload();
                ColorDst10 = null;
            }
            if (ColorDst11 != null)
            {
                ColorDst11.Unload();
                ColorDst11 = null;
            }
            GL.DeleteRenderbuffer(rbo_depth);

            Loaded = false;
            return Result.Success;
        }
    }
}
