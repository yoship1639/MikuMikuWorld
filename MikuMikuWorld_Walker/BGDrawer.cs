using MikuMikuWorld.Assets;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    class BGDrawer
    {
        public Texture2D Texture { get; set; }

        public TransitColor Color { get; private set; }

        public Vector2 Speed { get; set; } = Vector2.One;
        private Vector2 offset;

        public BGDrawer(Texture2D tex)
        {
            Texture = tex;
            Color = new TransitColor(Color4.White);
        }

        public void Update(double deltaTime)
        {
            Color.Update(deltaTime);
            offset += Speed * (float)deltaTime;
            offset.X = MMWMath.Repeat(offset.X, 0.0f, Texture.Size.Width);
            offset.Y = MMWMath.Repeat(offset.Y, 0.0f, Texture.Size.Height);
        }

        public void Draw(double deltaTime)
        {
            for (var y = -1; y < MMW.ClientSize.Height / Texture.Size.Height + 2; y++)
            {
                for (var x = -1; x < MMW.ClientSize.Width / Texture.Size.Width + 2; x++)
                {
                    //Drawer.DrawTexturePixeled(Texture, offset.X + (Texture.Size.Width * x), offset.Y + (Texture.Size.Height * y), Color.Now);
                    Drawer.DrawTexturePixeledAlignment(Texture, ContentAlignment.TopLeft, offset.X + (Texture.Size.Width * x), offset.Y + (Texture.Size.Height * y), Color.Now);

                    /*
                    Drawer.DrawTexture(
                        Texture,
                        new RectangleF(0, 0, 1, 1),
                        new RectangleF(offset.X + (Texture.Size.Width * x), offset.Y + (Texture.Size.Height * y), Texture.Size.Width, Texture.Size.Height),
                        Color.Now,
                        Vector3.Zero,
                        Vector2.One,
                        Vector2.One * 0.5f);
                        */
                }
            }
        }
    }
}
