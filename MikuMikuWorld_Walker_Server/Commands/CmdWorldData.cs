using MikuMikuWorld;
using MikuMikuWorld.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld_Walker_Server.Commands
{
    class CmdWorldData : Cmd
    {
        public override int[] ExecDataTypes => new int[]
        {
            MikuMikuWorld.Walker.DataType.RequestWorld,
            MikuMikuWorld.Walker.DataType.RequestCharacter,
            MikuMikuWorld.Walker.DataType.RequestObject,
            MikuMikuWorld.Walker.DataType.RequestGameObjectScript,
        };

        private Dictionary<int, int> reqMap = new Dictionary<int, int>()
        {
            { MikuMikuWorld.Walker.DataType.RequestWorld, MikuMikuWorld.Walker.DataType.ResponseWorld },
            { MikuMikuWorld.Walker.DataType.RequestCharacter, MikuMikuWorld.Walker.DataType.ResponseCharacter },
            { MikuMikuWorld.Walker.DataType.RequestObject, MikuMikuWorld.Walker.DataType.ResponseObject },
            { MikuMikuWorld.Walker.DataType.RequestGameObjectScript, MikuMikuWorld.Walker.DataType.ResponseGameObjectScript },
        };

        public override bool OnDataReceived(MainForm form, bool isTcp, Peer peer, int dataType, byte[] data)
        {
            if (peer.Pending)
            {
                peer.SendTcp(reqMap[dataType], "{}");
                return true;
            }

            byte[] buf = null;
            var req = Util.DeserializeJsonBinary<NwRequest>(data);
            if (dataType == MikuMikuWorld.Walker.DataType.RequestWorld)
            {
                form.WorldHashmap.TryGetValue(req.Hash, out buf);
            }
            if (dataType == MikuMikuWorld.Walker.DataType.RequestCharacter)
            {
                form.CharHashmap.TryGetValue(req.Hash, out buf);
            }
            if (dataType == MikuMikuWorld.Walker.DataType.RequestObject)
            {
                form.ObjHashmap.TryGetValue(req.Hash, out buf);
            }
            if (dataType == MikuMikuWorld.Walker.DataType.RequestGameObjectScript)
            {
                form.GameObjectScriptHashmap.TryGetValue(req.Hash, out buf);
            }

            if (buf != null) peer.SendTcp(reqMap[dataType], buf);
            else peer.SendTcp(reqMap[dataType], "{}");

            return true;
        }
    }
}
