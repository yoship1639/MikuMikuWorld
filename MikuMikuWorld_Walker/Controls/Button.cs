using MikuMikuWorld.Assets;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Controls
{
    class Button : Control
    {
        public Font Font { get; set; } = DefaultFont;
        public string Text { get; set; } = "Button";
        public Color4 BackBrush { get; set; } = Color.FromArgb(255, 128, 128, 128);
        public Color4 BackBrushDisabled { get; set; } = Color.FromArgb(255, 64, 64, 64);
        public Color4 BackBrushFocus { get; set; } = Color.FromArgb(255, 150, 180, 180);
        public Brush Brush { get; set; } = Brushes.White;
        public Brush BrushDisabled { get; set; } = new SolidBrush(Color.FromArgb(128, 255, 255, 255));
        public bool Enabled { get; set; } = true;
        public string Sound { get; set; }

        public Button() { }
        public Button(Control parent, string text, Vector2 location, string sound) : this(parent, text, location, new Vector2(140.0f, 32.0f), sound ) { }
        public Button(Control parent, string text, Vector2 location, Vector2 size, string sound)
        {
            Parent = parent;
            Text = text;
            LocalLocation = location;
            Size = size;
            Sound = sound;

            Clicked += (s, e) =>
            {
                if (Enabled) MMW.GetAsset<Sound>(Sound).Play();
                else MMW.GetAsset<Sound>("back").Play();
            };
        }

        public override bool IsIn(Vector2 pos)
        {
            var l = GetLocation(Size.X, Size.Y, Alignment);
            var x = l.X + WorldLocation.X;
            var y = l.Y + WorldLocation.Y;
            var w = Size.X;
            var h = Size.Y;

            return pos.X >= x && pos.X < x + w && pos.Y >= y && pos.Y < y + h;
        }

        public override void Draw(Graphics g, double deltaTime)
        {
            var s = g.MeasureString(Text, Font);
            var l = GetLocation(Size.X, Size.Y, Alignment);
            var x = l.X + WorldLocation.X;
            var y = l.Y + WorldLocation.Y;
            var w = Size.X;
            var h = Size.Y;

            var back = BackBrush;
            var brush = Brush;
            if (IsMouseOn) back = BackBrushFocus;
            if (!Enabled)
            {
                back = BackBrushDisabled;
                brush = BrushDisabled;
            }

            ControlDrawer.DrawFrame(x, y, w, h, back);

            var t = new Vector2(w - s.Width, h - s.Height) * 0.5f;
            g.DrawString(Text, Font, brush, x + t.X, y + t.Y);
        }
    }
}
