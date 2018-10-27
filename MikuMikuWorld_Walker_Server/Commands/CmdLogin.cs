using MikuMikuWorld;
using MikuMikuWorld.Network;
using MikuMikuWorld.Walker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MikuMikuWorld_Walker_Server.Commands
{
    class CmdLogin : Cmd
    {
        public override int[] ExecDataTypes => new int[]
        {
            DataType.Login,
            DataType.ResponseClientLocalPort,
        };

        public override bool OnDataReceived(MainForm form, bool isTcp, Peer peer, int dataType, byte[] data)
        {
            if (!peer.Pending) return false;

            if (dataType == DataType.Login)
            {
                var desc = Util.DeserializeJson<LoginDesc>(data.ToJson());

                if (form.checkBox_worldPass.Checked && desc.Password != form.textBox_worldPass.Text)
                {
                    Thread.Sleep(1000);
                    peer.SendTcp(DataType.LoginResult, BitConverter.GetBytes(-1));
                    return true;
                }

                if (form.Server.Blacklist.IsIgnoreID(desc.UserID))
                {
                    form.Server.pendingPeers.Remove(peer);
                    form.Server.EventPeerRejected(form, peer.EndPoint);
                    return true;
                }

                peer.Name = desc.UserName;
                //peer.UniqueID = desc.UserID;
                //peer.NameColor = new OpenTK.Graphics.Color4(desc.UserColor.R, desc.UserColor.G, desc.UserColor.B, 1.0f);
                //peer.Icon = Util.FromBitmapString(desc.UserIcon);
                peer.Pending = false;

                peer.tcp.ReceiveTimeout = Timeout.Infinite;
                form.Server.peers.Add(peer);
                form.Server.pendingPeers.Remove(peer);
                form.Server.EventPeerAccepted(this, new PeerEventArgs()
                {
                    peer = peer,
                    isTcp = true,
                });

                peer.SendTcp(DataType.LoginResult, BitConverter.GetBytes(peer.SessionID));
            }
            
            return true;
            
        }

        public override bool OnPeerAccepted(MainForm form, Peer peer)
        {
            var rand = new Random();
            var port = rand.Next(49152, 65535);
            while (true)
            {
                try
                {
                    var test = new UdpClient(port, AddressFamily.InterNetwork);
                    test.Close();
                    break;
                }
                catch
                {
                    port = rand.Next(49152, 65535);
                }
            }

            peer.StartUdp(port);

            var p = new NwPortDesc()
            {
                RemoteUdpPort = port,
                LocalUdpPort = form.Server.UdpStartPort,
                MulticastAddress = form.Server.MulticastAddress.ToString(),
            };

            peer.SendTcp(DataType.ResponseHostRemotePort, Util.SerializeJson(p));

            return true;
        }
    }
}
