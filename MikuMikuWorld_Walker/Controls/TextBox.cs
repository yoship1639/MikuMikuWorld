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
    class TextBox : Control
    {
        public Font Font { get; set; } = DefaultFont;
        private string text = "Text";
        public string Text
        {
            get { return text; }
            set { text = value; TextChanged(); }
        }
        public Color4 BackBrush { get; set; } = Color.FromArgb(255, 128, 128, 128);
        public Color4 BackBrushDisabled { get; set; } = Color.FromArgb(255, 64, 64, 64);
        public Brush Brush { get; set; } = Brushes.White;
        public Brush BrushDisabled { get; set; } = new SolidBrush(Color.FromArgb(128, 255, 255, 255));
        public bool Enabled { get; set; } = true;
        public bool Readonly { get; set; } = false;
        public int MaxLength { get; set; } = 32000;

        public bool AllowAlphabet { get; set; } = true;
        public bool AllowNumber { get; set; } = true;
        public bool AllowSpecial { get; set; } = true;

        private string[] texts;
        private int seek = 0;
        private StringFormat sf = new StringFormat(StringFormatFlags.MeasureTrailingSpaces);
        private Key[] Key_AtoZ;
        private Key[] Key_0to9;
        private Key[] Key_special = new Key[] { Key.Period, Key.BackSlash };
        private TimeTrigger tt;

        public TextBox(Control parent, string text, Vector2 location, Vector2 size)
        {
            Parent = parent;
            Size = size;
            LocalLocation = location;
            Text = text;

            Key_AtoZ = new Key[26];
            for (var i = 0; i < 26; i++) Key_AtoZ[i] = (Key)(83 + i);

            Key_0to9 = new Key[10];
            for (var i = 0; i < 10; i++) Key_0to9[i] = (Key)(109 + i);

            tt = new TimeTrigger() { FirstSpan = 0.45, Span = 0.04 };
        }

        private void TextChanged()
        {
            var g = Drawer.GetGraphics();
            var se = 0;
            var c = 1;
            var list = new List<string>();

            text = text.Replace("\r\n", "\n");

            while (se + c <= text.Length)
            {
                if (se + c == text.Length)
                {
                    list.Add(text.Substring(se, c));
                    break;
                }
                if (text[se + c] == '\n')
                {
                    list.Add(text.Substring(se, c));
                    se += c + 1;
                    c = 1;
                    continue;
                }
                var le = g.MeasureString(text.Substring(se, c), Font).Width;
                if (le > Size.X - 9.0f)
                {
                    list.Add(text.Substring(se, c - 1));
                    se += c - 1;
                    c = 1;
                    continue;
                }
                c++;
            }

            texts = list.ToArray();
        }

        public override void Update(Graphics g, double deltaTime)
        {
            base.Update(g, deltaTime);

            if (FocusedControl != this) return;
            if (Readonly) return;

            if (Input.IsAnyKeyDown())
            {
                if (!tt.Trigger(deltaTime, true) && Input.PressedKeys.Length == 0) return;
                Key key = Input.DownKeys[0];
                if (Input.PressedKeys.Length > 0) key = Input.PressedKeys[0];

                int idx = -1;
                if (AllowAlphabet && Text.Length < MaxLength && (idx = Array.IndexOf(Key_AtoZ, key)) >= 0)
                {
                    char c = 'a';
                    if (Input.Shift) c = 'A';
                    c += (char)idx;
                    Text = Text.Insert(seek, c.ToString());
                    seek++;
                }
                else if (AllowNumber && Text.Length < MaxLength && (idx = Array.IndexOf(Key_0to9, key)) >= 0 && !Input.Shift)
                {
                    char c = '0';
                    c += (char)idx;
                    Text = Text.Insert(seek, c.ToString());
                    seek++;
                }
                else if (AllowSpecial && Text.Length < MaxLength && key == Key.Period)
                {
                    Text = Text.Insert(seek, ".");
                    seek++;
                }
                else if (AllowSpecial && Text.Length < MaxLength && key == Key.BackSlash && Input.Shift)
                {
                    Text = Text.Insert(seek, "_");
                    seek++;
                }
                else if (AllowSpecial && Text.Length < MaxLength && key == Key.Space)
                {
                    Text = Text.Insert(seek, " ");
                    seek++;
                }
                else if (key == Key.Enter && Text.Length < MaxLength)
                {
                    Text = Text.Insert(seek, "\r\n");
                    seek += 2;
                }
                else if (key == Key.BackSpace && Text.Length > 0 && seek > 0)
                {
                    if (Text[seek - 1] == '\n')
                    {
                        Text = Text.Remove(seek - 2, 2);
                        seek -= 2;
                        if (Text.Length > 0 && seek > 0)
                        {
                            Text = Text.Remove(seek - 1, 1);
                            seek--;
                        }
                    }
                    else
                    {
                        Text = Text.Remove(seek - 1, 1);
                        seek--;
                    }
                }
                else if (key == Key.Delete && Text.Length > 0)
                {
                    Text = "";
                    seek = 0;
                }

                else if (key == Key.Left && seek > 0) seek--;
                else if (key == Key.Right && seek < Text.Length) seek++;
            }
            else tt.Trigger(deltaTime, false);

            if (IsMouseOn && Input.IsButtonPressed(MouseButton.Left))
            {
                g = Drawer.GetGraphics();

                var x = GetLocation(Size.X, Size.Y, Alignment).X + WorldLocation.X;
                var px = Input.MousePosition.X - x;

                var idx = 0;
                while (idx < Text.Length)
                {
                    var str = new string(Text.Take(idx + 1).ToArray());
                    var w = g.MeasureString(str, Font).Width;
                    if (px < w) break;
                    idx++;
                }
                seek = idx;
            }
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
            for (var i = 0; i < texts.Length; i++)
            {
                g.DrawString(texts[i], Font, Brush, x + 6.0f, y + 3.0f + (i * 20.0f));
            }
            g.ResetClip();
            
            if (FocusedControl == this && !Readonly)
            {
                if ((MMW.TotalElapsedTime % 1.0) >= 0.30)
                {
                    var str = text.Substring(0, seek);
                    var wid = g.MeasureString(str, Font, 4000, sf);
                    var pen = new Pen(Brushes.White, 2.0f);
                    if (seek == 0) wid.Width = 6.0f;
                    g.DrawLine(pen, x + wid.Width + 2.0f, y + 6.0f, x + wid.Width + 2.0f, y + 6.0f + 20.0f);
                }
            }
        }
    }
}
