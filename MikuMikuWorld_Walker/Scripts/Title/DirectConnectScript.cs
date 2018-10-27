using MikuMikuWorld.Assets;
using MikuMikuWorld.Controls;
using MikuMikuWorld.GameComponents;
using MikuMikuWorld.Networks;
using MikuMikuWorld.Walker;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MikuMikuWorld.Scripts
{
    class DirectConnectScript : DrawableGameComponent
    {
        public bool AcceptInput { get; set; } = true;

        List<Control> Controls = new List<Control>();

        MenuInputResolver input;
        TransitControl transit;

        float transition = 0.0f;
        bool trans = false;

        protected override void OnLoad()
        {
            base.OnLoad();

            MMW.FindGameComponent<BackgroundScript>().Trans(new Color4(148, 222, 148, 255), 0.25);

            input = new MenuInputResolver();
            input.Up = Key.W;
            input.Down = Key.S;
            input.Right = Key.D;
            input.Left = Key.A;

            transit = new TransitControl();
            transit.LocalLocation = new Vector2(MMW.ClientSize.Width * 2.0f, 0);
            transit.Size = new Vector2(MMW.ClientSize.Width, MMW.ClientSize.Height);
            transit.Target = Vector2.Zero;

            var label = new Label()
            {
                Parent = transit,
                Alignment = ContentAlignment.TopCenter,
                Text = "DIRECT CONNECT",
                Font = new Font("Yu Gothic UI Light", 40.0f),
                LocalLocation = new Vector2(0.0f, 32.0f),
            };
            Controls.Add(label);

            var labelHost = new Label(transit, "Host", new Vector2(MMW.Width * 0.5f - 160.0f, 300.0f + 2.0f));
            Controls.Add(labelHost);
            var labelPort = new Label(labelHost, "Port", new Vector2(0, 50.0f));
            Controls.Add(labelPort);

            var textBoxHost = new TextBox2(transit, "localhost", new Vector2(MMW.Width * 0.5f - 100.0f, 300.0f), new Vector2(360.0f, 32.0f));
            textBoxHost.MaxLength = 64;
            Controls.Add(textBoxHost);

            var textBoxPort = new TextBox2(textBoxHost, "39393", new Vector2(0, 50.0f), new Vector2(72.0f, 32.0f));
            textBoxPort.MaxLength = 5;
            Controls.Add(textBoxPort);

            var backBtn = new Button(transit, "Back", new Vector2(-70.0f - 20.0f, 440.0f), "back");
            backBtn.Alignment = ContentAlignment.TopCenter;
            backBtn.Clicked += (s, e) =>
            {
                trans = true;
                transit.Target = new Vector2(-MMW.ClientSize.Width * 2.0f, 0.0f);
                GameObject.AddComponent<ServerSelectScript>();
            };
            Controls.Add(backBtn);

            var labelInfo = new Label(transit, "", new Vector2(0.0f, 500.0f));
            labelInfo.Alignment = ContentAlignment.TopCenter;
            Controls.Add(labelInfo);

            var connectBtn = new Button(transit, "Connect", new Vector2(70.0f + 20.0f, 440.0f), "click");
            connectBtn.Alignment = ContentAlignment.TopCenter;
            connectBtn.Clicked += (s, e) =>
            {
                AcceptInput = false;

                labelInfo.Brush = Brushes.White;
                labelInfo.Text = "Connecting...";

                var host = textBoxHost.Text;
                var port = int.Parse(textBoxPort.Text);

                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(100);
                    var server = NetworkUtil.Connect(host, port);

                    if (server == null)
                    {
                        labelInfo.Brush = Brushes.Red;
                        labelInfo.Text = "Host not Found.";
                        AcceptInput = true;
                    }
                    else
                    {
                        labelInfo.Text = "Downloading World info...";

                        var info = server.RequestJson<WorldInfo>(DataType.RequestWorldInfo, 1, DataType.ResponseWorldInfo, 5000);

                        if (info.GameType != 1)
                        {
                            labelInfo.Brush = Brushes.Red;
                            labelInfo.Text = "Host not Found.";
                            AcceptInput = true;
                        }
                        else if ((int)info.Version != (int)MMW.GetAsset<GameData>().Version)
                        {
                            labelInfo.Brush = Brushes.Red;
                            labelInfo.Text = $"Wrong version. Host ver.{info.Version:0.0}";
                            AcceptInput = true;
                        }
                        else
                        {
                            MMW.Invoke(() =>
                            {
                                trans = true;
                                transit.Target = new Vector2(-MMW.ClientSize.Width * 2.0f, 0.0f);
                                GameObject.AddComponent<ServerInfoScript>(server, info);
                            });
                        }
                    }
                });
            };
            Controls.Add(connectBtn);
        }

        protected override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            transition += (trans ? -1.0f : 1.0f) * (float)deltaTime * 5.0f;
            transition = MMWMath.Saturate(transition);

            transit.Update(deltaTime);

            if (AcceptInput && !trans)
            {
                input.Update(deltaTime);

                Controls.ForEach(c => c.Update(null, deltaTime));

                if (input.IsBack)
                {
                    MMW.GetAsset<Sound>("back").Play();
                    trans = true;
                    transit.Target = new Vector2(-MMW.ClientSize.Width * 2.0f, 0.0f);
                    GameObject.AddComponent<ServerSelectScript>();
                }
            }

            if (trans && transition < 0.01f) Destroy();
        }

        protected override void Draw(double deltaTime, Camera camera)
        {
            var g = Drawer.BindGraphicsDraw();

            Controls.ForEach(c => c.Draw(g, deltaTime));

            Drawer.IsGraphicsUsed = true;
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            Controls.ForEach(c => c.Unload());
        }

        public override GameComponent Clone()
        {
            throw new NotImplementedException();
        }
    }
}
