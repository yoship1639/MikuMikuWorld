using MikuMikuWorld.Networks;
using MikuMikuWorld.Walker;
using MikuMikuWorldScript;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MikuMikuWorld.Controls
{
    class SelectWorldFrame : Control
    {
        List<SelectWorldPanel> panels = new List<SelectWorldPanel>();
        private Control dummy;
        private float targetHeight;

        public WorldInfo WorldInfo { get; set; }

        public SelectWorldFrame(Control parent, float height, Vector2 pos)
        {
            Parent = parent;
            Size = new Vector2(760.0f, height);
            LocalLocation = pos;

            dummy = new Control();
            dummy.Parent = this;
        }

        public void AddWorld(WorldInfo info)
        {
            var panel = new SelectWorldPanel(info);
            panel.Parent = dummy;
            panel.Size = new Vector2(360, 170);
            panel.LocalLocation = new Vector2(15.0f + (panels.Count % 2) * 370.0f, 15.0f + (panels.Count / 2) * 180.0f);
            panel.Clicked += (s, e) =>
            {
                WorldInfo = info;
                ServerClicked(this, info);
            };
            panel.DoubleClicked += (s, e) =>
            {
                if (panel.Connected)
                {
                    WorldInfo = info;
                    ServerSelected(this, info);
                }
            };

            

            /*
            task = Task.Factory.StartNew(() =>
            {
                while (!taskCancel)
                {
                    var data = NetworkUtil.QueryServerDescUdp(desc.HostName, desc.Port);
                    panel.ReceivedServerDesc(data);

                    Thread.Sleep(3000);
                }
            });*/

            panels.Add(panel);
        }

        public void RemoveWorld(WorldInfo info)
        {
            var pn = panels.Find(p => p.Info == info);
            panels.Remove(pn);
            pn.Destroy();
        }

        public override void Update(Graphics g, double deltaTime)
        {
            base.Update(g, deltaTime);

            if (IsMouseOn && Input.MouseWheel != 0)
            {
                targetHeight -= Input.MouseWheel * 100.0f;
                targetHeight = MMWMath.Clamp(targetHeight, -((panels.Count - 1) / 2) * 180.0f, 0.0f);
            }

            dummy.LocalLocation = new Vector2(dummy.LocalLocation.X, MMWMath.Lerp(dummy.LocalLocation.Y, targetHeight, (float)deltaTime * 6.0f));

            foreach (var p in panels) p.Update(g, deltaTime);
        }

        public override void Draw(Graphics g, double deltaTime)
        {
            base.Draw(g, deltaTime);

            ControlDrawer.DrawFrame(WorldLocation.X, WorldLocation.Y, Size.X, Size.Y);

            Drawer.SetClip(g, WorldLocation.X, WorldLocation.Y + 15.0f, Size.X, Size.Y - 30.0f);
            foreach (var p in panels) p.Draw(g, deltaTime);
            Drawer.ResetClip(g);
        }

        public event EventHandler<WorldInfo> ServerClicked = delegate { };
        public event EventHandler<WorldInfo> ServerSelected = delegate { };

        public override void Unload()
        {
            panels.ForEach(p => p.Destroy());
        }
    }
}
