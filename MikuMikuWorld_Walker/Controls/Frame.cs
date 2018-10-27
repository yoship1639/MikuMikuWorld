using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Controls
{
    class Frame : Control
    {
        public Frame(Control parent, Vector2 location, Vector2 size)
        {
            Parent = parent;
            LocalLocation = location;
            Size = size;
        }

        public override void Draw(Graphics g, double deltaTime)
        {
            var align = GetLocation(Size.X, Size.Y, Alignment);
            ControlDrawer.DrawFrame(align.X + WorldLocation.X, align.Y + WorldLocation.Y, Size.X, Size.Y);
        }
    }
}
