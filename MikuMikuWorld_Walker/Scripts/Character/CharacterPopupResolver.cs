using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikuMikuWorld.Assets;
using MikuMikuWorld.GameComponents;
using OpenTK;
using OpenTK.Graphics;

namespace MikuMikuWorld.Scripts.Character
{
    class CharacterPopupResolver : DrawableGameComponent
    {
        private Texture2D picTex;
        private double showTime = 0.0;
        private bool flipY = false;
        private float height;
        public double ShowTime { get; set; } = 8.0;
        public Vector2 Size { get; set; } = new Vector2(192, 192);

        protected override void OnLoad()
        {
            Layer = LayerUI + 1;

            picTex = new Texture2D(64, 64);
            picTex.MagFilter = OpenTK.Graphics.OpenGL4.TextureMagFilter.Nearest;
            picTex.MinFilter = OpenTK.Graphics.OpenGL4.TextureMinFilter.Nearest;
            picTex.Load();

            height = GameObject.GetComponent<CharacterInfo>().Character.Height;
        }

        protected override void Draw(double deltaTime, Camera camera)
        {
            showTime -= deltaTime;

            if (showTime > 0.0)
            {
                var wp = GameObject.Transform.WorldPosition;
                wp += new Vector3(0.0f, height + 0.2f, 0.0f);
                var pos = Util.ToScreenPos(wp, camera.View, camera.Projection, (int)MMW.Width, (int)MMW.Height);
                if (!float.IsNaN(pos.X))
                {
                    Drawer.DrawTextureScaled(picTex, pos.X - Size.X * 0.5f, pos.Y - Size.Y, Size.X, Size.Y, Color4.White, -1, flipY);
                }
                
            }
        }

        protected override void OnReceivedMessage(string message, params object[] args)
        {
            if (message == "picture chat")
            {
                if ((int)args[0] != (int)GameObject.Properties["sessionID"]) return;
                var data = (Color[,])args[1];
                var bytes = new byte[64 * 64 * 4];
                for (var y = 0; y < 64; y++)
                {
                    for (var x = 0; x < 64; x++)
                    {
                        var c = data[y, x];
                        bytes[((y * 64) + x) * 4 + 0] = data[y, x].R;
                        bytes[((y * 64) + x) * 4 + 1] = data[y, x].G;
                        bytes[((y * 64) + x) * 4 + 2] = data[y, x].B;
                        bytes[((y * 64) + x) * 4 + 3] = data[y, x].A;
                    }
                }
                MMW.Invoke(() =>
                {
                    picTex.Update(bytes, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, OpenTK.Graphics.OpenGL4.PixelType.UnsignedByte);
                    showTime = ShowTime;
                    flipY = true;
                });
            }
            else if (message == "stamp")
            {

            }
            else if (message == "enable popup")
            {
                Enabled = true;
            }
            else if (message == "disable popup")
            {
                Enabled = false;
            }
        }
    }
}
