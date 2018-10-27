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
    class CmdObjectPut : Cmd
    {
        public override int[] ExecDataTypes => new int[] { DataType.RequestObjectPut };

        public override bool OnDataReceived(MainForm form, bool isTcp, Peer peer, int dataType, byte[] data)
        {
            if (!isTcp || peer.Pending) return false;

            var obj = Util.DeserializeJsonBinary<NwWalkerGameObject>(data);

            form.AddWorldObject(obj);

            form.Server.SendTcp(DataType.ResponseObjectPut, data);

            return true;
        }
    }
}
