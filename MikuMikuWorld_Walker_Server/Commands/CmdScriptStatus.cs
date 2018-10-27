using MikuMikuWorld.Walker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld_Walker_Server.Commands
{
    class CmdScriptStatus : Cmd
    {
        public override int[] ExecDataTypes => new int[]
        {
            DataType.RequestScriptUpdate,
        };

        public override bool OnDataReceived(MainForm form, bool isTcp, Peer peer, int dataType, byte[] data)
        {
            if (!isTcp || peer.Pending) return true;

            Buffer.Read(data, br =>
            {
                var objHash = br.ReadString();
                var scrHash = br.ReadString();
                var length = br.ReadInt32();
                var buf = br.ReadBytes(length);
                form.UpdateWorldObjectStatus(objHash, scrHash, buf);
            });

            form.Server.SendTcp(DataType.ResponseScriptUpdate, data);

            return true;
        }
    }
}
