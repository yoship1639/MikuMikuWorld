using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Controls
{
    class TextBox2 : Control
    {
        public Font Font { get; set; } = DefaultFont;
        private string text = "Text";
        public string Text
        {
            get { return text; }
            set { text = value; }
        }
        public Color4 BackBrush { get; set; } = Color.FromArgb(255, 128, 128, 128);
        public Color4 BackBrushDisabled { get; set; } = Color.FromArgb(255, 64, 64, 64);
        public Brush Brush { get; set; } = Brushes.White;
        public Brush BrushDisabled { get; set; } = new SolidBrush(Color.FromArgb(128, 255, 255, 255));
        public bool Enabled { get; set; } = true;
        public bool Readonly { get; set; } = false;
        public int MaxLength { get; set; } = 32000;

        private StringFormat sf = new StringFormat(StringFormatFlags.MeasureTrailingSpaces);

        public event EventHandler TextChanged = delegate { };

        public TextBox2(Control parent, string text, Vector2 location, Vector2 size)
        {
            Parent = parent;
            Size = size;
            LocalLocation = location;
            Text = text;

            Clicked += (s, e) =>
            {
                var form = new TextInputForm("", MaxLength, true);
                form.InputText = Text;
                var res = form.ShowDialog();
                if (res == System.Windows.Forms.DialogResult.OK)
                {
                    Text = form.InputText;
                    TextChanged(this, EventArgs.Empty);
                }
            };
        }

        

        public override void Update(Graphics g, double deltaTime)
        {
            base.Update(g, deltaTime);

            if (FocusedControl != this) return;
            if (Readonly) return;

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
            if (!Enabled)
            {
                back = BackBrushDisabled;
                brush = BrushDisabled;
            }
            ControlDrawer.DrawFrame(x, y, w, h, back);

            g.SetClip(new RectangleF(x + 1, y + 1, w - 3 - 6, h - 3));
            g.DrawString(Text, Font, Brush, new RectangleF(x + 6.0f, y + 3.0f, w - 13.0f, h - 7.0f));
            //for (var i = 0; i < texts.Length; i++)
            //{
            //    g.DrawString(texts[i], Font, Brush, x + 6.0f, y + 3.0f + (i * 20.0f));
            //}
            g.ResetClip();
        }
    }
}
