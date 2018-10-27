using MikuMikuWorld.Networks.Commands;
using MikuMikuWorld.Walker;
using MikuMikuWorld.Assets;
using MikuMikuWorldScript;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MikuMikuWorld.Networks
{
    class Server : Assets.IAsset, IServer
    {
        public bool Loaded { get; private set; }
        
        public string Name { get; set; }

        public int SessionID { get; internal set; }

        private TcpClient tcp;
        private UdpClient udp;
        public IPAddress RemoteAddress => ((IPEndPoint)tcp.Client.RemoteEndPoint).Address;

        public IPEndPoint RemoteEndPoint { get; internal set; }
        public IPAddress MulticastAddress { get; internal set; }
        public int LocalUdpPort { get; internal set; }
        public int RemoteUdpPort { get; internal set; }

        public int ReceiveTimeout
        {
            get { return tcp.ReceiveTimeout; }
            set { tcp.ReceiveTimeout = value; }
        }

        public bool Connected => tcp.Connected;

        public event EventHandler<DataEventArgs> TcpDataReceived = delegate { };
        public event EventHandler<DataEventArgs> UdpDataReceived = delegate { };
        public event EventHandler Disconnected = delegate { };
        
        public List<ICmd> BeforeCmds = new List<ICmd>()
        {
            new CmdReceiveUdpPort(),
        };

        public Server(TcpClient tcp)
        {
            this.tcp = tcp;
            tcp.ReceiveTimeout = 3 * 60 * 1000;
        }

        public Result Load()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    while (true)
                    {
                        var res = NetworkUtil.Receive(tcp.GetStream());
                        if (!res.Succeeded) continue;
                        TcpDataReceived(this, new DataEventArgs()
                        {
                            Data = res.Data,
                            DataType = res.DataType,
                            IsTcp = true,
                        });
                    }
                }
                catch { }

                Disconnected?.Invoke(this, EventArgs.Empty);
            });

            TcpDataReceived += (s, e) =>
            {
                if (priDataType == e.DataType)
                {
                    priDataType = -1;
                    resData = e.Data;
                    return;
                }
                try
                {
                    var cmd = BeforeCmds.Find(c => c.ExecutableDataTypes.Contains(e.DataType));
                    if (cmd != null)
                    {
                        cmd.Execute(this, e.DataType, e.Data, e.IsTcp);
                        return;
                    }
                    var coms = MMW.FindGameComponents(c => c.TcpAcceptDataTypes != null && c.TcpAcceptDataTypes.Contains(e.DataType));
                    for (var i = 0; i < coms.Length; i++)
                    {
                        coms[i].OnTcpReceived(e.DataType, e.Data);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            };

            UdpDataReceived += (s, e) =>
            {
                try
                {
                    var cmd = BeforeCmds.Find(c => c.ExecutableDataTypes.Contains(e.DataType));
                    if (cmd != null)
                    {
                        cmd.Execute(this, e.DataType, e.Data, e.IsTcp);
                        return;
                    }
                    var coms = MMW.FindGameComponents(c => c.UdpAcceptDataTypes != null && c.UdpAcceptDataTypes.Contains(e.DataType));
                    for (var i = 0; i < coms.Length; i++)
                    {
                        coms[i].OnUdpReceived(e.DataType, e.Data);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            };

            Loaded = true;
            return Result.Success;
        }

        public Result Unload()
        {
            Disconnected = null;

            try {  tcp.Close(); }
            catch { }

            try { udp.Close(); }
            catch { }

            Loaded = false;
            return Result.Success;
        }

        public void SendTcp(int dataType, byte[] data)
        {
            if (!tcp.Connected) return;
            try
            {
                NetworkUtil.SendTcp(tcp.GetStream(), dataType, data);
            }
            catch { }
        }
        public void SendTcp(int dataType, string json)
        {
            if (!tcp.Connected) return;
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    bw.Write(json);
                }
                SendTcp(dataType, ms.ToArray());
            }
        }
        public void SendTcp(int dataType, object obj)
        {
            if (!tcp.Connected) return;
            var data = Util.SerializeJsonBinary(obj);
            SendTcp(dataType, data);
        }
        public void SendTcpCompress(int dataType, object obj)
        {
            if (!tcp.Connected) return;
            var data = Util.SerializeJsonBinaryCompress(obj);
            SendTcp(dataType, data);
        }

        public void SendUdp(byte[] data)
        {
            if (udp == null) return;

            var buf = DataType.Magic.Concat(data).ToArray();

            udp.Send(buf, buf.Length, RemoteEndPoint);
        }

        public void StartUdp()
        {
            if (udp != null)
            {
                try
                {
                    udp.Close();
                }
                catch { }
                udp = null;
            }

            Task.Factory.StartNew(() =>
            {
                try
                {
                    udp = new UdpClient(LocalUdpPort, AddressFamily.InterNetwork);
                    udp.JoinMulticastGroup(MulticastAddress, 50);

                    while (true)
                    {
                        IPEndPoint ep = null;
                        var data = udp.Receive(ref ep);

                        if (!DataType.Magic.SequenceEqual(data.Take(4))) continue;
                        Buffer.Read(data, (ms, br) =>
                        {
                            br.ReadBytes(4);
                            while (ms.Position < data.Length)
                            {
                                var type = br.ReadInt32();
                                var l = br.ReadInt32();
                                var buf = br.ReadBytes(l);
                                UdpDataReceived(this, new DataEventArgs()
                                {
                                    Data = buf,
                                    DataType = type,
                                    IsTcp = false,
                                });
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                
            });
        }

        internal void EventTcpDataReceived(DataEventArgs args) => TcpDataReceived(this, args);
        internal void EventUdpDataReceived(DataEventArgs args) => UdpDataReceived(this, args);

        private int priDataType = -1;
        private byte[] resData = null;
        public byte[] Request(int reqDataType, object reqObj, int resDataType, int timeout = Timeout.Infinite)
        {
            if (!tcp.Connected) return null;
            var reqJson = Util.SerializeJson(reqObj);
            priDataType = resDataType;
            resData = null;
            SendTcp(reqDataType, reqJson);

            int ms = 0;
            while (resData == null)
            {
                Thread.Sleep(10);
                ms += 10;

                if (ms > timeout && timeout >= 0)
                {
                    priDataType = -1;
                    return null;
                }

                if (!tcp.Connected)
                {
                    priDataType = -1;
                    return null;
                }
            }

            var res = resData.ToArray();
            priDataType = resDataType;
            resData = null;
            return res;
        }
        public T RequestJson<T>(int reqDataType, object reqObj, int resDataType, int timeout = Timeout.Infinite) where T : class
        {
            if (!tcp.Connected) return default(T);
            var reqJson = Util.SerializeJson(reqObj);
            priDataType = resDataType;
            resData = null;
            SendTcp(reqDataType, reqJson);

            int ms = 0;
            while (resData == null)
            {
                Thread.Sleep(10);
                ms += 10;

                if (ms > timeout && timeout >= 0)
                {
                    priDataType = -1;
                    return null;
                }

                if (!tcp.Connected)
                {
                    priDataType = -1;
                    return null;
                }
            }

            var resJson = resData.ToJson();
            priDataType = resDataType;
            resData = null;
            return Util.DeserializeJson<T>(resJson);
        }
        public T RequestCompJson<T>(int reqDataType, object reqObj, int resDataType, int timeout = Timeout.Infinite) where T : class
        {
            if (!tcp.Connected) return default(T);
            var reqJson = Util.SerializeJson(reqObj);
            priDataType = resDataType;
            resData = null;
            SendTcp(reqDataType, reqJson);

            int ms = 0;
            while (resData == null)
            {
                Thread.Sleep(10);
                ms += 10;

                if (ms > timeout && timeout >= 0)
                {
                    priDataType = -1;
                    return null;
                }

                if (!tcp.Connected)
                {
                    priDataType = -1;
                    return null;
                }
            }

            var decomp = Util.Decompress(resData);
            var resJson = Encoding.UTF8.GetString(decomp);
            priDataType = resDataType;
            resData = null;
            return Util.DeserializeJson<T>(resJson);
        }
        public T RequestBson<T>(int reqDataType, object reqObj, int resDataType, int timeout = Timeout.Infinite) where T : class
        {
            if (!tcp.Connected) return default(T);
            var reqJson = Util.SerializeJson(reqObj);
            priDataType = resDataType;
            resData = null;
            SendTcp(reqDataType, reqJson);

            int ms = 0;
            while (resData == null)
            {
                Thread.Sleep(10);
                ms += 10;

                if (ms > timeout && timeout >= 0)
                {
                    priDataType = -1;
                    return null;
                }

                if (!tcp.Connected)
                {
                    priDataType = -1;
                    return null;
                }
            }

            var decomp = Util.Decompress(resData);
            return Util.DeserializeBson<T>(decomp);
        }
    }

    class DataEventArgs : EventArgs
    {
        //public Player Player = null;
        public int DataType;
        public byte[] Data;
        public bool IsTcp;
    }
}
