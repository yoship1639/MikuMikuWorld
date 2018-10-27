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

using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Assets
{
    [DataContract]
    public class Texture2D : Texture
    {
        public override TextureTarget Target { get { return TextureTarget.Texture2D; } }

        [DataMember(Name = "flip_y", EmitDefaultValue = false, Order = 7)]
        private bool flipY = false;

        public Texture2D(Bitmap bitmap, bool flipY = true) : this(bitmap, "Texture2D", flipY) { }
        public Texture2D(Bitmap bitmap, string name, bool flipY = true)
        {
            Name = name;
            SrcBitmap = bitmap;
            Size = bitmap.Size;
            Format = PixelInternalFormat.Rgba8;
            this.flipY = flipY;
        }
        public Texture2D(int width, int height) : this(width, height, PixelInternalFormat.Rgba8, "Texture2D") { }
        public Texture2D(int width, int height, PixelInternalFormat format) : this(width, height, format, "Texture2D") { }
        public Texture2D(int width, int height, string name) : this(width, height, PixelInternalFormat.Rgba8, name) { }
        public Texture2D(int width, int height, PixelInternalFormat format, string name)
        {
            Name = name;
            Size = new Size(width, height);
            Format = format;
        }

        public override Result Load()
        {
            Unload();

            texture = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texture);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)MinFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)MagFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)WrapMode);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)WrapMode);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLod, 9);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 9);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinLod, 0);

            var bitmap = SrcBitmap;
            if (bitmap == null)
            {
                bitmap = new Bitmap(Size.Width, Size.Height);
                SrcBitmap = bitmap;
            }
            else if (flipY) bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

            var data = bitmap.LockBits(
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb
                    );

            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                Format,
                data.Width,
                data.Height,
                0,
                PixelFormat.Bgra,
                PixelType.UnsignedByte,
                data.Scan0
            );

            if (UseMipmap) GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            bitmap.UnlockBits(data);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            Loaded = true;
            return Result.Success;
        }

        public void Update<T>(T[] data, PixelFormat format = PixelFormat.Rgba, PixelType type = PixelType.Float) where T : struct
        {
            if (!Loaded) return;
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, Format, Size.Width, Size.Height, 0, format, type, data);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Update<T>(T[] data, int width, int height, PixelFormat format, PixelType type) where T : struct
        {
            if (!Loaded) return;
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, Format, width, height, 0, format, type, data);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public override Result Unload()
        {
            if (!Loaded) return Result.NotLoaded;

            GL.DeleteTexture(texture);
            texture = -1;

            if (SrcBitmap != null && flipY) SrcBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

            Loaded = false;
            return Result.Success;
        }

        public void Save(string filepath, System.Drawing.Imaging.ImageFormat format)
        {
            unsafe
            {
               
                var w = SrcBitmap.Size.Width;
                var h = SrcBitmap.Size.Height;
                var ptr = Marshal.AllocCoTaskMem(w * h * 4);
                GL.ReadBuffer(ReadBufferMode.Front);
                GL.ReadPixels(0, 0, w, h, PixelFormat.Bgra, PixelType.UnsignedByte, ptr);
                var p = (byte*)ptr;
                var bitmap = new Bitmap(w, h);
                var data = bitmap.LockBits(new Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                CopyMemory(data.Scan0, ptr, (uint)(w * h * 4));
                bitmap.UnlockBits(data);
                bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                bitmap.Save(filepath, format);
                bitmap.Dispose();
            }
        }

        public override string ToString()
        {
            return string.Format("{0}: ({1}, {2})", Name, Size.Width, Size.Height);
        }

        [DataMember(EmitDefaultValue = false, Order = 8)]
        private string srcbitmap
        {
            get
            {
                if (SrcBitmap == null) return null;
                byte[] buf;
                using (var stream = new System.IO.MemoryStream())
                {
                    SrcBitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    buf = stream.ToArray();
                }
                return Convert.ToBase64String(buf);
            }
            set
            {
                if (value == null) return;
                var buf = Convert.FromBase64String(value);
                using (var stream = new System.IO.MemoryStream(buf))
                {
                    stream.Position = 0;
                    SrcBitmap = (Bitmap)Image.FromStream(stream);
                }
            }
        }

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);
    }
}
