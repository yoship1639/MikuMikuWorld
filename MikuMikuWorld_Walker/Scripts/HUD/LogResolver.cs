using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikuMikuWorld.Assets;
using MikuMikuWorld.Controls;
using MikuMikuWorld.GameComponents;
using MikuMikuWorld.Networks;
using MikuMikuWorld.Walker;
using MikuMikuWorld.Walker.Network;
using OpenTK;
using OpenTK.Graphics;

namespace MikuMikuWorld.Scripts.HUD
{
    class LogResolver : DrawableGameComponent
    {
        class LogPanel
        {
            public string text;
            public int from;
            public Texture2D tex;
            public Texture2D icon;
            public Vector2 position;
            public Color4 color;
        }

        public override int[] TcpAcceptDataTypes => new int[] { DataType.Chat };

        public float Scroll { get; set; } = 0.0f;
        public float ScrollSpeed { get; set; } = 400.0f;
        public float Interval { get; set; } = 4.0f;
        public Vector2 Position { get; set; } = new Vector2(10.0f, 160.0f);
        public Vector2 Size { get; set; } = new Vector2(480.0f, MMW.Height - 320.0f);
        public Font Font { get; set; } = ControlDrawer.fontSmallB;
        public double ShowTime { get; set; } = 10.0;
        private double showtime = 0;
        List<LogPanel> logs = new List<LogPanel>();
        private Server server;
        private WorldData worldData;


        protected override void OnLoad()
        {
            Layer = LayerUI;
            worldData = MMW.GetAsset<WorldData>();
            server = MMW.GetAsset<Server>();
        }
        protected override void OnReceivedMessage(string message, params object[] args)
        {
            if (message == "chat")
            {
                showtime = ShowTime;

                var chat = (NwChat)args[0];
                MMW.Invoke(() =>
                {
                    var player = worldData.Players.Find(p => p.SessionID == chat.From);
                    var panel = new LogPanel()
                    {
                        text = chat.Text,
                        from = chat.From,
                        tex = Drawer.CreateStringTexture((player == null ? "" : player.Name + ": ") + chat.Text, Font, (int)Size.X - 24),
                        position = new Vector2(800.0f, 0.0f),
                        color = Color4.White,
                    };
                    logs.Add(panel);
                });
            }
            else if (message == "log")
            {
                showtime = ShowTime;

                MMW.Invoke(() =>
                {
                    var panel = new LogPanel()
                    {
                        text = (string)args[0],
                        tex = Drawer.CreateStringTexture((string)args[0], Font, (int)Size.X - 24),
                        position = new Vector2(800.0f, 0.0f),
                        color = Color4.White,
                    };
                    logs.Add(panel);
                });
            }
            else if (message == "log showtime")
            {
                try { ShowTime = (double)args[0]; }
                catch { }
            }
            else if (message == "log font")
            {
                try { Font = (Font)args[0]; }
                catch { }
            }
            else if (message == "hud show")
            {
                showtime = ShowTime;
            }
            else if (message == "hud hide")
            {
                showtime = 0;
            }
            else if (message == "enable log")
            {
                Enabled = true;
            }
            else if (message == "disable log")
            {
                Enabled = false;
            }
        }

        protected override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            showtime -= deltaTime;

            var l = logs.Count - 1;
            for (var i = l; i > (l - 30) && i >= 0; i--)
            {
                logs[i].position.X = MMWMath.Lerp(logs[i].position.X, 8.0f, MMWMath.Saturate((float)deltaTime * 12.0f));
            }

            if (Input.Ctrl && Input.IsKeyDown(OpenTK.Input.Key.Down))
            {
                Scroll -= (float)deltaTime * ScrollSpeed;
                showtime = ShowTime;
            }
            else if (Input.Ctrl && Input.IsKeyDown(OpenTK.Input.Key.Up))
            {
                Scroll += (float)deltaTime * ScrollSpeed;
                showtime = ShowTime;
            }

            var max = logs.Sum(log => log.tex.Size.Height + Interval) + 8.0f - Size.Y;
            if (max < 0.0f) max = 0.0f;
            Scroll = MMWMath.Clamp(Scroll, 0.0f, max);
        }

        protected override void Draw(double deltaTime, Camera camera)
        {
            if (showtime < 0) return;

            //ControlDrawer.DrawFrame(Position.X, Position.Y, Size.X, Size.Y);

            var height = 8.0f;
            var count = logs.Count - 1;
            Drawer.SetClip(Drawer.GetGraphics(), Position.X + 8.0f, Position.Y + 8.0f, Size.X - 16.0f, Size.Y - 16.0f);
            for (var i = count; i >= 0; i--)
            {
                var x = Position.X + logs[i].position.X;
                var y = Position.Y + Size.Y - height - logs[i].tex.Size.Height + Scroll;
                Drawer.DrawTexturePixeledAlignment(logs[i].tex, ContentAlignment.TopLeft, x, y, logs[i].color);
                height += logs[i].tex.Size.Height + Interval;
                if (height - Scroll > Size.Y) break;
            }
            Drawer.ResetClip(Drawer.GetGraphics());
        }

        public override void OnTcpReceived(int dataType, byte[] data)
        {
            if (dataType == DataType.Chat)
            {
                var chat = Util.DeserializeJson<NwChat>(data.ToJson());
                OnReceivedMessage("chat", chat);
            }
        }
    }
}
