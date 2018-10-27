using MikuMikuWorld.Assets;
using MikuMikuWorld.Scripts.HUD;
using MikuMikuWorld.Networks;
using MikuMikuWorld.Properties;
using MikuMikuWorldScript;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MikuMikuWorld.Walker;

namespace MikuMikuWorld.Controls
{
    class SelectWorldPanel : Control
    {
        public static SelectWorldPanel SelectedPanel { get; set; }

        private Bitmap defBackImage;
        public WorldInfo Info;
        private Timer timer;
        private Color4 backColor = new Color4(0.5f, 0.5f, 0.65f, 1.0f);
        public bool Connected { get; private set; } = false;

        Texture2D texImage;
        Texture2D texIcon;

        public void ReceivedServerDesc(WorldInfo info)
        {
            if (info == null || info.GameType != 1)
            {
                Connected = false;
                MMW.Invoke(() =>
                {
                    if (texIcon != null && texIcon.Loaded)
                    {
                        texIcon.Unload();
                        texIcon = null;
                        Info.WorldIcon = null;
                    }
                });
                return;
            } 

            if (info != null)
            {
                Connected = true;
                Info.WorldName = info.WorldName;
                Info.WorldIcon = info.WorldIcon;
                if (Info.WorldIcon != null)
                {
                    MMW.Invoke(() =>
                    {
                        if (texIcon != null && texIcon.Loaded)
                        {
                            texIcon.Unload();
                            texIcon = null;
                        }
                        var bitmap = Util.FromBitmapString(Info.WorldIcon);
                        if (bitmap != null)
                        {
                            texIcon = new Texture2D(bitmap);
                            texIcon.Load();
                        }
                    });
                }
                
            }
        }

        public SelectWorldPanel(WorldInfo info)
        {
            Info = info;
            defBackImage = Resources.mmw_defaultbackpanel;

            texImage = MMW.GetAsset<Texture2D>("default back panel");
            if (texImage == null)
            {
                texImage = new Texture2D(defBackImage);
                texImage.Name = "default back panel";
                MMW.RegistAsset(texImage);
            }
            if (!texImage.Loaded) texImage.Load();

            Clicked += (s, e) => SelectedPanel = this;

            timer = new Timer((t) =>
            {
                var data = NetworkUtil.QueryWorldInfoUdp(info.HostName, info.Port);
                ReceivedServerDesc(data);
            }, null, 0, 3000);
        }

        float rate = 0.0f;
        public override void Update(Graphics g, double deltaTime)
        {
            base.Update(g, deltaTime);

            if (IsMouseOn || SelectedPanel == this) rate = MMWMath.Lerp(rate, 1.0f, (float)deltaTime * 6.0f);
            else rate = MMWMath.Lerp(rate, 0.0f, (float)deltaTime * 6.0f);
        }

        public override void Draw(Graphics g, double deltaTime)
        {
            base.Draw(g, deltaTime);

            Vector2 sub = Size * MMWMath.Lerp(0.95f, 1.0f, rate);

            var pos = WorldLocation + (Size - sub) * 0.5f;
            var add = (SelectedPanel == this ? 0.15f : 0.0f);
            var r = MMWMath.Lerp(0.75f, 1.0f, rate);
            r += Connected ? 0.3f : 0.0f;

            Drawer.DrawTextureScaled(texImage, pos.X, pos.Y, sub.X, sub.Y, new Color4(backColor.R * r, backColor.G * r + add, backColor.B * r, 1.0f));

            if (texIcon != null && texIcon.Loaded)
            {
                var iconSize = texIcon.Size.ToVector2();
                iconSize = new Vector2(64.0f);
                var iconPos = WorldLocation + (Size - iconSize) * 0.5f;
                Drawer.DrawTextureScaled(texIcon, iconPos.X, WorldLocation.Y + 24.0f, iconSize.X, iconSize.Y, Color4.White);
            }

            var size = g.MeasureString(Info.WorldName, DefaultFont);
            g.DrawString(Info.WorldName, DefaultFont, Brushes.White, WorldLocation.X + (Size.X - size.Width) * 0.5f, WorldLocation.Y + (Size.Y - size.Height) * 0.5f + 24.0f);
            size = g.MeasureString(Info.HostName, DefaultFontS);
            g.DrawString(Info.HostName, DefaultFontS, Brushes.LightGray, WorldLocation.X + (Size.X - size.Width) * 0.5f, WorldLocation.Y + (Size.Y - size.Height) * 0.5f + 48.0f);

            Icons.DrawConnection(g, WorldLocation.X + Size.X - 50.0f, WorldLocation.Y + 20.0f, 20.0f, 20.0f, Connected ? 2 : 0);
        }

        public void Destroy()
        {
            timer.Dispose();
            if (texIcon != null && texIcon.Loaded)
            {
                texIcon.Unload();
                texIcon = null;
            }
        }
    }
}
