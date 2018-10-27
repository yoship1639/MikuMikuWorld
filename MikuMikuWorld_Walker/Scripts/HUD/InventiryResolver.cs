using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikuMikuWorld.Controls;
using MikuMikuWorld.GameComponents;
using MikuMikuWorld.GameComponents.ImageEffects;
using MikuMikuWorld.Walker;
using OpenTK;
using OpenTK.Graphics;

namespace MikuMikuWorld.Scripts.HUD
{
    class InventiryResolver : DrawableGameComponent
    {
        public bool Shown { get; set; } = false;

        private UserData userData;
        private List<Control> controls = new List<Control>();
        private HoldItem hold;

        protected override void OnLoad()
        {
            Layer = LayerUI + 2;

            userData = MMW.GetAsset<UserData>();

            var label = new Label(
                null,
                "BAG",
                new Font("Yu Gothic UI", 32.0f),
                new Vector2((MMW.Width - Drawer.MeasureString("BAG", new Font("Yu Gothic UI", 32.0f)).X) / 2.0f, 100.0f));
            controls.Add(label);

            var frame = new Frame(null, new Vector2((MMW.Width - 640.0f) * 0.5f, 200.0f), new Vector2(640.0f, 440.0f));
            controls.Add(frame);

            for (var y = 0; y < 5; y++)
            {
                for (var x = 0; x < 10; x++)
                {
                    var idx = y * 10 + x;
                    var f = new ItemFrame(frame, new Vector2(60.0f * x + 26.0f, 60.0f * y + 26.0f), idx);
                    controls.Add(f);

                    f.Clicked += (s, e) =>
                    {
                        var i = f.ItemIndex;
                        if (i >= userData.MaxItemCount) return;
                        if (hold.Item == null)
                        {
                            hold.Item = userData.Items[i];
                            userData.Items[i] = null;
                            
                        }
                        else
                        {
                            if (userData.Items[i] == null)
                            {
                                userData.Items[i] = hold.Item;
                                hold.Item = null;
                            }
                            else if (userData.Items[i].Info.Hash == hold.Item.Info.Hash)
                            {
                                var max = userData.Items[i].Info.MaxStack;
                                while (userData.Items[i].Number < max && hold.Item.Number > 0)
                                {
                                    userData.Items[i].Number++;
                                    hold.Item.Number--;
                                }
                                if (hold.Item.Number <= 0) hold.Item = null;
                            }
                            else
                            {
                                var tmp = hold.Item;
                                hold.Item = userData.Items[i];
                                userData.Items[i] = tmp;
                            }
                        }
                    };

                    f.RightClicked += (s, e) =>
                    {
                        var i = f.ItemIndex;
                        if (i >= userData.MaxItemCount) return;

                        if (hold.Item != null)
                        {
                            if (userData.Items[i] == null)
                            {
                                userData.Items[i] = new WalkerItem()
                                {
                                    Info = hold.Item.Info.Clone(),
                                    Number = 1,
                                    Status = hold.Item.Status,
                                };

                                hold.Item.Number--;
                                if (hold.Item.Number <= 0) hold.Item = null;
                            }
                            else if (userData.Items[i].Info.Hash == hold.Item.Info.Hash)
                            {
                                var max = userData.Items[i].Info.MaxStack;
                                if (userData.Items[i].Number < max)
                                {
                                    userData.Items[i].Number++;
                                    hold.Item.Number--;
                                    if (hold.Item.Number <= 0) hold.Item = null;
                                }
                            }
                        }
                    };
                }
            }

            for (var x = 0; x < userData.MaxHotbatItemCount; x++)
            {
                var f = new HotbarItemFrame(frame, new Vector2(60.0f * x + 26.0f, frame.Size.Y - 72.0f), x);
                controls.Add(f);

                f.Clicked += (s, e) =>
                {
                    var i = f.ItemIndex;
                    if (hold.Item == null)
                    {
                        hold.Item = userData.HotbarItems[i];
                        userData.HotbarItems[i] = null;
                    }
                    else
                    {
                        if (userData.HotbarItems[i] == null)
                        {
                            userData.HotbarItems[i] = hold.Item;
                            hold.Item = null;
                        }
                        else if (userData.HotbarItems[i].Info.Hash == hold.Item.Info.Hash)
                        {
                            var max = userData.HotbarItems[i].Info.MaxStack;
                            while (userData.HotbarItems[i].Number < max && hold.Item.Number > 0)
                            {
                                userData.HotbarItems[i].Number++;
                                hold.Item.Number--;
                            }
                            if (hold.Item.Number <= 0) hold.Item = null;
                        }
                        else
                        {
                            var tmp = hold.Item;
                            hold.Item = userData.HotbarItems[i];
                            userData.HotbarItems[i] = tmp;
                        }
                    }
                };

                f.RightClicked += (s, e) =>
                {
                    var i = f.ItemIndex;
                    if (i >= userData.MaxItemCount) return;

                    if (hold.Item != null)
                    {
                        if (userData.HotbarItems[i] == null)
                        {
                            userData.HotbarItems[i] = new WalkerItem()
                            {
                                Info = hold.Item.Info.Clone(),
                                Number = 1,
                                Status = hold.Item.Status,
                            };

                            hold.Item.Number--;
                            if (hold.Item.Number <= 0) hold.Item = null;
                        }
                        else if (userData.HotbarItems[i].Info.Hash == hold.Item.Info.Hash)
                        {
                            var max = userData.HotbarItems[i].Info.MaxStack;
                            if (userData.HotbarItems[i].Number < max)
                            {
                                userData.HotbarItems[i].Number++;
                                hold.Item.Number--;
                                if (hold.Item.Number <= 0) hold.Item = null;
                            }
                        }
                    }
                };
            }

            hold = new HoldItem();
            controls.Add(hold);
        }

        protected override void Update(double deltaTime)
        {
            if (!Shown) return;
            controls.ForEach(c => c.Update(null, deltaTime));
        }

        protected override void Draw(double deltaTime, Camera camera)
        {
            if (!Shown) return;
            var g = Drawer.GetGraphics();

            controls.ForEach(c => c.Draw(g, deltaTime));
        }

        protected override void OnReceivedMessage(string message, params object[] args)
        {
            if (message == "show inventory")
            {
                MMW.MainCamera.GameObject.GetComponent<Blur>().Radius = 40.0f;
                MMW.Window.CursorVisible = true;
                Shown = true;
                Enabled = true;
            }
            else if (message == "close inventory")
            {
                MMW.MainCamera.GameObject.GetComponent<Blur>().Radius = 0.0f;
                MMW.Window.CursorVisible = false;
                Shown = false;
                Enabled = false;
            }
        }
    }

    class ItemFrame : Control
    {
        public int ItemIndex { get; set; }
        private UserData userData;
        private WorldResources resources;

        public ItemFrame(Control parent, Vector2 pos, int itemIndex)
        {
            Size = new Vector2(48.0f, 48.0f);
            Parent = parent;
            LocalLocation = pos;
            ItemIndex = itemIndex;
            userData = MMW.GetAsset<UserData>();
            resources = MMW.GetAsset<WorldResources>();
        }

        public override void Draw(Graphics g, double deltaTime)
        {
            var l = GetLocation(Size.X, Size.Y, Alignment);
            var x = l.X + WorldLocation.X;
            var y = l.Y + WorldLocation.Y;

            Color4 color = new Color4(0.5f, 0.5f, 0.5f, 0.65f);
            if (userData.Items[ItemIndex] != null && !resources.Objects.ContainsKey(userData.Items[ItemIndex].Info.Hash))
            {
                if (IsMouseOn) color = new Color4(0.75f, 0.5f, 0.5f, 0.65f);
                else color = new Color4(0.6f, 0.4f, 0.4f, 0.65f);
            } 
            else if (ItemIndex >= userData.MaxItemCount) color = new Color4(0.5f, 0.5f, 0.75f, 0.65f);
            else if (IsMouseOn) color = new Color4(0.75f, 0.75f, 0.75f, 0.65f);

            Drawer.FillRect(new Vector2(x, y), Size, color);
            Drawer.DrawRect(new Vector2(x, y), Size, Color4.Gray);

            if (userData.Items[ItemIndex] != null)
            {
                var item = userData.Items[ItemIndex];

                Drawer.ScriptDrawer.Texture = userData.Items[ItemIndex].Info.bitmap;
                Drawer.ScriptDrawer.Color = Color4.White;
                Drawer.ScriptDrawer.DrawTexture(new RectangleF(x + 24.0f, y + 24.0f, Size.X, Size.Y));

                if (IsMouseOn)
                {
                    var ns = g.MeasureString(item.Info.Name, ControlDrawer.fontSmallB);
                    ControlDrawer.DrawFrame(x + 48.0f, y - ns.Height - 8.0f, ns.Width + 8.0f, ns.Height + 8.0f);
                    g.DrawString(item.Info.Name, ControlDrawer.fontSmallB, Brushes.White, x + 48.0f + 4.0f, y - ns.Height - 8.0f + 4.0f);
                }

                if (item.Number > 1)
                {
                    var s = g.MeasureString(item.Number.ToString(), ControlDrawer.fontSmallB);
                    g.DrawString(item.Number.ToString(), ControlDrawer.fontSmallB, Brushes.White, x + 48.0f - s.Width, y + 48.0f - s.Height);
                }
            }
        }
    }

    class HotbarItemFrame : Control
    {
        private static Font font = new Font("Yu Gothic UI", 8.0f);
        public int ItemIndex { get; set; }
        private UserData userData;
        private WorldResources resources;

        public HotbarItemFrame(Control parent, Vector2 pos, int itemIndex)
        {
            Size = new Vector2(48.0f, 48.0f);
            Parent = parent;
            LocalLocation = pos;
            ItemIndex = itemIndex;
            userData = MMW.GetAsset<UserData>();
            resources = MMW.GetAsset<WorldResources>();
        }

        public override void Draw(Graphics g, double deltaTime)
        {
            var l = GetLocation(Size.X, Size.Y, Alignment);
            var x = l.X + WorldLocation.X;
            var y = l.Y + WorldLocation.Y;

            Color4 color = new Color4(0.5f, 0.5f, 0.5f, 0.65f);
            if (userData.HotbarItems[ItemIndex] != null && !resources.Objects.ContainsKey(userData.HotbarItems[ItemIndex].Info.Hash))
            {
                if (IsMouseOn) color = new Color4(0.75f, 0.5f, 0.5f, 0.65f);
                else color = new Color4(0.6f, 0.4f, 0.4f, 0.65f);
            }
            else if (IsMouseOn) color = new Color4(0.75f, 0.75f, 0.75f, 0.65f);

            Drawer.FillRect(new Vector2(x, y), Size, color);
            Drawer.DrawRect(new Vector2(x, y), Size, Color4.Gray);

            if (userData.HotbarItems[ItemIndex] != null)
            {
                var item = userData.HotbarItems[ItemIndex];

                Drawer.ScriptDrawer.Texture = userData.HotbarItems[ItemIndex].Info.bitmap;
                Drawer.ScriptDrawer.Color = Color4.White;
                Drawer.ScriptDrawer.DrawTexture(new RectangleF(x + 24.0f, y + 24.0f, Size.X, Size.Y));

                if (IsMouseOn)
                {
                    var ns = g.MeasureString(item.Info.Name, ControlDrawer.fontSmallB);
                    ControlDrawer.DrawFrame(x + 48.0f, y - ns.Height - 8.0f, ns.Width + 8.0f, ns.Height + 8.0f);
                    g.DrawString(item.Info.Name, ControlDrawer.fontSmallB, Brushes.White, x + 48.0f + 4.0f, y - ns.Height - 8.0f + 4.0f);
                }

                if (item.Number > 1)
                {
                    var s = g.MeasureString(item.Number.ToString(), ControlDrawer.fontSmallB);
                    g.DrawString(item.Number.ToString(), ControlDrawer.fontSmallB, Brushes.White, x + 48.0f - s.Width, y + 48.0f - s.Height);
                }
            }

            g.DrawString((ItemIndex + 1).ToString(), font, Brushes.White, x, y);
        }
    }

    class HoldItem : Control
    {
        public WalkerItem Item { get; set; }

        public HoldItem()
        {
            Size = new Vector2(48.0f, 48.0f);
        }

        public override void Update(Graphics g, double deltaTime)
        {
            LocalLocation = Input.MousePosition - new Vector2(24.0f, 24.0f);
        }

        public override void Draw(Graphics g, double deltaTime)
        {
            if (Item == null) return;

            Drawer.ScriptDrawer.Texture = Item.Info.bitmap;
            Drawer.ScriptDrawer.Color = Color4.White;
            Drawer.ScriptDrawer.DrawTexture(new RectangleF(WorldLocation.X + 24.0f, WorldLocation.Y + 24.0f, Size.X, Size.Y));

            if (Item.Number > 1)
            {
                var s = g.MeasureString(Item.Number.ToString(), ControlDrawer.fontSmallB);
                g.DrawString(Item.Number.ToString(), ControlDrawer.fontSmallB, Brushes.White, WorldLocation.X + 48.0f - s.Width, WorldLocation.Y + 48.0f - s.Height);
            }
        }
    }
}
