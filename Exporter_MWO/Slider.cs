using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MikuMikuWorld;

namespace Exporter_MMW
{
    public partial class Slider : UserControl
    {
        public Slider()
        {
            InitializeComponent();
            DoubleBuffered = true;
        }

        private float value = 0.0f;
        public float Value
        {
            get { return value; }
            set { this.value = value; Invalidate(); ValueChanged(this, value); }
        }

        public float MinValue { get; set; } = 0.0f;
        public float MaxValue { get; set; } = 1.0f;

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            var g = e.Graphics;

            g.DrawLine(Pens.Gray, 8.0f, Size.Height / 2 + 1, Size.Width - 8.0f, Size.Height / 2 + 1);
            g.DrawLine(Pens.Gray, 8.0f, Size.Height / 2, Size.Width - 8.0f, Size.Height / 2);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            var rate = 1.0f - (MaxValue - Value) / (MaxValue - MinValue);
            rate = Clamp(rate, 0.0f, 1.0f);
            var x = Lerp(0.0f, Size.Width - 17.0f, rate);
            

            g.FillEllipse(Brushes.White, x, 0.0f, 16.0f, 16.0f);
            g.DrawEllipse(Pens.Gray, x, 0.0f, 16.0f, 16.0f);
        }
        private float Lerp(float from, float to, float rate)
        {
            return from + (to - from) * rate;
        }
        private float Clamp(float v, float min, float max)
        {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }

        private bool drag = false;
        private void Slider_MouseDown(object sender, MouseEventArgs e)
        {
            drag = true;

            var rate = (e.X - 8.0f) / (Size.Width - 16.0f);
            rate = Clamp(rate, 0.0f, 1.0f);

            Value = Lerp(MinValue, MaxValue, rate);
            Invalidate();
        }
        private void Slider_MouseMove(object sender, MouseEventArgs e)
        {
            if (drag)
            {
                var rate = (e.X - 8.0f) / (Size.Width - 16.0f);
                rate = Clamp(rate, 0.0f, 1.0f);

                Value = Lerp(MinValue, MaxValue, rate);
                Invalidate();
            }
        }
        private void Slider_MouseUp(object sender, MouseEventArgs e)
        {
            drag = false;
        }

        public event EventHandler<float> ValueChanged = delegate { };
    }
}
