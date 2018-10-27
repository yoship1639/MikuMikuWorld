using MikuMikuWorld.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Scripts.HUD
{
    public static class Icons
    {
        public static void DrawConnection(Graphics g, float x, float y, float width, float height, int level)
        {
            if (level == 2) g.DrawImage(Resources.icon_connect, x, y, width, height);
            if (level == 1) g.DrawImage(Resources.icon_connect2, x, y, width, height);
            if (level == 0) g.DrawImage(Resources.icon_connect3, x, y, width, height);
        }
    }
}
