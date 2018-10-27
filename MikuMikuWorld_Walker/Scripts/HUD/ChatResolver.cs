using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MikuMikuWorld.GameComponents;
using MikuMikuWorld.Networks;
using MikuMikuWorld.Networks.Commands;
using MikuMikuWorld.Walker;
using MikuMikuWorld.Walker.Network;
using MikuMikuWorldScript;

namespace MikuMikuWorld.Scripts.HUD
{
    class ChatResolver : GameComponent
    {
        private TextInputForm form;
        Server server;

        protected override void OnLoad()
        {
            base.OnLoad();

            Layer = LayerUI;
            server = MMW.GetAsset<Server>();
        }

        protected override void Update(double deltaTime)
        {
            if (form == null && !Input.Ctrl && !Input.Alt && !Input.Shift && Input.IsKeyPressed(OpenTK.Input.Key.Enter))
            {
                if (form == null)
                {
                    form = new TextInputForm("Chat", 256, true);
                    form.Show(System.Windows.Forms.NativeWindow.FromHandle(MMW.Window.WindowInfo.Handle));
                    form.Location = new System.Drawing.Point(form.Location.X, (int)MMW.Height - 200);
                    form.TopLevel = true;

                    form.FormClosed += (se, e) =>
                    {
                        form.ImeMode = System.Windows.Forms.ImeMode.Off;
                        SendChat(form.InputText);
                        Thread.Sleep(200);
                        form = null;
                    };
                }
                else form.TopMost = true;
            }
        }

        private void SendChat(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            var chat = new NwChat()
            {
                Text = text,
                From = server.SessionID,
            };
            server.SendTcp(DataType.Chat, Util.SerializeJson(chat));
        }

        protected override void OnReceivedMessage(string message, params object[] args)
        {
            if (message == "enable chat")
            {
                Enabled = true;
            }
            else if (message == "disable chat")
            {
                Enabled = false;
            }
        }
    }
}
