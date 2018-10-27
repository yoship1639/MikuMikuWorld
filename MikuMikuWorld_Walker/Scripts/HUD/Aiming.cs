using MikuMikuWorld.GameComponents;
using MikuMikuWorld.Scripts;
using MikuMikuWorld.Scripts.Player;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Scripts.HUD
{
    class Aiming : DrawableGameComponent
    {
        public bool Shown { get; set; } = true;

        private GameObject focusedObj;

        protected override void OnLoad()
        {
            base.OnLoad();

            Layer = LayerUI;
        }

        protected override void Draw(double deltaTime, Camera camera)
        {
            if (Shown)
            {
                Color4 color = Color4.White;
                if (focusedObj != null)
                {
                    if (focusedObj.Tags.Contains("world")) color = Color4.Blue;
                    else if (focusedObj.Tags.Contains("world object")) color = Color4.Red;
                    else if (focusedObj.Tags.Contains("character")) color = Color4.Green;
                }
                Drawer.FillRect(new Vector2(MMW.Width / 2.0f - 5.0f, MMW.Height / 2.0f - 1.0f), new Vector2(10.5f, 3.0f), color);
                Drawer.FillRect(new Vector2(MMW.Width / 2.0f - 1.0f, MMW.Height / 2.0f - 5.0f), new Vector2(2.5f, 11.0f), color);
            }
        }

        protected override void OnReceivedMessage(string message, params object[] args)
        {
            if (message == "aim show") Shown = true;
            else if (message == "aim hide") Shown = false;
            else if (message == "show dialog") Shown = false;
            else if (message == "close dialog") Shown = true;
            else if (message == "focus enter")
            {
                focusedObj = (GameObject)args[0];
                //data = (PlayerRayData)args[1];
            }
            else if (message == "focus leave")
            {
                focusedObj = null;
                //data = (PlayerRayData)args[1];
            }
        }

        public override GameComponent Clone()
        {
            throw new NotImplementedException();
        }
    }
}
