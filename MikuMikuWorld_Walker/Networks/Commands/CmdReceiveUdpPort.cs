using MikuMikuWorld.Network;
using MikuMikuWorld.Walker;
using MikuMikuWorldScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Networks.Commands
{
    class CmdReceiveUdpPort : ICmd
    {
        public int[] ExecutableDataTypes => new int[]
        {
            DataType.ResponseHostRemotePort,
        };

        public bool Execute(Server server, int dataType, byte[] data, bool isTcp)
        {
            if (!isTcp) return false;

            var p = Util.DeserializeJson<NwPortDesc>(data.ToJson());

            if (dataType == DataType.ResponseHostRemotePort)
            {
                server.RemoteUdpPort = p.RemoteUdpPort;
                server.LocalUdpPort = p.LocalUdpPort;
                server.MulticastAddress = IPAddress.Parse(p.MulticastAddress);
                server.RemoteEndPoint = new IPEndPoint(server.RemoteAddress, p.RemoteUdpPort);

                server.StartUdp();
            }

            return true;
        }
    }
}
