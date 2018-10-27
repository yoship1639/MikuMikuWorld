using MikuMikuWorld.Network;
using MikuMikuWorld.Walker;
using MikuMikuWorldScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Networks.Commands
{
    class CmdLogin : ICmd
    {
        public int[] ExecutableDataTypes => new int[]
        {
            DataType.LoginResult,
        };

        public bool Execute(Server server, int dataType, byte[] data, bool isTcp)
        {
            if (!isTcp) return false;

            Buffer.Read(data, br =>
            {
                var res = br.ReadInt32();
                if (res >= 0) server.SessionID = res;

                MMW.BroadcastMessage("login result", res >= 0);
                //server.EventLoginResultReceived(res >= 0);
            });

            return true;
        }
    }
}
