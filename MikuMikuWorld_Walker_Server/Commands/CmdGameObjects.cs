using MikuMikuWorld;
using MikuMikuWorld.Network;
using MikuMikuWorld.Walker.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld_Walker_Server.Commands
{
    class CmdGameObjects : Cmd
    {
        public override int[] ExecDataTypes => new int[]
        {
            MikuMikuWorld.Walker.DataType.RequestGameObjects,
        };

        private Dictionary<int, int> reqMap = new Dictionary<int, int>()
        {
            { MikuMikuWorld.Walker.DataType.RequestGameObjects, MikuMikuWorld.Walker.DataType.ResponseGameObjects },
        };

        public override bool OnDataReceived(MainForm form, bool isTcp, Peer peer, int dataType, byte[] data)
        {
            if (peer.Pending)
            {
                peer.SendTcp(reqMap[dataType], "{}");
                return true;
            }

            var objects = new NwGameObjects();
            objects.GameObjects = form.GameObjectHashmap.Values.ToArray();

            peer.SendTcp(reqMap[dataType], Util.SerializeJson(objects));

            return true;
        }
    }
}
