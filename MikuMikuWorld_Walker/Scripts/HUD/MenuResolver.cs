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
    class MenuResolver : DrawableGameComponent
    {
        private List<Control> controls = new List<Control>();
        private Dictionary<string, List<Control>> controlDic = new Dictionary<string, List<Control>>();
        public bool Shown { get; private set; }
        private string type = "players";
        private SimpleDrawableObject drawObj;
        private UserData userData;

        protected override void OnLoad()
        {
            Layer = LayerUI + 2;

            userData = MMW.GetAsset<UserData>();

            var label = new Label(
                null,
                "MENU",
                new Font("Yu Gothic UI", 32.0f),
                new Vector2((MMW.Width - Drawer.MeasureString("MENU", new Font("Yu Gothic UI", 32.0f)).X) / 2.0f, 100.0f));
            controls.Add(label);

            var btnShop = new Button(null, "Shop", new Vector2(200, 200), "click");
            btnShop.Clicked += (s, e) =>
            {
                type = "shop";
                MMW.BroadcastMessage("show coin resolver");
            };
            controls.Add(btnShop);

            var btnSkill = new Button(btnShop, "Skill", new Vector2(0, 50), "click");
            btnSkill.Clicked += (s, e) =>
            {
                type = "skill";
                MMW.BroadcastMessage("close coin resolver");
            };
            controls.Add(btnSkill);

            var btnPlayers = new Button(btnSkill, "Players", new Vector2(0, 50), "click");
            btnPlayers.Clicked += (s, e) =>
            {
                type = "players";
                MMW.BroadcastMessage("close coin resolver");
            };
            controls.Add(btnPlayers);

            var btnGraphics = new Button(btnPlayers, "Graphics", new Vector2(0, 50), "click");
            btnGraphics.Clicked += (s, e) =>
            {
                type = "graphics";
                MMW.BroadcastMessage("close coin resolver");
            };
            controls.Add(btnGraphics);

            var btnExit = new Button(btnGraphics, "Leave", new Vector2(0, 50), "click");
            btnExit.Clicked += (s, e) =>
            {
                type = "leave";
                MMW.BroadcastMessage("close coin resolver");
            };
            controls.Add(btnExit);

            drawObj = new SimpleDrawableObject();
        }

        public void Show()
        {
            controlDic.Clear();
            Shown = true;

            // shop
            {
                var list = new List<Control>();
                controlDic.Add("shop", list);

                var frame = new Frame(null, new Vector2(400, 200), new Vector2(440, MMW.Height - 200 - 100));
                list.Add(frame);

                var labelName = new Label(frame, "Name", ControlDrawer.fontSmallB, new Vector2(20, 10));
                list.Add(labelName);

                var labelPrice = new Label(labelName, "Price", ControlDrawer.fontSmallB, new Vector2(260, 0));
                list.Add(labelPrice);

                var labelBag = new Label(labelPrice, "Bag", ControlDrawer.fontSmallB, new Vector2(100, 0));
                list.Add(labelBag);

                var btnBuy = new Button(null, "Buy", new Vector2(MMW.Width - 160, MMW.Height - 124), new Vector2(60, 24), "click");
                btnBuy.Enabled = false;
                btnBuy.Clicked += (s, e) =>
                {
                    var userData = MMW.GetAsset<UserData>();
                    btnBuy.Enabled = userData.Coin >= SelectRect.Focused.Price;
                    if (userData.Coin < SelectRect.Focused.Price) return;

                    var o = MMW.GetAsset<WorldResources>().Objects[SelectRect.Focused.Hash];
                    var info = new WalkerItemInfo()
                    {
                        Name = o.Name,
                        Desc = o.Desc,
                        Hash = o.Hash,
                        Type = o.ItemType,
                        Icon = Util.FromBitmap(o.Icon),
                        MaxStack = o.MaxStack,
                        Consume = o.Consume,
                        Price = o.Price,
                        Sync = o.Sync,
                    };

                    if (userData.AddItem(info, null))
                    {
                        userData.SubCoin(SelectRect.Focused.Price);
                        MMW.BroadcastMessage("log", $"Bought a \"{info.Name}\"");
                    }
                    else
                    {
                        MMW.BroadcastMessage("log", "Items are full");
                    }
                };
                list.Add(btnBuy);

                Control prev = labelName;
                var first = true;
                foreach (var o in MMW.GetAsset<WorldResources>().Objects)
                {
                    if (!o.Value.Purchasable) continue;

                    var name = new Label(prev, o.Value.Name, new Vector2(0, 30));
                    if (first)
                    {
                        name.LocalLocation = new Vector2(0, 40);
                        first = false;
                    }
                    list.Add(name);

                    var price = new Label(name, o.Value.Price.ToString(), new Vector2(260, 0));
                    list.Add(price);

                    var bag = new Label(price, "0", new Vector2(100, 0));
                    list.Add(bag);

                    var rect = new SelectRect(name, new Vector2(-12.0f, 1.0f), new Vector2(420.0f, 24));
                    rect.Hash = o.Key;
                    rect.Price = o.Value.Price;
                    rect.Clicked += (s, e) =>
                    {
                        var userData = MMW.GetAsset<UserData>();
                        btnBuy.Enabled = userData.Coin >= rect.Price;

                        if (!o.Value.Loaded) o.Value.Load();
                        drawObj.Load(o.Value.Mesh, o.Value.Materials);
                    };
                    list.Add(rect);

                    prev = name;
                }
            }

            // skill
            {
                var list = new List<Control>();
                controlDic.Add("skill", list);

                var labelSkill1 = new Label(null, "Skill Point", ControlDrawer.fontSmallB, new Vector2(400, 200));
                list.Add(labelSkill1);

                var labelSkill2 = new Label(null, userData.SkillPoint.ToString(), ControlDrawer.fontSmallB, new Vector2(520, 200));
                labelSkill2.Brush = Brushes.LightGreen;
                list.Add(labelSkill2);

                var frame = new Frame(null, new Vector2(400, 250), new Vector2(MMW.Width - 400 - 100, MMW.Height - 250 - 100));
                list.Add(frame);

                var labelName = new Label(frame, "Name", ControlDrawer.fontSmallB, new Vector2(20, 10));
                list.Add(labelName);


            }

            // players
            {
                var players = MMW.GetAsset<WorldData>().Players;
                var id = MMW.GetAsset<UserData>().SessionID;
                var admin = players.Find(p => p.SessionID == id).IsAdmin;

                var list = new List<Control>();

                var frame = new Frame(null, new Vector2(400, 200), new Vector2(MMW.Width - 400 - 100, MMW.Height - 200 - 100));
                list.Add(frame);

                var labelName = new Label(frame, "Name", ControlDrawer.fontSmallB, new Vector2(20, 10));
                list.Add(labelName);

                var labelRank = new Label(labelName, "Rank", ControlDrawer.fontSmallB, new Vector2(180, 0));
                list.Add(labelRank);

                var labelLiked = new Label(labelRank, "Liked", ControlDrawer.fontSmallB, new Vector2(80, 0));
                list.Add(labelLiked);

                var labelAchivements = new Label(labelLiked, "Achivements", ControlDrawer.fontSmallB, new Vector2(80, 0));
                list.Add(labelAchivements);

                var labelID = new Label(labelAchivements, "ID", ControlDrawer.fontSmallB, new Vector2(150, 0));
                list.Add(labelID);

                var labelBan = new Label(labelID, "Ban", ControlDrawer.fontSmallB, new Vector2(270, 0));
                if (admin) list.Add(labelBan);

                
                Control prev = labelName;
                bool first = true;
                foreach (var p in players)
                {
                    var name = new Label(prev, p.Name, new Vector2(0, 30));
                    if (first)
                    {
                        name.LocalLocation = new Vector2(0, 40);
                        first = false;
                    }
                    list.Add(name);

                    var rank = new Label(name, p.Rank.ToString(), new Vector2(180, 0));
                    list.Add(rank);

                    var like = new Label(rank, p.LikedCount.ToString(), new Vector2(80, 0));
                    list.Add(like);

                    var achivement = new Label(like, p.Achivements.Count.ToString(), new Vector2(80, 0));
                    list.Add(achivement);

                    var userid = new Label(achivement, p.UserID, new Vector2(150, 0));
                    list.Add(userid);

                    if (admin && p.SessionID != id)
                    {
                        var ban = new Button(userid, "Ban", new Vector2(270, 0), new Vector2(48, 24), "click");
                        list.Add(ban);
                    }

                    prev = name;
                }

                controlDic.Add("players", list);
            }

            // graphics
            {
                var list = new List<Control>();
                controlDic.Add("graphics", list);
            }

            // leave
            {
                var list = new List<Control>();
                controlDic.Add("leave", list);

                var label = new Label(null, "Are you sure you want to leave world?", new Vector2(400, 200));
                list.Add(label);

                var btn = new Button(label, "OK", new Vector2(0, 40), "click");
                btn.Clicked += (s, e) =>
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
                list.Add(btn);
            }
        }
        public void Hide()
        {
            Shown = false;

            controlDic.Clear();
        }

        protected override void Update(double deltaTime)
        {
            if (!Shown) return;

            controls.ForEach(c => c.Update(null, deltaTime));
            controlDic[type].ForEach(c => c.Update(null, deltaTime));
        }

        protected override void Draw(double deltaTime, Camera camera)
        {
            if (!Shown) return;

            Drawer.FillRect(Vector2.Zero, new Vector2(MMW.Width, MMW.Height), new Color4(0.0f, 0.0f, 0.0f, 0.1f));

            var g = Drawer.GetGraphics();
            controls.ForEach(c => c.Draw(g, deltaTime));
            controlDic[type].ForEach(c => c.Draw(g, deltaTime));
        }

        protected override void MeshDraw(double deltaTime, Camera camera)
        {
            if (!Shown) return;
            if (type != "shop") return;

            
            //OpenTK.Graphics.OpenGL4.GL.Disable(OpenTK.Graphics.OpenGL4.EnableCap.DepthTest);

            drawObj.Model = MatrixHelper.CreateTransform(new Vector3(-2.0f, -0.5f, 4.0f), Vector3.UnitY * (float)MMW.TotalElapsedTime, Vector3.One);
            drawObj.View = Matrix4.LookAt(Vector3.UnitY * 0.7f, Vector3.UnitZ * 10.0f, Vector3.UnitY);
            drawObj.Projection = camera.Projection;
            drawObj.Draw(deltaTime, camera);

            //if (drawObj.MeshRenderer != null)
            //{
                //Drawer.DrawWireframeMesh(drawObj.MeshRenderer.Mesh, drawObj.Model * drawObj.View * drawObj.Projection, Color4.White);
            //}

            //OpenTK.Graphics.OpenGL4.GL.Enable(OpenTK.Graphics.OpenGL4.EnableCap.DepthTest);
        }

        protected override void OnReceivedMessage(string message, params object[] args)
        {
            if (message == "show menu")
            {
                MMW.MainCamera.GameObject.GetComponent<Blur>().Radius = 40.0f;
                MMW.Window.CursorVisible = true;
                Show();
                Enabled = true;
            }
            else if (message == "close menu")
            {
                MMW.MainCamera.GameObject.GetComponent<Blur>().Radius = 0.0f;
                MMW.Window.CursorVisible = false;
                Hide();
                Enabled = false;
            }
        }
    }

    class SelectRect : Control
    {
        public static SelectRect Focused;
        public string Hash;
        public long Price;

        public SelectRect(Control parent, Vector2 location, Vector2 size)
        {
            Parent = parent;
            LocalLocation = location;
            Size = size;
            Clicked += (s, e) =>
            {
                if (Focused != this) MMW.GetAsset<Sound>("select").Play();
                Focused = this;
            };
        }

        public override void Draw(Graphics g, double deltaTime)
        {
            if (Focused == this)
            {
                var align = GetLocation(Size.X, Size.Y, Alignment);
                Drawer.DrawRect(new Vector2(align.X + WorldLocation.X, align.Y + WorldLocation.Y), Size, Color4.White);
            }
        }
    }
}