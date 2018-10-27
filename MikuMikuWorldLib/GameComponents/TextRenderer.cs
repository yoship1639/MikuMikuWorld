using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.GameComponents
{
    public class TextRenderer : DrawableGameComponent
    {
        public override bool ComponentDupulication { get { return true; } }

        protected class DrawText
        {
            public string text;
            public float x;
            public float y;
        }

        protected Dictionary<int, DrawText> texts = new Dictionary<int, DrawText>();

        public Font Font { get; set; }
        public Brush Brush { get; set; }

        public void SetText(int index, string text, float x, float y)
        {
            if (texts.Keys.Contains(index))
            {
                var t = texts[index];
                t.text = text;
                t.x = x;
                t.y = y;
            }
            else
            {
                var t = new DrawText()
                {
                    text = text,
                    x = x,
                    y = y,
                };
                texts.Add(index, t);
            }
        }
        public void SetText(int index, string text)
        {
            if (texts.Keys.Contains(index))
            {
                var t = texts[index];
                t.text = text;
            }
            else
            {
                var t = new DrawText()
                {
                    text = text,
                    x = 0.0f,
                    y = 0.0f
                };
                texts.Add(index, t);
            }
        }
        public void RemoveText(int index)
        {
            texts.Remove(index);
        }

        protected internal override void OnLoad()
        {
            base.OnLoad();

            Font = new Font("Yu Gothic UI", 12.0f);
            Brush = Brushes.White;
        }

        protected internal override void Draw(double deltaTime, Camera camera)
        {
            var g = Drawer.BindGraphicsDraw();
            g.Clip = new Region(new RectangleF(0, 0, MMW.Width, MMW.Height));
            bool draw = false;
            foreach (var t in texts.Values)
            {
                if (string.IsNullOrWhiteSpace(t.text)) continue;
                if (MMW.ClientSize.Width < t.x || MMW.ClientSize.Height < t.y) continue;
                g.DrawString(t.text, Font, Brush, t.x+1, t.y+1);
                g.DrawString(t.text, Font, Brushes.Black, t.x, t.y);
                draw = true;
            }
            g.ResetClip();
            if (draw) Drawer.IsGraphicsUsed = true;
        }

        public override GameComponent Clone()
        {
            return new TextRenderer()
            {
                Font = Font,
                Brush = Brush,
                texts = new Dictionary<int, DrawText>(texts),
            };
        }
    }
}
