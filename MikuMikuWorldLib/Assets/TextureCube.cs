using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Assets
{
    public class TextureCube : Texture
    {
        public override TextureTarget Target { get { return TextureTarget.TextureCubeMap; } }

        public Bitmap SrcBitmapNegX { get; internal set; }
        public Bitmap SrcBitmapNegY { get; internal set; }
        public Bitmap SrcBitmapNegZ { get; internal set; }
        public Bitmap SrcBitmapPosX { get; internal set; }
        public Bitmap SrcBitmapPosY { get; internal set; }
        public Bitmap SrcBitmapPosZ { get; internal set; }

        private bool flipY;

        public TextureCube(Bitmap bitmap, bool flipY = true) : this(bitmap, "TextureCube", flipY) { }
        public TextureCube(Bitmap bitmap, string name, bool flipY = true)
        {
            Name = name;
            SrcBitmap = bitmap;
            Size = bitmap.Size;
            Format = PixelInternalFormat.Rgba8;
            this.flipY = flipY;
        }
        public TextureCube(Bitmap negX, Bitmap negY, Bitmap negZ, Bitmap posX, Bitmap posY, Bitmap posZ, bool flipY = true)
            : this(negX, negY, negZ, posX, posY, posZ, "TextureCube", flipY) { }
        public TextureCube(Bitmap negX, Bitmap negY, Bitmap negZ, Bitmap posX, Bitmap posY, Bitmap posZ, string name, bool flipY = true)
        {
            Name = name;
            SrcBitmapNegX = negX;
            SrcBitmapNegY = negY;
            SrcBitmapNegZ = negZ;
            SrcBitmapPosX = posX;
            SrcBitmapPosY = posY;
            SrcBitmapPosZ = posZ;
            Size = negX.Size;
            Format = PixelInternalFormat.Rgba8;
            this.flipY = flipY;
        }

        public override Result Load()
        {
            Unload();

            texture = GL.GenTexture();

            GL.BindTexture(Target, texture);

            GL.TexParameter(Target, TextureParameterName.TextureMinFilter, (int)MinFilter);
            GL.TexParameter(Target, TextureParameterName.TextureMagFilter, (int)MagFilter);
            GL.TexParameter(Target, TextureParameterName.TextureWrapR, (int)WrapMode);
            GL.TexParameter(Target, TextureParameterName.TextureWrapS, (int)WrapMode);
            GL.TexParameter(Target, TextureParameterName.TextureWrapT, (int)WrapMode);
            GL.TexParameter(Target, TextureParameterName.TextureMaxLod, 9);
            GL.TexParameter(Target, TextureParameterName.TextureMaxLevel, 9);
            GL.TexParameter(Target, TextureParameterName.TextureMinLod, 0);

            var target = new TextureTarget[]
            {
                TextureTarget.TextureCubeMapNegativeX,
                TextureTarget.TextureCubeMapNegativeY,
                TextureTarget.TextureCubeMapNegativeZ,
                TextureTarget.TextureCubeMapPositiveX,
                TextureTarget.TextureCubeMapPositiveY,
                TextureTarget.TextureCubeMapPositiveZ,
            };

            // 1面キューブマップ
            if (SrcBitmap != null)
            {
                var sx = SrcBitmap.Width / 4;
                var sy = SrcBitmap.Height / 3;
                var rects = new Rectangle[]
                {
                    new Rectangle(0, sy, sx, sy), // neg x
                    new Rectangle(sx, sy*2, sx, sy), // neg y
                    new Rectangle(sx*3, sy, sx, sy), // neg z
                    new Rectangle(sx*2, sy, sx, sy), // pos x
                    new Rectangle(sx, 0, sx, sy), // pos y
                    new Rectangle(sx, sy, sx, sy), // pos z
                };

                if (flipY) SrcBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

                for (var i = 0; i < target.Length; i++)
                {
                    var data = SrcBitmap.LockBits(
                    rects[i],
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb
                    );

                    GL.TexImage2D(
                    target[i],
                    0,
                    Format,
                    sx,
                    sy,
                    0,
                    PixelFormat.Bgra,
                    PixelType.UnsignedByte,
                    data.Scan0);

                    if (UseMipmap) GL.GenerateMipmap(GenerateMipmapTarget.TextureCubeMap);

                    SrcBitmap.UnlockBits(data);
                }
            }
            else if (SrcBitmapNegX != null && SrcBitmapNegY != null && SrcBitmapNegZ != null && SrcBitmapPosX != null && SrcBitmapPosY != null && SrcBitmapPosZ != null)
            {
                var bitmaps = new Bitmap[]
                {
                    SrcBitmapNegX,
                    SrcBitmapNegY,
                    SrcBitmapNegZ,
                    SrcBitmapPosX,
                    SrcBitmapPosY,
                    SrcBitmapPosZ,
                };

                for (var i = 0; i < target.Length; i++)
                {
                    if (flipY) bitmaps[i].RotateFlip(RotateFlipType.RotateNoneFlipY);

                    var data = bitmaps[i].LockBits(
                    new Rectangle(0, 0, bitmaps[i].Width, bitmaps[i].Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb
                    );

                    GL.TexImage2D(
                    target[i],
                    0,
                    Format,
                    bitmaps[i].Width,
                    bitmaps[i].Height,
                    0,
                    PixelFormat.Bgra,
                    PixelType.UnsignedByte,
                    data.Scan0);

                    if (UseMipmap) GL.GenerateMipmap(GenerateMipmapTarget.TextureCubeMap);

                    bitmaps[i].UnlockBits(data);
                }
            }
            else
            {
                GL.BindTexture(Target, 0);
                GL.DeleteTexture(texture);
                texture = -1;
                return Result.ObjectIsNull;
            }

            GL.BindTexture(Target, 0);

            Loaded = true;
            return Result.Success;
        }

        public override Result Unload()
        {
            if (!Loaded) return Result.NotLoaded;

            GL.DeleteTexture(texture);
            texture = -1;

            if (SrcBitmap != null)
            {
                if (flipY) SrcBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            }
            else if (SrcBitmapNegX != null && SrcBitmapNegY != null && SrcBitmapNegZ != null && SrcBitmapPosX != null && SrcBitmapPosY != null && SrcBitmapPosZ != null)
            {
                var bitmaps = new Bitmap[]
                {
                    SrcBitmapNegX,
                    SrcBitmapNegY,
                    SrcBitmapNegZ,
                    SrcBitmapPosX,
                    SrcBitmapPosY,
                    SrcBitmapPosZ,
                };
                for (var i = 0; i < bitmaps.Length; i++)
                {
                    if (flipY) bitmaps[i].RotateFlip(RotateFlipType.RotateNoneFlipY);
                }
            }

            Loaded = false;
            return Result.Success;
        }
    }
}
