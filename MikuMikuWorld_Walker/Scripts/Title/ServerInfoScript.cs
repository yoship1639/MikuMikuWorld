using System;
using MikuMikuWorld.GameComponents;
using MikuMikuWorld.Networks;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK;
using MikuMikuWorld.Controls;
using MikuMikuWorld;
using System.Drawing;
using System.Threading.Tasks;
using System.Threading;
using MikuMikuWorld.Network;
using MikuMikuWorld.Walker.Network;
using System.Collections.Generic;
using MikuMikuWorld.Walker;
using MikuMikuWorld.Assets;

namespace MikuMikuWorld.Scripts
{
    class ServerInfoScript : DrawableGameComponent
    {
        public override int[] TcpAcceptDataTypes => new int[] { DataType.LoginResult };

        public bool AcceptInput { get; set; } = true;
        public Server Server { get; private set; }
        public WorldInfo Info { get; private set; }

        private List<Control> Controls = new List<Control>();
        Label labelInfo;

        MenuInputResolver input;
        TransitControl transit;

        float transition = 0.0f;
        bool trans = false;

        public ServerInfoScript(WorldInfo info)
        {
            Server = MMW.GetAsset<Server>();
            Info = info;
        }

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

            var labelTitle = new Label()
            {
                Parent = transit,
                Alignment = ContentAlignment.TopCenter,
                Text = "WORLD INFO",
                Font = new Font("Yu Gothic UI Light", 40.0f),
                LocalLocation = new Vector2(0.0f, 32.0f),
            };
            Controls.Add(labelTitle);

            var imgWorld = new PictureBox()
            {
                Parent = transit,
                Image = Util.FromBitmapString(Info.WorldImage),
                LocalLocation = new Vector2(100.0f, 0.0f),
                Size = new Vector2((MMW.Width * 0.5f) - 200.0f, MMW.Height - 280.0f),
                Alignment = ContentAlignment.MiddleLeft,
                SizeMode = SizeMode.Zoom,
            };
            Controls.Add(imgWorld);

            var labelWorldNameTitle = new Label()
            {
                Parent = transit,
                Text = "World Name",
                LocalLocation = new Vector2(MMW.Width * 0.5f, 146.0f),
                Font = new Font("Yu Gothic UI", 14.0f, FontStyle.Bold),
            };
            Controls.Add(labelWorldNameTitle);

            var textWorldName = new TextBox(transit, Info.WorldName, new Vector2(MMW.Width * 0.5f, 175.0f), new Vector2(MMW.Width * 0.5f - 100.0f, 32.0f));
            textWorldName.Readonly = true;
            Controls.Add(textWorldName);

            var labelWorldDescTitle = new Label()
            {
                Parent = transit,
                Text = "World Desc",
                LocalLocation = new Vector2(MMW.Width * 0.5f, 216.0f),
                Font = new Font("Yu Gothic UI", 14.0f, FontStyle.Bold),
            };
            Controls.Add(labelWorldDescTitle);

            var textWorldDesc = new TextBox(transit, Info.WorldDesc, new Vector2(MMW.Width * 0.5f, 245.0f), new Vector2(MMW.Width * 0.5f - 100.0f, MMW.Height - 420.0f));
            textWorldDesc.Readonly = true;
            Controls.Add(textWorldDesc);

            TextBox2 textboxPass = null;
            if (Info.WorldPass)
            {
                var labelPass = new Label(textWorldDesc, "Password", new Vector2(0.0f, MMW.Height - 420.0f + 10.0f));
                Controls.Add(labelPass);

                textboxPass = new TextBox2(labelPass, "", new Vector2(100.0f, 0.0f), new Vector2(200.0f, 32.0f));
                Controls.Add(textboxPass);
            }
            

            var btnJoin = new Button(textWorldDesc, "Join", new Vector2(MMW.Width * 0.5f - 200.0f, MMW.Height - 420.0f + 10.0f), new Vector2(100.0f, 32.0f), "click");
            Controls.Add(btnJoin);

            labelInfo = new Label(btnJoin, "", new Vector2(0.0f, 30.0f));
            Controls.Add(labelInfo);

            btnJoin.Clicked += (s, e) =>
            {
                AcceptInput = false;

                var loginDesc = new LoginDesc()
                {
                    UserName = "test",
                    UserColor = new Color4f() { },
                };
                if (Info.WorldPass) loginDesc.Password = textboxPass.Text;

                //Server.LoginResultReceived += Server_LoginResultReceived;

                var json = Util.SerializeJson(loginDesc);
                Server.SendTcp(Walker.DataType.Login, json);
                //Server.ReceiveTimeout = 5 * 1000;

                labelInfo.Text = "Logging In...";
                labelInfo.Brush = Brushes.White;
            };

            
        }

        public override void OnTcpReceived(int dataType, byte[] dt)
        {
            if (dataType != DataType.LoginResult) return;

            var res = -1;
            Buffer.Read(dt, br =>
            {
                res = br.ReadInt32();
                if (res >= 0) Server.SessionID = res;
            });

            if (res >= 0)
            {
                var ls = MMW.FindGameComponent<LoadingScript>();
                ls.StartLoading((mes) =>
                {
                    MMW.DestroyGameObjects(o => o.Tags.Contains("title"));

                    mes.mes = "Download Data...";
                    var desc = Server.RequestJson<NwWorldDataDesc>(Walker.DataType.RequestDataDesc, null, Walker.DataType.ResponseDataDesc, 8000);

                    if (desc == null) return null;

                    var data = new NwWorldData();

                    data.Worlds = new NwWorld[desc.Worlds.Length];
                    for (var i = 0; i < desc.Worlds.Length; i++)
                    {
                        mes.mes = $"Download World...({i + 1}/{desc.Worlds.Length})";

                        var world = Server.RequestCompJson<NwWorld>(Walker.DataType.RequestWorld, new NwRequest(desc.Worlds[i].Hash), Walker.DataType.ResponseWorld, 10 * 60 * 1000);
                        if (world == null) return null;
                        world.Hash = desc.Worlds[i].Hash;
                        data.Worlds[i] = world;
                    }

                    data.Characters = new NwCharacter[desc.Characters.Length];
                    for (var i = 0; i < desc.Characters.Length; i++)
                    {
                        mes.mes = $"Download Character...({i + 1}/{desc.Characters.Length})";

                        var ch = Server.RequestCompJson<NwCharacter>(Walker.DataType.RequestCharacter, new NwRequest(desc.Characters[i].Hash), Walker.DataType.ResponseCharacter, 3 * 60 * 1000);
                        if (ch == null) return null;
                        ch.Hash = desc.Characters[i].Hash;
                        data.Characters[i] = ch;
                    }

                    data.Objects = new NwObject[desc.Objects.Length];
                    for (var i = 0; i < desc.Objects.Length; i++)
                    {
                        mes.mes = $"Download Object...({i + 1}/{desc.Objects.Length})";

                        var obj = Server.RequestCompJson<NwObject>(Walker.DataType.RequestObject, new NwRequest(desc.Objects[i].Hash), Walker.DataType.ResponseObject, 3 * 60 * 1000);
                        if (obj == null) return null;
                        obj.Hash = desc.Objects[i].Hash;
                        data.Objects[i] = obj;
                    }

                    data.GameObjectScripts = new NwGameObjectScript[desc.GameObjectScripts.Length];
                    for (var i = 0; i < desc.GameObjectScripts.Length; i++)
                    {
                        mes.mes = $"Download Scripts...({i + 1}/{desc.GameObjectScripts.Length})";

                        var bytes = Server.Request(Walker.DataType.RequestGameObjectScript, new NwRequest(desc.GameObjectScripts[i].Hash), Walker.DataType.ResponseGameObjectScript, 60 * 1000);
                        if (bytes == null) return null;
                        data.GameObjectScripts[i] = new NwGameObjectScript()
                        {
                            Assembly = bytes,
                            Hash = desc.GameObjectScripts[i].Hash,
                        };
                    }

                    // ゲームオブジェクトを取得
                    var objects = Server.RequestJson<NwGameObjects>(Walker.DataType.RequestGameObjects, new byte[0], Walker.DataType.ResponseGameObjects, 3 * 60 * 1000);
                    data.GameObjects = objects.GameObjects;

                    mes.mes = "Creating a world...";
                    Thread.Sleep(10);
                    return data;
                },
                (arg) =>
                {
                    if (arg == null)
                    {

                    }
                    MMW.RegistAsset(Server);
                    var go = new GameObject("Walker");
                    go.Tags.Add("walker");
                    go.Tags.Add("system");
                    go.AddComponent<WalkerScript>((NwWorldData)arg);

                    MMW.RegistGameObject(go);

                    return true;
                });
            }
            else
            {
                AcceptInput = true;
                labelInfo.Text = "Login Failed";
                labelInfo.Brush = Brushes.Red;
            }
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
