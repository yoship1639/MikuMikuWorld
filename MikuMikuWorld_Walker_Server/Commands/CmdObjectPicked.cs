using MikuMikuWorld.Walker;
using MikuMikuWorld.Walker.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld_Walker_Server.Commands
{
    class CmdObjectPicked : Cmd
    {
        public override int[] ExecDataTypes => new int[] { DataType.RequestObjectPicked };

        public override bool OnDataReceived(MainForm form, bool isTcp, Peer peer, int dataType, byte[] data)
        {
            if (!isTcp || peer.Pending) return false;

            string hash = null;
            Buffer.Read(data, br =>
            {
                hash = br.ReadString();
            });

            form.RemoveWorldObject(hash);

           var buf = Buffer.Write(br =>
            {
                br.Write(peer.SessionID);
                br.Write(hash);
            });

            form.Server.SendTcp(DataType.ResponseObjectPicked, buf);

            return true;
        }
    }
}
