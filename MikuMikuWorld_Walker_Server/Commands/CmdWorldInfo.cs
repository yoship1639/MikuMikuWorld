using MikuMikuWorld;
using MikuMikuWorld.Walker;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld_Walker_Server.Commands
{
    class CmdWorldInfo : Cmd
    {
        public override int[] ExecDataTypes => new int[]
        {
            DataType.RequestWorldInfo,
        };

        public override bool OnDataReceived(MainForm form, bool isTcp, Peer peer, int dataType, byte[] data)
        {
            if (!isTcp) return true;

            var type = Util.DeserializeJsonBinary<int>(data);

            var info = form.CreateWorldInfo();
            if (type == 1) info.WorldImage = form.worldImageString;

            peer.SendTcp(DataType.ResponseWorldInfo, Util.SerializeJson(info));
            return true;
        }
    }
}
