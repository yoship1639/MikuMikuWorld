using MikuMikuWorld;
using MikuMikuWorld.Walker;
using MikuMikuWorld.Walker.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld_Walker_Server.Commands
{
    class CmdPictureChat : Cmd
    {
        public override int[] ExecDataTypes => new int[]
        {
            DataType.PictureChat,
        };

        public override bool OnDataReceived(MainForm form, bool isTcp, Peer peer, int dataType, byte[] data)
        {
            if (peer.Pending) return true;

            var chat = Util.DeserializeJsonBinaryCompress<NwPictureChat>(data);

            form.Server.SendTcp(DataType.PictureChat, data);

            return true;
        }
    }
}
