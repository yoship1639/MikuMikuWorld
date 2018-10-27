using MikuMikuWorld.Walker;
using MikuMikuWorldScript;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Networks
{
    static class NetworkUtil
    {
        public static readonly int TcpBufferSize = 16 * 1024 * 1024;
        private static byte[] receiveBuffer = new byte[TcpBufferSize];

        public static WorldInfo QueryWorldInfoUdp(string host, int port)
        {
            try
            {
                var ips = Dns.GetHostAddresses(host);

                var p = Array.Find(ips, i => i.AddressFamily == AddressFamily.InterNetwork);

                var ip = new IPEndPoint(p, port);

                byte[] data;
                var po = RandomHelper.NextInt(49152, 65535);
                using (var req = new UdpClient(po, AddressFamily.InterNetwork))
                {
                    req.Client.ReceiveTimeout = 3000;
                    using (var ms = new MemoryStream())
                    {
                        using (var bw = new BinaryWriter(ms))
                        {
                            bw.Write(DataType.Magic);
                            bw.Write(DataType.RequestServerDesc);
                            bw.Write(po);
                        }

                        var buf = ms.ToArray();
                        req.SendAsync(buf, buf.Length, ip);
                    }

                    IPEndPoint ep = null;
                    data = req.Receive(ref ep);
                    var desc = Util.DeserializeJson<WorldInfo>(data.ToJson());
                    return desc;
                }

                
            }
            catch { }

            return null;
        }

        public static Server Connect(string host, int port)
        {
            try
            {
                var ips = Dns.GetHostAddresses(host);
                var p = Array.Find(ips, i => i.AddressFamily == AddressFamily.InterNetwork);
                var ip = new IPEndPoint(p, port);

                var tcp = new TcpClient(host, port)
                {
                    ReceiveTimeout = 5000
                };

                //var res = Receive(tcp.GetStream());
                //if (res.DataType != DataType.ResponseServerDesc) return null;
                //ServerDesc desc = Buffer.JsonDeserialize<ServerDesc>(res.Data);

                return new Server(tcp);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return null;
        }

        public static ReceiveData Receive(NetworkStream ns)
        {
            List<byte> data = new List<byte>();
            int totalLength = 0;
            int l = 0;
            while ((l = ns.Read(receiveBuffer, 0, receiveBuffer.Length)) != 0)
            {
                var magic = receiveBuffer.Take(4).ToArray();
                if (magic.SequenceEqual(DataType.Magic))
                {
                    totalLength = BitConverter.ToInt32(receiveBuffer, 4);
                    var d = receiveBuffer.Skip(8).Take(l - 8);
                    data.AddRange(d);
                }
                else
                {
                    data.AddRange(receiveBuffer.Take(l));
                }

                if (data.Count == totalLength)
                {
                    var dataType = BitConverter.ToInt32(data.ToArray(), 0);
                    var buf = data.Skip(4).ToArray();
                    return new ReceiveData()
                    {
                        Data = buf,
                        DataType = dataType,
                        Succeeded = true,
                    };
                }
                else if (data.Count > totalLength)
                {
                    return new ReceiveData()
                    {
                        Succeeded = false,
                    };
                }
            }

            return null;
        }
        
        public static void SendTcp(NetworkStream ns, int dataType, byte[] buf)
        {
            int seek = 0;
            var data = DataType.Magic
                .Concat(BitConverter.GetBytes(buf.Length + 4))
                .Concat(BitConverter.GetBytes(dataType))
                .Concat(buf).ToArray();

            while (seek < data.Length)
            {
                var length = data.Length - seek;
                if (length > TcpBufferSize) length = TcpBufferSize;
                var d = data.Skip(seek).Take(length).ToArray();
                ns.Write(d, 0, d.Length);
                seek += TcpBufferSize;
            }
        }

        public static string ToJson(this byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                using (var br = new BinaryReader(ms))
                {
                    return br.ReadString();
                }
            }
        }
        public static byte[] ToBytes(this string json)
        {
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    bw.Write(json);
                }
                return ms.ToArray();
            }
        }
    }

    public class ReceiveData
    {
        public int DataType;
        public byte[] Data;
        public bool Succeeded;
    }
}
