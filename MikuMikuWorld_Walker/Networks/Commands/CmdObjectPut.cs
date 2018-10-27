using MikuMikuWorld.Scripts.Player;
using MikuMikuWorld.Walker;
using MikuMikuWorld.Walker.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Networks.Commands
{
    class CmdObjectPut : ICmd
    {
        private PlayerRayResolver script;
        public CmdObjectPut(PlayerRayResolver script)
        {
            this.script = script;
        }

        public int[] ExecutableDataTypes => new int[] { DataType.ResponseObjectPut };

        public bool Execute(Server server, int dataType, byte[] data, bool isTcp)
        {
            if (!isTcp) return false;

            var obj = Util.DeserializeJson<NwWalkerGameObject>(data.ToJson());

            //script.ReceivedObjectPut(obj);

            return true;
        }
    }
}
