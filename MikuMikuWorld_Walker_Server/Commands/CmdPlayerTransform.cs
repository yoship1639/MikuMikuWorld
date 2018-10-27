using MikuMikuWorld;
using MikuMikuWorld.Walker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld_Walker_Server.Commands
{
    class CmdPlayerTransform : Cmd
    {
        public override int[] ExecDataTypes => new int[]
        {
            DataType.ResponsePlayerTransform,
        };

        public override bool OnDataReceived(MainForm form, bool isTcp, Peer peer, int dataType, byte[] data)
        {
            if (peer.Pending || isTcp) return false;

            Buffer.Read(data, br =>
            {
                peer.Player.Position = new Vector3f(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                peer.Player.Rotation = new Vector3f(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            });

            return true;
        }
    }
}