using MikuMikuWorld.Assets;
using MikuMikuWorld.Controls;
using MikuMikuWorld.GameComponents;
using MikuMikuWorld.GameComponents.ImageEffects;
using MikuMikuWorld.Walker;
using OpenTK;
using OpenTK.Graphics;
using System.Collections.Generic;
using System.Drawing;

namespace MikuMikuWorld.Scripts.HUD
{
    class LeaveResolver : DrawableGameComponent
    {
        private List<Control> controls = new List<Control>();
        public bool Shown { get; private set; }

        protected override void OnLoad()
        {
            Layer = LayerUI + 2;

            var label = new Label(
                null,
                "Disconnected",
                new Font("Yu Gothic UI", 32.0f),
                new Vector2((MMW.Width - Drawer.MeasureString("DISCONNECTED", new Font("Yu Gothic UI", 32.0f)).X) / 2.0f, 100.0f));
            controls.Add(label);

            var sw = Drawer.MeasureString("Can't connect to the world. Return to the Title Window.", ControlDrawer.fontSmallB).X;
            var text = new Label(null, "Can't connect to the world. Return to the Title Window.", new Vector2((MMW.Width - sw) * 0.5f, 200));
            text.Font = ControlDrawer.fontSmallB;
            controls.Add(text);

            var btnOK = new Button(null, "OK", new Vector2((MMW.Width - sw) * 0.5f, 240), "click");
            btnOK.Clicked += (s, e) =>
            {
                GameObject.SendMessage("close leave window");
                var ws = MMW.FindGameComponent<WalkerScript>();
                MMW.DestroyGameObject(ws.GameObject);

                var title = new GameObject("Title", Matrix4.Identity, "title");
                MMW.RegistGameObject(title);
                title.AddComponent<BackgroundScript>();
                title.AddComponent<TitleScript>();
                MMW.Window.CursorVisible = true;
            };
            controls.Add(btnOK);
        }

        public void Show()
        {
            Shown = true;
        }
        public void Hide()
        {
            Shown = false;
        }

        protected override void Update(double deltaTime)
        {
            if (!Shown) return;

            controls.ForEach(c => c.Update(null, deltaTime));
        }

        protected override void Draw(double deltaTime, Camera camera)
        {
            if (!Shown) return;

            Drawer.FillRect(Vector2.Zero, new Vector2(MMW.Width, MMW.Height), new Color4(0.0f, 0.0f, 0.0f, 0.1f));

            var g = Drawer.GetGraphics();
            controls.ForEach(c => c.Draw(g, deltaTime));
        }

        protected override void OnReceivedMessage(string message, params object[] args)
        {
            if (message == "show leave window")
            {
                MMW.MainCamera.GameObject.GetComponent<Blur>().Radius = 40.0f;
                MMW.Window.CursorVisible = true;
                Show();
                Enabled = true;
            }
            else if (message == "close leave window")
            {
                MMW.MainCamera.GameObject.GetComponent<Blur>().Radius = 0.0f;
                MMW.Window.CursorVisible = false;
                Hide();
                Enabled = false;
            }
        }
    }

}