using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MikuMikuWorld_Walker_Server
{
    public class NetworkServer
    {
        public static readonly byte[] Magic = Encoding.UTF8.GetBytes("MMWN");

        public IPAddress Address { get; set; }
        public int TcpPort { get; set; }
        public int UdpStartPort { get; set; }
        public int MaxConnection { get; set; } = 20;
        public IPAddress MulticastAddress { get; set; } = IPAddress.Parse("239.0.0.39");

        internal List<Peer> peers = new List<Peer>();
        public Peer[] Peers => peers.ToArray();
        private TcpListener listener;

        public UdpClient multicastUdp;
        private IPEndPoint multicastEP;

        //private Task tcpTask;
        private bool listenCancel = false;

        internal List<Peer> pendingPeers = new List<Peer>();

        public Blacklist Blacklist { get; internal set; }
        public int LocalUdpPort { get; internal set; }

        public NetworkServer(IPAddress addr, int tcpPort, int udpStartPort)
        {
            Address = addr;
            TcpPort = tcpPort;
            UdpStartPort = udpStartPort;
            multicastEP = new IPEndPoint(MulticastAddress, UdpStartPort);
            
        }

        public void Start()
        {
            listenCancel = false;
            Task.Factory.StartNew(() =>
            {
                while (!listenCancel)
                {
                    while (peers.Count >= MaxConnection)
                    {
                        Thread.Sleep(500);
                    }
                    
                    listener = new TcpListener(Address, TcpPort);
                    listener.Start();

                    var client = listener.AcceptTcpClient();
                        
                    listener.Stop();
                    if (listenCancel) break;

                    // ブラックリストに存在
                    var ip = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                    if (Blacklist.IsIgnoreIP(ip))
                    {
                        PeerRejected(this, (IPEndPoint)client.Client.RemoteEndPoint);
                        client.Close();
                        continue;
                    }

                    client.ReceiveTimeout = 10 * 60 * 1000;
                    var peer = new Peer(client);
                    peer.DataReceived += Peer_DataReceived;
                    peer.Disconnected += Peer_Disconnected;
                        
                    pendingPeers.Add(peer);

                    PeerConnected(this, new PeerEventArgs()
                    {
                        peer = peer,
                    });
                }
            });

            while (true)
            {
                try
                {
                    LocalUdpPort = new Random().Next(49152, 65535);
                    multicastUdp = new UdpClient(LocalUdpPort, AddressFamily.InterNetwork);
                    break;
                }
                catch { }
            }
            multicastUdp.JoinMulticastGroup(MulticastAddress, 50);
        }
        public void Stop()
        {
            try
            {
                listenCancel = true;
                listener.Stop();
            }
            catch { }
            

            foreach (var p in peers.ToList())
            {
                try
                {
                    p.Close();
                    p.DataReceived -= Peer_DataReceived;
                    p.Disconnected -= Peer_Disconnected;
                }
                catch { }
            }
            foreach (var p in pendingPeers.ToList())
            {
                try
                {
                    p.Close();
                    p.DataReceived -= Peer_DataReceived;
                    p.Disconnected -= Peer_Disconnected;
                }
                catch { }
            }
            peers.Clear();
            pendingPeers.Clear();

            try
            {
                multicastUdp.Close();
            }
            catch { }
            multicastUdp = null;
        }

        public void SendTcp(int dataType, byte[] buf)
        {
            Parallel.ForEach(peers, p =>
            {
                p.SendTcp(dataType, buf);
            });
        }
        public void SendUdp(byte[] buf)
        {
            var data = Magic.Concat(buf).ToArray();
            multicastUdp.Send(data, data.Length, multicastEP);
        }

        public event EventHandler<IPEndPoint> PeerRejected = delegate { };
        public event EventHandler<PeerEventArgs> PeerConnected = delegate { };
        public event EventHandler<PeerEventArgs> PeerAccepted = delegate { };
        public event EventHandler<PeerEventArgs> PeerDataReceived = delegate { };
        public event EventHandler<PeerEventArgs> PeerDisconnected = delegate { };

        internal void EventPeerRejected(object sender, IPEndPoint ep) { PeerRejected(sender, ep); }
        internal void EventPeerConnected(object sender, PeerEventArgs e) { PeerConnected(sender, e); }
        internal void EventPeerAccepted(object sender, PeerEventArgs e) { PeerAccepted(sender, e); }
        internal void EventPeerDataReceived(object sender, PeerEventArgs e) { PeerDataReceived(sender, e); }
        internal void EventPeerDisconnected(object sender, PeerEventArgs e) { PeerDisconnected(sender, e); }

        private void Peer_DataReceived(object sender, PeerEventArgs e)
        {
            PeerDataReceived(this, e);
        }

        private void Peer_Disconnected(object sender, PeerEventArgs e)
        {
            if (peers.Remove(e.peer)) PeerDisconnected(this, e);
            pendingPeers.Remove(e.peer);
            GC.Collect();
        }
    }
}
