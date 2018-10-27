using MikuMikuWorld.Scripts.Player;
using MikuMikuWorld.Walker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Networks.Commands
{
    class CmdObjectPicked : ICmd
    {
        private PlayerRayResolver script;
        public CmdObjectPicked(PlayerRayResolver script)
        {
            this.script = script;
        }

        public int[] ExecutableDataTypes => new int[] { DataType.ResponseObjectPicked };

        public bool Execute(Server server, int dataType, byte[] data, bool isTcp)
        {
            Buffer.Read(data, br =>
            {
                var player = br.ReadInt32();
                var hash = br.ReadString();
                //script.ReceivedObjectPicked((Player)Array.Find(server.Players, p => p.SessionID == player), hash);
            });

            return true;
        }
    }
}
