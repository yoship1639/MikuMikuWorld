using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Controls
{
    class Label : Control
    {
        public Font Font { get; set; } = DefaultFont;
        public string Text { get; set; } = "Text";
        public Brush Brush { get; set; } = Brushes.White;

        public Label() { }
        public Label(Control parent, string text, Vector2 location) : this(parent, text, DefaultFont, location) { }
        public Label(Control parent, string text, Font font, Vector2 location)
        {
            Parent = parent;
            Text = text;
            Font = font;
            LocalLocation = location;
        }

        public override void Draw(Graphics g, double deltaTime)
        {
            var s = g.MeasureString(Text, Font);
            var l = GetLocation(s.Width, s.Height, Alignment);
            g.DrawString(Text, Font, Brush, l.X + WorldLocation.X, l.Y + WorldLocation.Y);
            Drawer.IsGraphicsUsed = true;
        }
    }
}
