using MikuMikuWorld;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exporter_MMW
{
    static class Extensions
    {
        public static Color ToColor(this Color4f c)
        {
            return Color.FromArgb((int)(c.A * 255.0f), (int)(c.R * 255.0f), (int)(c.G * 255.0f), (int)(c.B * 255.0f));
        }
        public static Color ToColor(this Color4f c, int alpha)
        {
            return Color.FromArgb(alpha, (int)(c.R * 255.0f), (int)(c.G * 255.0f), (int)(c.B * 255.0f));
        }
        public static Color4f ToColor4f(this Color c)
        {
            return new Color4f(c.R / 255.0f, c.G / 255.0f, c.B / 255.0f, c.A / 255.0f);
        }
        public static Color4f ToColor4f(this Color c, int alpha)
        {
            return new Color4f(c.R / 255.0f, c.G / 255.0f, c.B / 255.0f, alpha / 255.0f);
        }


    }
}
