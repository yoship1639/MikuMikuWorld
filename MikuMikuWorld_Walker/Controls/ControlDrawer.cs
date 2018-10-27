using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Controls
{
    static class ControlDrawer
    {
        private static Brush focusBrush = new SolidBrush(Color.FromArgb(220, 64, 64, 64));
        private static Brush defocusBrush = new SolidBrush(Color.FromArgb(128, 0, 0, 0));

        public static Font fontSmall = new Font("Yu Gothic UI", 14.0f);
        public static Font fontSmallB = new Font("Yu Gothic UI", 14.0f, FontStyle.Bold);

        public static SizeF MeasureString(Graphics g, string text)
        {
            return g.MeasureString(text, fontSmall);
        }

        public static void DrawString(Graphics g, string text, float x, float y, bool focus = true)
        {
            if (focus) g.DrawString(text, fontSmallB, Brushes.White, x, y);
            else g.DrawString(text, fontSmall, Brushes.LightGray, x, y);
        }
        public static void DrawString(Graphics g, string text, float x, float y, Color4 color)
        {
            g.DrawString(text, fontSmall, new SolidBrush(Color.FromArgb(color.ToArgb())), x, y);
        }
        public static void DrawFrame(float x, float y, float width, float height, bool focus = true)
        {
            Drawer.FillRect(new Vector2(x, y), new Vector2(width, height), focus ? Color.FromArgb(220, 64, 64, 64) : Color.FromArgb(128, 0, 0, 0));
            Drawer.DrawRect(new Vector2(x, y), new Vector2(width, height), Color.Gray);
            Drawer.DrawRect(new Vector2(x+1, y+1), new Vector2(width-2, height-2), Color.White);
        }
        public static void DrawFrame(float x, float y, float width, float height, Color4 color)
        {
            Drawer.FillRect(new Vector2(x, y), new Vector2(width, height), color);
            Drawer.DrawRect(new Vector2(x, y), new Vector2(width, height), Color.Gray);
            Drawer.DrawRect(new Vector2(x + 1.0f, y + 1.0f), new Vector2(width - 2, height - 2), Color.White);
        }
        public static void DrawTab(Graphics g, float x, float y, float width, float height, bool focus = true)
        {
            g.Clip = new Region(new RectangleF(x, y, width, height));
            g.FillRectangle(focus ? focusBrush : defocusBrush, x, y, width, height);
            g.DrawRectangle(Pens.Gray, x, y, width - 1, height + 1);
            g.DrawRectangle(Pens.White, x + 1, y + 1, width - 3, height);
            g.Clip = new Region();
        }
        public static void DrawTabControl(Graphics g, int[] tabWidths, int tabIndex, float x, float y, float width, float height, float tabHeight = 30.0f, bool focus = true)
        {
            var offset = x;
            for (var i = 0; i < tabWidths.Length; i++)
            {
                DrawTab(g, offset, y, tabWidths[i], tabHeight, focus && tabIndex == i);
                offset += tabWidths[i];
            }

            offset = x;
            for (var i = 0; i < tabWidths.Length; i++)
            {
                if (tabIndex > i)
                {
                    g.DrawLine(Pens.Gray, offset + 1, y + tabHeight - 2, offset + tabWidths[i], y + tabHeight - 2);
                    g.DrawLine(Pens.White, offset + 1, y + tabHeight - 1, offset + tabWidths[i], y + tabHeight - 1);
                }
                else if (tabIndex < i)
                {
                    g.DrawLine(Pens.Gray, offset - 1, y + tabHeight - 2, offset + tabWidths[i], y + tabHeight - 2);
                    g.DrawLine(Pens.White, offset - 1, y + tabHeight - 1, offset + tabWidths[i], y + tabHeight - 1);
                }
                offset += tabWidths[i];
            }

            g.DrawLine(Pens.Gray, offset + 1, y + tabHeight - 2, x + width - 1, y + tabHeight - 2);
            g.DrawLine(Pens.White, offset + 1, y + tabHeight - 1, x + width - 2, y + tabHeight - 1);

            g.Clip = new Region(new RectangleF(x, y + tabHeight, width, height - tabHeight));
            DrawFrame(x, y + tabHeight - 2, width, height - tabHeight + 2, focus);
            g.Clip = new Region();
        }
        public static void DrawError(Graphics g, int x, int y, int width, int height)
        {
            g.DrawRectangle(Pens.Red, x, y, width, height);
            g.DrawLine(Pens.Red, x, y, x + width, y + height);
            g.DrawLine(Pens.Red, x, y + height, x + width, y);
        }

        public static void DrawImage(Graphics g, Image image, Vector2 pos, Vector2 size, Color4 mulColor)
        {
            DrawImage(g, image, pos.X, pos.Y, size.X, size.Y, new RectangleF(0, 0, image.Width, image.Height), mulColor, new Color4(0.0f, 0.0f, 0.0f, 0.0f));
        }
        public static void DrawImage(Graphics g, Image image, Vector2 pos, Vector2 size, Color4 mulColor, Color4 addColor)
        {
            DrawImage(g, image, pos.X, pos.Y, size.X, size.Y, new RectangleF(0, 0, image.Width, image.Height), mulColor, addColor);
        }
        public static void DrawImage(Graphics g, Image image, float x, float y, float width, float height, Color4 mulColor)
        {
            DrawImage(g, image, x, y, width, height, new RectangleF(0, 0, image.Width, image.Height), mulColor, new Color4(0.0f, 0.0f, 0.0f, 0.0f));
        }
        public static void DrawImage(Graphics g, Image image, float x, float y, float width, float height, Color4 mulColor, Color4 addColor)
        {
            DrawImage(g, image, x, y, width, height, new RectangleF(0, 0, image.Width, image.Height), mulColor, addColor);
        }
        public static void DrawImage(Graphics g, Image image, Vector2 pos, Vector2 size, RectangleF srcRect, Color4 mulColor)
        {
            DrawImage(g, image, pos.X, pos.Y, size.X, size.Y, srcRect, mulColor, new Color4(0.0f, 0.0f, 0.0f, 0.0f));
        }
        public static void DrawImage(Graphics g, Image image, Vector2 pos, Vector2 size, RectangleF srcRect, Color4 mulColor, Color4 addColor)
        {
            DrawImage(g, image, pos.X, pos.Y, size.X, size.Y, srcRect, mulColor, addColor);
        }
        public static void DrawImage(Graphics g, Image image, float x, float y, float width, float height, RectangleF srcRect, Color4 mulColor)
        {
            DrawImage(g, image, x, y, width, height, srcRect, mulColor, new Color4(0.0f, 0.0f, 0.0f, 0.0f));
        }
        public static void DrawImage(Graphics g, Image image, float x, float y, float width, float height, RectangleF srcRect, Color4 mulColor, Color4 addColor)
        {
            var cm = new ColorMatrix();
            cm.Matrix00 *= mulColor.R;
            cm.Matrix11 *= mulColor.G;
            cm.Matrix22 *= mulColor.B;
            cm.Matrix33 *= mulColor.A;
            cm.Matrix40 += addColor.R;
            cm.Matrix41 += addColor.G;
            cm.Matrix42 += addColor.B;
            cm.Matrix43 += addColor.A;
            var attr = new ImageAttributes();
            attr.SetColorMatrix(cm);

            var poses = new PointF[]
            {
                new PointF(x, y),
                new PointF(x + width, y),
                new PointF(x, y + height),
            };

            g.DrawImage(image, poses, srcRect, GraphicsUnit.Pixel, attr);
        }
    }
}
