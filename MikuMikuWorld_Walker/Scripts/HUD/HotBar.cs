using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikuMikuWorld.Assets;
using MikuMikuWorld.GameComponents;
using MikuMikuWorld.Properties;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

namespace MikuMikuWorld.Scripts.HUD
{
    class HotBar : DrawableGameComponent
    {
        private Lerper lerp;
        private UserData userData;
        private WorldResources resources;

        public bool IsShown { get; private set; } = false;
        public int BoxSize { get; set; } = 64;

        Texture2D tex;

        protected override void OnLoad()
        {
            base.OnLoad();

            Layer = LayerUI;

            lerp = new Lerper(MMW.ClientSize.Height + 40.0f);

            execs.Add("Show", (obj, args) =>
            {
                Show();
                return true;
            });
            execs.Add("Hide", (obj, args) =>
            {
                Hide();
                return true;
            });

            tex = new Texture2D(Resources.mmw_icon);
            tex.Load();

            userData = MMW.GetAsset<UserData>();
            resources = MMW.GetAsset<WorldResources>();

            Show();
        }

        public void Show()
        {
            lerp.Target = MMW.Height - 100.0f;
            IsShown = true;
        }
        public void Hide()
        {
            lerp.Target = MMW.Height + 40.0f;
            IsShown = false;
        }

        protected override void Update(double deltaTime)
        {
            if (Input.IsKeyPressed(Key.Number1) && 0 < userData.MaxHotbatItemCount) userData.ItemSelectIndex = 0;
            if (Input.IsKeyPressed(Key.Number2) && 1 < userData.MaxHotbatItemCount) userData.ItemSelectIndex = 1;
            if (Input.IsKeyPressed(Key.Number3) && 2 < userData.MaxHotbatItemCount) userData.ItemSelectIndex = 2;
            if (Input.IsKeyPressed(Key.Number4) && 3 < userData.MaxHotbatItemCount) userData.ItemSelectIndex = 3;
            if (Input.IsKeyPressed(Key.Number5) && 4 < userData.MaxHotbatItemCount) userData.ItemSelectIndex = 4;
            if (Input.IsKeyPressed(Key.Number6) && 5 < userData.MaxHotbatItemCount) userData.ItemSelectIndex = 5;
            if (Input.IsKeyPressed(Key.Number7) && 6 < userData.MaxHotbatItemCount) userData.ItemSelectIndex = 6;
            if (Input.IsKeyPressed(Key.Number8) && 7 < userData.MaxHotbatItemCount) userData.ItemSelectIndex = 7;
            if (Input.IsKeyPressed(Key.Number9) && 8 < userData.MaxHotbatItemCount) userData.ItemSelectIndex = 8;

            if (Input.MouseWheel > 0) userData.ItemSelectIndex = MMWMath.Repeat(userData.ItemSelectIndex + 1, 0, userData.MaxHotbatItemCount - 1);
            if (Input.MouseWheel < 0) userData.ItemSelectIndex = MMWMath.Repeat(userData.ItemSelectIndex - 1, 0, userData.MaxHotbatItemCount - 1);
        }

        protected override void Draw(double deltaTime, Camera camera)
        {
            lerp.Update(deltaTime);

            var g = Drawer.GetGraphics();

            DrawHotBar(g);

            Drawer.IsGraphicsUsed = true;
        }

        private Brush brush = new SolidBrush(Color.FromArgb(192, 0, 0, 0));
        private Font font = new Font("Yu Gothic UI", 8.0f);
        private void DrawHotBar(Graphics g)
        {
            var allx = BoxSize * userData.MaxHotbatItemCount + 4 + userData.MaxHotbatItemCount - 1;
            var ally = BoxSize + 4;
            var px = (MMW.Width - allx) * 0.5f;

            if (userData.HotbarItems[userData.ItemSelectIndex] != null)
            {
                var name = userData.HotbarItems[userData.ItemSelectIndex].Info.Name;
                var w = g.MeasureString(name, Controls.ControlDrawer.fontSmall).Width;
                g.DrawString(name, Controls.ControlDrawer.fontSmall, Brushes.White, (MMW.Width - w) * 0.5f, lerp.Now - 28.0f);
            }

            Color4 color = new Color4(0.25f, 0.25f, 0.25f, 0.75f);
            for (var i = 0; i < userData.MaxHotbatItemCount; i++)
            {
                color = new Color4(0.25f, 0.25f, 0.25f, 0.75f);
                if (userData.HotbarItems[i] != null && !resources.Objects.ContainsKey(userData.HotbarItems[i].Info.Hash)) color = new Color4(0.5f, 0.25f, 0.25f, 0.75f);
                Drawer.FillRect(new Vector2(px + (BoxSize * i) + i + 1, lerp.Now + 2.0f), new Vector2(BoxSize, BoxSize), color);
            }

            Drawer.DrawRect(new Vector2(px, lerp.Now), new Vector2(allx, ally), Color4.Gray);
            Drawer.DrawRect(new Vector2(px + 1.0f, lerp.Now + 1.0f), new Vector2(allx - 2.0f, ally - 2.0f), Color4.White);

            Drawer.DrawRect(new Vector2(px + userData.ItemSelectIndex * (BoxSize + 1.0f), lerp.Now), new Vector2(BoxSize + 3.0f, ally), Color4.White);

            color = Color4.Gray;
            if (userData.HotbarItems[userData.ItemSelectIndex] != null && !resources.Objects.ContainsKey(userData.HotbarItems[userData.ItemSelectIndex].Info.Hash)) color = new Color4(0.5f, 0.25f, 0.25f, 1.0f);
            Drawer.FillRect(new Vector2(px + 2.0f + userData.ItemSelectIndex * (BoxSize + 1.0f), lerp.Now + 1.0f), new Vector2(BoxSize + 1.0f, BoxSize + 1.0f), color);

            for (var i = 0; i < userData.MaxHotbatItemCount - 1; i++)
            {
                var p = i * (BoxSize + 1) + px + (BoxSize + 2);
                //g.DrawLine(Pens.White, p, lerp.Now + 1.0f, p, lerp.Now + BoxSize + 1.0f);
                Drawer.DrawLine2D(new Vector2(p, lerp.Now + 1.0f), new Vector2(p, lerp.Now + BoxSize + 3.0f), Color4.White);
            }

            for (var i = 0; i < userData.MaxHotbatItemCount; i++)
            {
                if (userData.HotbarItems[i] == null) continue;
                if (userData.HotbarItems[i].Info.Icon == null) continue;

                g.DrawImage(userData.HotbarItems[i].Info.bitmap, px + 2.0f + i * (BoxSize + 1.0f), lerp.Now + 2.0f, BoxSize, BoxSize);
            }

            for (var i = 0; i < userData.MaxHotbatItemCount; i++)
            {
                var p = i * (BoxSize + 1) + px + 3;
                g.DrawString((i + 1).ToString(), font, Brushes.White, p, lerp.Now + 3.0f);

                if (userData.HotbarItems[i] != null && userData.HotbarItems[i].Number > 1)
                {
                    var s = g.MeasureString(userData.HotbarItems[i].Number.ToString(), Controls.ControlDrawer.fontSmallB);
                    g.DrawString(userData.HotbarItems[i].Number.ToString(), Controls.ControlDrawer.fontSmallB, Brushes.White, p + BoxSize - s.Width, lerp.Now + 3.0f + BoxSize - s.Height + 4.0f);
                }
            }
        }

        protected override void OnReceivedMessage(string message, params object[] args)
        {
            if (message == "hud show")
            {
                Show();
            }
            else if (message == "hud hide")
            {
                Hide();
            }
            else if (message == "enable hotbar")
            {
                Enabled = true;
            }
            else if (message == "disable hotbar")
            {
                Enabled = false;
            }
            else if (message == "show dialog")
            {
                Enabled = false;
                Hide();
            }
            else if (message == "close dialog")
            {
                Enabled = true;
                Show();
            }
            else if (message == "set selected item index")
            {
                userData.ItemSelectIndex = MMWMath.Clamp((int)args[0], 0, userData.MaxHotbatItemCount - 1);
            }
        }

        public override GameComponent Clone()
        {
            throw new NotImplementedException();
        }
    }
}
