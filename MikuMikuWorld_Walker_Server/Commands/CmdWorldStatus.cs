using MikuMikuWorld;
using MikuMikuWorld.Walker;
using MikuMikuWorld.Walker.Network;
using MikuMikuWorldScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld_Walker_Server.Commands
{
    class CmdWorldStatus : Cmd
    {
        public override int[] ExecDataTypes => new int[]
        {
            DataType.RequestWorldStatus,
        };

        public override bool OnDataReceived(MainForm form, bool isTcp, Peer peer, int dataType, byte[] data)
        {
            if (dataType != DataType.RequestWorldStatus) return false;
            List<WalkerPlayer> players = new List<WalkerPlayer>();

            foreach (Peer p in form.listBox_player.Items)
            {
                if (p.Player != null) players.Add(p.Player);
            } 

            var objs = new List<NwWalkerGameObject>();
            foreach (var o in form.GameObjectHashmap.Values) objs.Add(o);

            var status = new NwWorldStatus()
            {
                Players = players.ToArray(),
                WorldObjects = objs.ToArray(),
            };

            var buf = Util.SerializeJsonBinary(status, false);

            peer.SendTcp(DataType.ResponseWorldStatus, buf);

            return true;
        }
    }
}
