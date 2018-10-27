using MikuMikuWorld.Assets;
using MikuMikuWorld.Properties;
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
    class SelectLabel : Control
    {
        public Font Font { get; set; } = DefaultFont;
        public string Text { get; set; } = "Text";
        private Brush brush = Brushes.White;
        public Brush Brush
        {
            get { return brush; }
            set { brush = value; pen = new Pen(brush); }
        }
        private Pen pen = new Pen(Brushes.White);

        private float rate;
        private Texture2D texStar;

        public SelectLabel() { }
        public SelectLabel(Control parent, string text, Vector2 location)
        {
            Parent = parent;
            Text = text;
            LocalLocation = location;

            texStar = new Texture2D(Resources.star);
            texStar.Load();
        }

        public override void Update(Graphics g, double deltaTime)
        {
            base.Update(g, deltaTime);

            if (IsMouseOn) rate = MMWMath.Lerp(rate, 1.0f, (float)deltaTime * 10.0f);
            else rate = 0.0f;
        }

        public override void Draw(Graphics g, double deltaTime)
        {
            var s = g.MeasureString(Text, Font);
            var l = GetLocation(s.Width, s.Height, Alignment);
            var pos = new Vector2(l.X + WorldLocation.X, l.Y + WorldLocation.Y);
            g.DrawString(Text, Font, Brush, pos.X, pos.Y);

            g.DrawLine(pen, pos.X, pos.Y + s.Height, MMWMath.Lerp(pos.X, pos.X + s.Width, rate), pos.Y + s.Height);

            if (rate > 0.0f)
            {
                Drawer.DrawTexturePixeledAlignment(
                texStar,
                ContentAlignment.TopLeft,
                MMWMath.Lerp(pos.X, pos.X + s.Width, rate) - texStar.Size.Width * 0.5f,
                pos.Y + s.Height - texStar.Size.Height * 0.5f,
                Color4.White,
                -(float)MMW.TotalElapsedTime * 2.0f,
                1.0f / 32.0f);
            }
        }
    }
}
