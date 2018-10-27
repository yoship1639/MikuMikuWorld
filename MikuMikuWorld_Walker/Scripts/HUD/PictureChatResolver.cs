using MikuMikuWorld.Assets;
using MikuMikuWorld.GameComponents;
using MikuMikuWorld.Networks;
using MikuMikuWorld.Walker;
using MikuMikuWorld.Walker.Network;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Scripts.HUD
{
    class PictureChatResolver : GameComponent
    {
        public override int[] TcpAcceptDataTypes => new int[]
        {
            DataType.PictureChat,
        };

        private PictChatForm form;
        private Server server;

        protected override void OnLoad()
        {
            base.OnLoad();

            Layer = LayerUI;
            server = MMW.GetAsset<Server>();
        }

        protected override void Update(double deltaTime)
        {
            if (!Input.Ctrl && Input.Alt && !Input.Shift && Input.IsKeyPressed(OpenTK.Input.Key.Enter))
            {
                if (form == null)
                {
                    form = new PictChatForm(MMW.GetAsset<UserData>());
                    form.Show(System.Windows.Forms.NativeWindow.FromHandle(MMW.Window.WindowInfo.Handle));
                    form.TopLevel = true;
                    form.Show();

                    form.SendClicked += (s, e) =>
                    {
                        SendPicture(e);
                    };

                    form.FormClosed += (s, e) => form = null;
                }
                else form.TopMost = true;
            }
        }

        public void SendPicture(Color[,] pic)
        {
            var data = new NwPictureChat()
            {
                Data = pic,
                From = server.SessionID,
            };
            server.SendTcpCompress(DataType.PictureChat, data);
        }

        public override void OnTcpReceived(int dataType, byte[] data)
        {
            if (dataType == DataType.PictureChat)
            {
                var chat = Util.DeserializeJsonBinaryCompress<NwPictureChat>(data);

                MMW.BroadcastMessage("picture chat", chat.From, chat.Data);
            }
        }

        protected override void OnReceivedMessage(string message, params object[] args)
        {
            if (message == "enable picture chat")
            {
                Enabled = true;
            }
            else if (message == "disable picture chat")
            {
                Enabled = false;
            }
            else if (message == "show dialog")
            {
                Enabled = false;
            }
            else if (message == "close dialog")
            {
                Enabled = true;
            }
        }
    }
}
