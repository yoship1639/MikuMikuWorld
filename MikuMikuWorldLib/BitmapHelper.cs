using MikuMikuWorld.Assets;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MikuMikuWorld
{
    public static class BitmapHelper
    {
        /// <summary>
        /// 複数のビットマップを１つのビットマップにまとめる
        /// </summary>
        /// <param name="bitmaps"></param>
        /// <param name="gridNum"></param>
        /// <param name="gridSize"></param>
        /// <returns></returns>
        public static Bitmap CombineBitmaps(Bitmap[] bitmaps, int gridNum, int gridSize = 256)
        {
            var bitmap = new Bitmap(gridNum * gridSize, gridNum * gridSize);
            var g = Graphics.FromImage(bitmap);
            g.Clear(Color.FromArgb(0));

            for (var y = 0; y < gridNum; y++)
            {
                for (var x = 0; x < gridNum; x++)
                {
                    var index = y * gridNum + x;
                    if (index >= bitmaps.Length) continue;

                    g.DrawImage(bitmaps[index], x * gridSize, y * gridSize, gridSize, gridSize);
                }
            }
            g.Dispose();
            return bitmap;
        }

        /// <summary>
        /// 複数のテクスチャを１つのビットマップにまとめる
        /// </summary>
        /// <param name="textures"></param>
        /// <param name="gridNum"></param>
        /// <param name="gridSize"></param>
        /// <returns></returns>
        public static Texture2D CombineBitmaps(Texture2D[] textures, int gridNum, int gridSize = 256)
        {
            RenderTexture rt = new RenderTexture(gridNum * gridSize, gridNum * gridSize);
            rt.ColorFormat0 = MMW.Configuration.DefaultPixelFormat;
            rt.Load();

            rt.Bind(Color4.White);

            for (var y = 0; y < gridNum; y++)
            {
                for (var x = 0; x < gridNum; x++)
                {
                    var index = y * gridNum + x;
                    if (index >= textures.Length) continue;

                    Drawer.DrawTexture(
                        textures[index],
                        new RectangleF(0.0f, 0.0f, 1.0f, 1.0f),
                        new RectangleF(x / (float)gridNum, y / (float)gridNum, 1.0f / (float)gridNum, 1.0f / (float)gridNum));
                }
            }

            //var res = (Bitmap)rt.ColorDst0.SrcBitmap.Clone();
            return rt.ColorDst0;
        }

        public static Bitmap ResizeBitmap(Bitmap bitmap, int width, int height)
        {
            var b = new Bitmap(width, height);

            var g = Graphics.FromImage(b);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.DrawImage(bitmap, new Rectangle(0, 0, width, height), new Rectangle(0, 0, bitmap.Width, bitmap.Height), GraphicsUnit.Pixel);
            g.Dispose();

            return b;
        }

        public static Bitmap Blur(this Bitmap bitmap, int radius)
        {
            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            IntPtr ptr = data.Scan0;
            byte[] pixels = new byte[data.Stride * bitmap.Height];
            byte[] dsts = new byte[data.Stride * bitmap.Height];
            Marshal.Copy(ptr, pixels, 0, pixels.Length);

            for (var y = 0; y < bitmap.Height; y++)
            {
                for (var x = 0; x < bitmap.Width; x++)
                {
                    int r = 0;
                    int g = 0;
                    int b = 0;
                    var picNum = 0;
                    for (var iy = -radius; iy <= radius; iy++)
                    {
                        for (var ix = -radius; ix <= radius; ix++)
                        {
                            if ((Math.Abs(iy) + Math.Abs(ix)) > radius) continue;
                            picNum++;
                            var picy = y + iy;
                            var picx = x + ix;
                            if (picy < 0) picy += bitmap.Height;
                            else if (picy >= bitmap.Height) picy -= bitmap.Height;
                            if (picx < 0) picx += bitmap.Width;
                            else if (picx >= bitmap.Width) picx -= bitmap.Width;
                            var offset = ((picy * bitmap.Width) + picx) * 4;
                            r += pixels[offset + 0];
                            g += pixels[offset + 1];
                            b += pixels[offset + 2];
                        }
                    }
                    r /= picNum;
                    g /= picNum;
                    b /= picNum;

                    var o = ((y * bitmap.Width) + x) * 4;
                    dsts[o + 0] = (byte)r;
                    dsts[o + 1] = (byte)g;
                    dsts[o + 2] = (byte)b;
                    dsts[o + 3] = 255;
                }
            }

            Marshal.Copy(dsts, 0, ptr, pixels.Length);
            bitmap.UnlockBits(data);
            return bitmap;
        }

        public static Bitmap ToBitmap(this BitmapSource bitmapSource, System.Drawing.Imaging.PixelFormat pixelFormat)
        {
            int width = bitmapSource.PixelWidth;
            int height = bitmapSource.PixelHeight;
            int stride = width * ((bitmapSource.Format.BitsPerPixel + 7) / 8);  // 行の長さは色深度によらず8の倍数のため
            IntPtr intPtr = IntPtr.Zero;
            try
            {
                intPtr = Marshal.AllocCoTaskMem(height * stride);
                bitmapSource.CopyPixels(new Int32Rect(0, 0, width, height), intPtr, height * stride, stride);
                using (var bitmap = new Bitmap(width, height, stride, pixelFormat, intPtr))
                {
                    // IntPtrからBitmapを生成した場合、Bitmapが存在する間、AllocCoTaskMemで確保したメモリがロックされたままとなる
                    // （FreeCoTaskMemするとエラーとなる）
                    // そしてBitmapを単純に開放しても解放されない
                    // このため、明示的にFreeCoTaskMemを呼んでおくために一度作成したBitmapから新しくBitmapを
                    // 再作成し直しておくとメモリリークを抑えやすい
                    return new Bitmap(bitmap);
                }
            }
            finally
            {
                if (intPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(intPtr);
            }
        }
    }
}
