using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikuMikuWorld.GameComponents;
using OpenTK.Graphics;
using MikuMikuWorld.Controls;
using System.Drawing;
using OpenTK;
using OpenTK.Input;
using System.Threading;
using MikuMikuWorld.Networks;
using MikuMikuWorld.Assets;
using MikuMikuWorld.Walker;

namespace MikuMikuWorld.Scripts
{
    class ServerSelectScript : DrawableGameComponent
    {
        public bool AcceptInput { get; set; } = true;

        private List<Control> Controls = new List<Control>();

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
                Text = "WORLD SELECT",
                Font = new Font("Yu Gothic UI Light", 40.0f),
                LocalLocation = new Vector2(0.0f, 32.0f),
            };
            Controls.Add(label);

            var frame = new SelectWorldFrame(transit, MMW.ClientSize.Height - 120 - 100, new Vector2((MMW.ClientSize.Width - 760) * 0.5f, 120));
            Controls.Add(frame);
            foreach (var desc in MMW.GetAsset<UserData>().SignupServers)
            {
                frame.AddWorld(desc);
            }

            var signupBtn = new Button(frame, "SignUp World", new Vector2(70, MMW.ClientSize.Height - 120 - 100 + 20), new Vector2(140, 32), "click");
            Controls.Add(signupBtn);

            var editBtn = new Button(signupBtn, "Edit", new Vector2(160, 0), new Vector2(140, 32), "click");
            editBtn.Enabled = false;
            Controls.Add(editBtn);

            var delBtn = new Button(editBtn, "Delete", new Vector2(160, 0), new Vector2(140, 32), "click");
            delBtn.Enabled = false;
            Controls.Add(delBtn);

            var directBtn = new Button(delBtn, "Direct Connect", new Vector2(160, 0), new Vector2(140, 32), "click");
            Controls.Add(directBtn);

            signupBtn.Clicked += (s, b) =>
            {
                if (!signupBtn.Enabled) return;
                trans = true;
                transit.Target = new Vector2(-MMW.ClientSize.Width * 2.0f, 0.0f);
                GameObject.AddComponent<SignUpServerScript>();
            };

            editBtn.Clicked += (s, b) =>
            {
                if (!editBtn.Enabled) return;
                trans = true;
                transit.Target = new Vector2(-MMW.ClientSize.Width * 2.0f, 0.0f);
                GameObject.AddComponent<ServerEditScript>(frame.WorldInfo);
            };

            delBtn.Clicked += (s, b) =>
            {
                if (!delBtn.Enabled) return;
                var res = System.Windows.Forms.MessageBox.Show(System.Windows.Forms.NativeWindow.FromHandle(MMW.Window.WindowInfo.Handle), "よろしいですか？\n(サーバ元のワールドは削除されません)", "ワールドを削除", System.Windows.Forms.MessageBoxButtons.OKCancel);
                if (res == System.Windows.Forms.DialogResult.OK)
                {
                    MMW.GetAsset<UserData>().SignupServers.Remove(frame.WorldInfo);
                    frame.RemoveWorld(frame.WorldInfo);
                    frame.WorldInfo = null;
                    editBtn.Enabled = false;
                    delBtn.Enabled = false;
                }
            };

            directBtn.Clicked += (s, b) =>
            {
                if (!directBtn.Enabled) return;
                trans = true;
                transit.Target = new Vector2(-MMW.ClientSize.Width * 2.0f, 0.0f);
                GameObject.AddComponent<DirectConnectScript>();
            };

            frame.ServerClicked += (s, e) =>
            {
                editBtn.Enabled = true;
                delBtn.Enabled = true;
            };
            frame.ServerSelected += (s, e) =>
            {
                MMW.GetAsset<Sound>("click").Play();
                var desc = frame.WorldInfo;
                Task.Factory.StartNew(() =>
                {
                    AcceptInput = false;
                    Thread.Sleep(100);

                    var server = NetworkUtil.Connect(desc.HostName, desc.Port);
                    if (server == null) return;

                    server.Load();

                    var info = server.RequestJson<WorldInfo>(DataType.RequestWorldInfo, 1, DataType.ResponseWorldInfo, 3000);

                    if (info == null || info.GameType != 1 || (int)info.Version != (int)MMW.GetAsset<GameData>().Version)
                    {
                        AcceptInput = true;
                    }
                    else
                    {
                        MMW.Invoke(() =>
                        {
                            MMW.RegistAsset(server);
                            trans = true;
                            transit.Target = new Vector2(-MMW.ClientSize.Width * 2.0f, 0.0f);
                            GameObject.AddComponent<ServerInfoScript>(info);
                        });
                    }
                });
            };
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
                    GameObject.AddComponent<TitleScript>();
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
