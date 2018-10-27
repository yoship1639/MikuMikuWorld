using MikuMikuWorld.Walker;
using MikuMikuWorldScript;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MikuMikuWorld_Walker_Server
{
    public class Peer
    {
        public string Name { get; set; }
        public WalkerPlayer Player { get; set; }
        public int SessionID { get; private set; }
        public uint UniqueID { get; internal set; }

        public bool Pending { get; internal set; }
        internal TcpClient tcp;
        internal UdpClient udp;

        public IPAddress IPAddress { get; private set; }
        public int TcpPort { get; private set; }
        public int UdpPort { get; private set; }
        public IPEndPoint EndPoint { get; private set; }
        public IPAddress MulticastAddress { get; private set; }

        private Task tcpTask;
        private Task udpTask;

        public static readonly int TcpBufferSize = 16 * 1024 * 1024;
        public static readonly int UdpBufferSize = 64000;

        public Peer(TcpClient client)
        {
            tcp = client;
            client.SendBufferSize = TcpBufferSize;
            client.ReceiveBufferSize = TcpBufferSize;

            IPAddress = ((IPEndPoint)tcp.Client.RemoteEndPoint).Address;
            TcpPort = ((IPEndPoint)tcp.Client.RemoteEndPoint).Port;
            EndPoint = (IPEndPoint)tcp.Client.RemoteEndPoint;

            SessionID = GetHashCode();
            Pending = true;

            tcpTask = Task.Factory.StartNew(() =>
            {
                var stream = client.GetStream();

                var bytes = new byte[TcpBufferSize];
                int l;

                //メッセージを受信 
                try
                {
                    List<byte> data = new List<byte>();
                    int totalLength = 0;
                    while ((l = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        var magic = bytes.Take(4).ToArray();
                        if (magic.SequenceEqual(NetworkServer.Magic))
                        {
                            totalLength = BitConverter.ToInt32(bytes, 4);
                            var d = bytes.Skip(8).Take(l - 8);
                            data.AddRange(d);
                        }
                        else
                        {
                            data.AddRange(bytes.Take(l));
                        }

                        if (data.Count == totalLength)
                        {
                            DataReceived(this, new PeerEventArgs()
                            {
                                peer = this,
                                data = data.ToArray(),
                                length = data.Count,
                                isTcp = true,
                            });
                            data.Clear();
                            totalLength = 0;
                        }
                        else if (data.Count > totalLength)
                        {
                            Console.WriteLine(data.Count + " : " + totalLength);
                            data.Clear();
                            totalLength = 0;
                        }
                    }
                }
                catch { }

                client.Close();
                closed = true;
                Disconnected(this, new PeerEventArgs()
                {
                    peer = this,
                });
            });
        }

        public void SendTcp(int dataType, byte[] buf)
        {
            int seek = 0;
            var data = NetworkServer.Magic.Concat(BitConverter.GetBytes(buf.Length + 4)).Concat(BitConverter.GetBytes(dataType)).Concat(buf).ToArray();
            var stream = tcp.GetStream();

            while (seek < data.Length)
            {
                var length = data.Length - seek;
                if (length > TcpBufferSize) length = TcpBufferSize;
                var d = data.Skip(seek).Take(length).ToArray();
                stream.Write(d, 0, d.Length);
                seek += TcpBufferSize;
            }
        }
        public void SendTcp(int dataType, string json)
        {
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    bw.Write(json);
                }
                SendTcp(dataType, ms.ToArray());
            }
        }
        public void SendUdp(byte[] buf)
        {
            if (buf.Length > UdpBufferSize) return;

            var data = NetworkServer.Magic.Concat(buf).ToArray();

            try
            {
                udp.SendAsync(data, data.Length);
            }
            catch { }
        }

        public void StartUdp(int udpPort)
        {
            UdpPort = udpPort;
            udpTask = Task.Factory.StartNew(() =>
            {
                try
                {
                    udp = new UdpClient(UdpPort, AddressFamily.InterNetwork);
                }
                catch (Exception ex)
                {
                    Log.LogError(ex.ToString(), true);
                    return;
                }

                while (!closed)
                {
                    try
                    {
                        IPEndPoint ip = null;
                        var data = udp.Receive(ref ip);

                        if (data.Length > UdpBufferSize + 4) continue;
                        if (!NetworkServer.Magic.SequenceEqual(data.Take(4))) continue;

                        var buf = data.Skip(4).Take(data.Length - 4).ToArray();
                        DataReceived(this, new PeerEventArgs()
                        {
                            peer = this,
                            data = buf,
                            isTcp = false,
                            length = buf.Length,
                        });
                    }
                    catch { }

                }
            });
        }

        private bool closed = false;

        public void Close()
        {
            closed = true;
            tcp.Close();
            udp.Close();
        }

        public event EventHandler<PeerEventArgs> DataReceived = delegate { };
        public event EventHandler<PeerEventArgs> Disconnected = delegate { };

        public override string ToString()
        {
            return Name;
        }
    }

    public class PeerEventArgs : EventArgs
    {
        public int length;
        public Peer peer;
        public byte[] data;
        public bool isTcp;
    }
}
