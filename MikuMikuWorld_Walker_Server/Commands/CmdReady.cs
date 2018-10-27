using MikuMikuWorld;
using MikuMikuWorld.Walker;
using MikuMikuWorld.Walker.Network;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld_Walker_Server.Commands
{
    class CmdReady : Cmd
    {
        public override int[] ExecDataTypes => new int[] { DataType.ResponseReady };

        public override bool OnDataReceived(MainForm form, bool isTcp, Peer peer, int dataType, byte[] data)
        {
            if (!isTcp || peer.Pending) return false;

            var p = Util.DeserializeJson<WalkerPlayer>(data.ToJson());

            peer.Player = p;
            p.SessionID = peer.SessionID;

            form.Server.SendTcp(DataType.ResponsePlayerJoin, Util.SerializeJsonBinary(p));

            return true;
        }
    }
}
