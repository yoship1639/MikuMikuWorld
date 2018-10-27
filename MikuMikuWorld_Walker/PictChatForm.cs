using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MikuMikuWorld
{
    partial class PictChatForm : Form
    {
        private Panel selected;
        public Color[,] Canvas = new Color[64, 64];
        private UserData userData;

        public PictChatForm(UserData userData)
        {
            this.userData = userData;
            InitializeComponent();
            DoubleBuffered = true;
            ClientSize = new Size(520, 640);

            comboBox_pics.DisplayMember = "Name";
            button_clear_Click(this, EventArgs.Empty);

            if (!userData.Achivements.Exists(a => a.DisplayName == "picture chat 2"))
            {
                var colors = new Color[]
                {
                    Color.FromArgb(230, 0, 18),
                    Color.FromArgb(243, 152, 0),
                    Color.FromArgb(255, 241, 0),
                    Color.FromArgb(143, 195, 31),
                    Color.FromArgb(0, 153, 68),
                    Color.FromArgb(0, 158, 150),
                    Color.FromArgb(0, 160, 233),
                    Color.FromArgb(0, 104, 183),
                    Color.FromArgb(29, 32, 136),
                    Color.FromArgb(146, 3, 131),
                    Color.FromArgb(228, 0, 127),
                    Color.FromArgb(229, 0, 79),
                };

                var sat = new double[]
                {
                1.0,
                0.5,
                0.25,
                };

                for (var i = 0; i < 12; i++)
                {
                    for (var s = 0; s < 3; s++)
                    {
                        var p = new Panel();
                        p.Size = new Size(20, 20);
                        p.Location = new Point(i * 24 + 4, 516 + 4 + (s * 24));
                        p.BorderStyle = BorderStyle.FixedSingle;
                        var c = colors[i];
                        var hsv = FromRGB(c);
                        hsv.Saturation = 1.0 * sat[s];
                        c = ToRGB(hsv);
                        p.BackColor = c;
                        Controls.Add(p);
                        p.Click += P_Click;
                    }
                }

                var blacks = new Color[]
                {
                Color.Black,
                Color.Gray,
                Color.FromArgb(192, 192, 192),
                };

                for (var s = 0; s < blacks.Length; s++)
                {
                    var p = new Panel();
                    p.Size = new Size(20, 20);
                    p.Location = new Point(12 * 24 + 4, 516 + 4 + (s * 24));
                    p.BorderStyle = BorderStyle.FixedSingle;
                    p.BackColor = blacks[s];
                    Controls.Add(p);
                    p.Click += P_Click;
                }

                var whites = new Color[]
                {
                    Color.White,
                    Color.FromArgb(0, 160, 160, 160),
                };

                for (var s = 0; s < whites.Length; s++)
                {
                    var p = new Panel();
                    p.Size = new Size(20, 20);
                    p.Location = new Point(13 * 24 + 4, 516 + 4 + (s * 24));
                    p.BorderStyle = BorderStyle.FixedSingle;
                    p.BackColor = whites[s];
                    Controls.Add(p);
                    p.Click += P_Click;
                }

                var se = new Panel();
                se.Size = new Size(48, 48);
                se.Location = new Point(14 * 24 + 4, 516 + 4);
                se.BorderStyle = BorderStyle.FixedSingle;
                se.BackColor = Color.Black;
                Controls.Add(se);
                selected = se;
            }
            else
            {
                var colors = new Color[]
                {
                    Color.Black,
                    Color.White,
                    Color.FromArgb(0, 160, 160, 160),
                };
                for (var i = 0; i < colors.Length; i++)
                {
                    var p = new Panel();
                    p.Size = new Size(20, 20);
                    p.Location = new Point(i * 24 + 4, 516 + 4);
                    p.BorderStyle = BorderStyle.FixedSingle;
                    p.BackColor = colors[i];
                    Controls.Add(p);
                    p.Click += P_Click;
                }

                var se = new Panel();
                se.Size = new Size(48, 48);
                se.Location = new Point(3 * 24 + 4, 516 + 4);
                se.BorderStyle = BorderStyle.FixedSingle;
                se.BackColor = Color.Black;
                Controls.Add(se);
                selected = se;
            }
        }

        private void P_Click(object sender, EventArgs e)
        {
            selected.BackColor = ((Panel)sender).BackColor;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            var g = e.Graphics;

            g.Clear(BackColor);

            //var cr = e.ClipRectangle;
            for (var y = 0; y < 64; y++)
            {
                for (var x = 0; x < 64; x++)
                {
                    g.FillRectangle(new SolidBrush(Canvas[y, x]), new Rectangle(4 + x * 8, 4 + y * 8, 8, 8));
                }
            }

            if (checkBox_drawgrid.Checked)
            {
                for (var i = 0; i <= 64; i++)
                {
                    g.DrawLine(Pens.LightGray, i * 8 + 4, 4, i * 8 + 4, 512 + 4);
                }

                for (var i = 0; i <= 64; i++)
                {
                    g.DrawLine(Pens.LightGray, 4, i * 8 + 4, 512 + 4, i * 8 + 4);
                }
            }
        }

        public static HSV FromRGB(Color color)
        {
            // R、GおよびBが0.0を最小量、1.0を最大値とする0.0から1.0の範囲にある
            var r = color.R / 255.0;
            var g = color.G / 255.0;
            var b = color.B / 255.0;

            var max = Math.Max(Math.Max(r, g), b);
            var min = Math.Min(Math.Min(r, g), b);
            var sub = max - min;

            double h = 0, s = 0, v = 0;

            // Calculate Hue
            if (sub == 0)
            {
                // MAX = MIN(例・S = 0)のとき、 Hは定義されない。
                h = 0;
            }
            else
            {
                if (max == r)
                {
                    h = (60 * (g - b) / sub) + 0;
                }
                else if (max == g)
                {
                    h = (60 * (b - r) / sub) + 120;
                }
                else if (max == b)
                {
                    h = (60 * (r - g) / sub) + 240;
                }

                // さらに H += 360 if H < 0
                if (h < 0)
                {
                    h += 360;
                }
            }

            // Calculate Saturation
            if (max > 0)
            {
                s = sub / max;
            }

            // Calculate Value
            v = max;

            return new HSV(h, s, v);
        }
        public static Color ToRGB(HSV hsv)
        {
            // まず、もしSが0.0と等しいなら、最終的な色は無色もしくは灰色である。
            if (hsv.Saturation == 0)
            {
                var val = (int)(hsv.Value * 255);
                return Color.FromArgb(val, val, val);
            }

            double r = 0, g = 0, b = 0;
            double f = 0;
            double p = 0, q = 0, t = 0;

            //var h = Math.Min(360.0, Math.Max(0, Hue));
            // 角座標系で、Hの範囲は0から360までであるが、その範囲を超えるHは360.0で
            // 割った剰余（またはモジュラ演算）でこの範囲に対応させることができる。
            // たとえば-30は330と等しく、480は120と等しくなる。
            var h = hsv.Hue % 360;
            var s = Math.Min(1.0, Math.Max(0, hsv.Saturation));
            var v = Math.Min(1.0, Math.Max(0, hsv.Value));

            var hi = (int)(h / 60);
            f = (h / 60) - hi;
            p = v * (1 - s);
            q = v * (1 - f * s);
            t = v * (1 - (1 - f) * s);

            if (hi == 0)
            {
                r = v; g = t; b = p;
            }
            else if (hi == 1)
            {
                r = q; g = v; b = p;
            }
            else if (hi == 2)
            {
                r = p; g = v; b = t;
            }
            else if (hi == 3)
            {
                r = p; g = q; b = v;
            }
            else if (hi == 4)
            {
                r = t; g = p; b = v;
            }
            else if (hi == 5)
            {
                r = v; g = p; b = q;
            }

            return Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255));
        }

        public class HSV
        {
            /// <summary>
            /// HSV クラスの新しいインスタンスを初期化します。
            /// </summary>
            /// <param name="h">色相</param>
            /// <param name="s">彩度</param>
            /// <param name="v">明度</param>
            public HSV(double h, double s, double v)
            {
                Hue = h;
                Saturation = s;
                Value = v;
            }

            /// <summary>
            /// HSV クラスの新しいインスタンスを初期化します。
            /// </summary>
            /// <param name="hsv">HSV各要素の値が格納されたdouble配列</param>
            public HSV(double[] hsv)
            {
                Hue = hsv[0];
                Saturation = hsv[1];
                Value = hsv[2];
            }

            /// <summary>
            /// 色相
            /// </summary>
            public double Hue { get; set; }

            /// <summary>
            /// 彩度
            /// </summary>
            public double Saturation { get; set; }

            /// <summary>
            /// 明度
            /// </summary>
            public double Value { get; set; }
        }

        private void PictChatForm_MouseMove(object sender, MouseEventArgs e)
        {
            if ((MouseButtons & MouseButtons.Left) == MouseButtons.Left)
            {
                var p = PointToClient(MousePosition);
                p.X -= 4;
                p.Y -= 4;

                var px = p.X / 8;
                var py = p.Y / 8;

                if (px >= 64 || py >= 64) return;

                Canvas[py, px] = selected.BackColor;
                Invalidate(new Rectangle(p.X - 16, p.Y - 16, 32, 32));
            }
        }

        private void button_clear_Click(object sender, EventArgs e)
        {
            var color = Color.White;
            if (selected != null) color = selected.BackColor;
            for (var i = 0; i < 64; i++)
            {
                for (var j = 0; j < 64; j++)
                {
                    Canvas[i, j] = color;
                }
            }

            Invalidate();
        }

        private void PictChatForm_MouseDown(object sender, MouseEventArgs e)
        {
            if ((MouseButtons & MouseButtons.Left) == MouseButtons.Left)
            {
                var p = PointToClient(MousePosition);
                p.X -= 4;
                p.Y -= 4;

                var px = p.X / 8;
                var py = p.Y / 8;

                if (px >= 64 || py >= 64) return;

                Canvas[py, px] = selected.BackColor;
                Invalidate(new Rectangle(p.X - 16, p.Y - 16, 32, 32));
            }
        }

        private void checkBox_whiteline_CheckedChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void button_save_Click(object sender, EventArgs e)
        {
            var form = new PictSaveForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                var data = new PictChatData()
                {
                    Name = form.textBox_name.Text,
                    Data = (Color[,])Canvas.Clone(),
            };
                userData.PictChats.Add(data);

                comboBox_pics.Items.Add(data);
            }
        }

        private void button_load_Click(object sender, EventArgs e)
        {
            if (comboBox_pics.SelectedItem != null)
            {
                var data = comboBox_pics.SelectedItem as PictChatData;
                Canvas = (Color[,])data.Data.Clone();
                Invalidate();
            }
        }

        private Color[,] CloneCanvas(Color[,] canvas)
        {
            var c = new Color[64, 64];

            for (var y = 0; y < 64; y++)
            {
                for (var x = 0; x < 64; x++)
                {
                    c[y, x] = canvas[y, x];
                }
            }

            return c;
        }

        public event EventHandler<Color[,]> SendClicked = delegate { };

        private void button_send_Click(object sender, EventArgs e)
        {
            SendClicked(this, (Color[,])Canvas.Clone());
        }
    }
}
