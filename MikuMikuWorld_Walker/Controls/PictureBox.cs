using MikuMikuWorld.Assets;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Controls
{
    enum SizeMode
    {
        Normal,
        Zoom,
        Stretch,
        Center,
    }

    class PictureBox : Control
    {
        public bool DrawFrame { get; set; } = false;
        private Bitmap image;
        public Bitmap Image
        {
            get { return image; }
            set
            {
                image = value;
                if (image != null)
                {
                    MMW.Invoke(() =>
                    {
                        if (tex != null && tex.Loaded) tex.Unload();
                        tex = new Texture2D(image);
                        tex.Load();
                    });
                }
            }
        }
        private Texture2D tex;
        public SizeMode SizeMode { get; set; } = SizeMode.Stretch;

        public PictureBox() { }
        public PictureBox(Control parent, Bitmap img) : this(parent, img, Vector2.Zero, new Vector2(img.Width, img.Height)) { }
        public PictureBox(Control parent, Bitmap img, Vector2 size) : this(parent, img, Vector2.Zero, size) { }
        public PictureBox(Control parent, Bitmap img, Vector2 location, Vector2 size)
        {
            Parent = parent;
            Image = img;
            LocalLocation = location;
            Size = size;
        }

        public override void Draw(Graphics g, double deltaTime)
        {
            var pos = WorldLocation + GetLocation(Size.X, Size.Y, Alignment);

            if (tex == null)
            {
                var s = g.MeasureString("No Image", DefaultFont);
                g.DrawString("No Image", DefaultFont, Brushes.White, pos.X + (Size.X - s.Width) * 0.5f, pos.Y + (Size.Y - s.Height) * 0.5f);
                return;
            } 
            
            if (SizeMode == SizeMode.Stretch)
            {
                Drawer.DrawTextureScaled(tex, pos.X, pos.Y, Size.X, Size.Y, Color4.White);
                //g.DrawImage(Image, pos.X, pos.Y, Size.X, Size.Y);
            }
            else if (SizeMode == SizeMode.Normal)
            {
                Drawer.DrawTextureScaled(tex, pos.X, pos.Y, Size.X, Size.Y, Color4.White);
                //g.DrawImageUnscaled(Image, (int)pos.X, (int)pos.Y, (int)Size.X, (int)Size.Y);
            }
            else if (SizeMode == SizeMode.Zoom)
            {
                var w = (float)Image.Width;
                var h = (float)Image.Height;

                var sx = Size.X;
                var sy = Size.Y;

                if ((w / h) < (sx / sy))
                {
                    var y = sy;
                    var x = w * (sy / h);
                    Drawer.DrawTextureScaled(tex, pos.X + (sx - x) * 0.5f, pos.Y, x, y, Color4.White);
                    //g.DrawImage(Image, pos.X + (sx - x) * 0.5f, pos.Y, x, y);
                }
                else
                {
                    var x = sx;
                    var y = h * (sx / w);
                    Drawer.DrawTextureScaled(tex, pos.X, pos.Y + (sy - y) * 0.5f, x, y, Color4.White);
                    //g.DrawImage(Image, pos.X, pos.Y + (sy - y) * 0.5f, x, y);
                }
            }
        }
    }
}
