using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TcpNetworkTest
{
    public partial class Form1 : Form
    {
        public float Version { get; set; }
        public int SessionID { get; set; }
        public int UdpPort { get; set; }
        public string ServerName { get; set; }
        public string ServerDesc { get; set; }
        public bool ServerPass { get; set; }
        public int MaxPlayer { get; set; }
        public int NowPlayer { get; set; }
        public bool AllowScript { get; set; }
        public bool AllowUserCharacter { get; set; }
        public CultureInfo Culture { get; set; }

        public Form1()
        {
            InitializeComponent();

            TcpReceived += (s, e) =>
            {
                using (var ms = new MemoryStream(e))
                {
                    using (var br = new BinaryReader(ms))
                    {
                        var pref = br.ReadInt32();
                        if (pref == 1)
                        {
                            try
                            {
                                Version = br.ReadSingle();
                                SessionID = br.ReadInt32();
                                ServerName = br.ReadString();
                                ServerDesc = br.ReadString();
                                ServerPass = br.ReadBoolean();
                                MaxPlayer = br.ReadInt32();
                                NowPlayer = br.ReadInt32();
                                Culture = CultureInfo.GetCultureInfo(br.ReadInt32());

                                Log("SessionID:" + SessionID);
                            }
                            catch { }



                            using (var ms2 = new MemoryStream())
                            {
                                using (var bw = new BinaryWriter(ms2))
                                {
                                    bw.Write(2);
                                    bw.Write(textBox_text.Text);
                                    bw.Write("Test User");
                                    bw.Write("UniqueID");
                                    bw.Write(1.0f);
                                    bw.Write(1.0f);
                                    bw.Write(1.0f);
                                    bw.Write(1.0f);

                                    var stream = new MemoryStream();
                                    Bitmap.FromFile(@"C:\Users\yoship\Downloads\16hakodot.png").Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                                    bw.Write((int)stream.Length);
                                    bw.Write(stream.GetBuffer());

                                    SendTcp(ms2.ToArray());
                                }
                            }
                        }
                        else if (pref == 5)
                        {
                            UdpPort = br.ReadInt32();
                        }
                    }
                }
            };
        }

        public static readonly int BufferSize = 16 * 1024 * 1024;

        TcpClient client;
        UdpClient udp;

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                button1.Enabled = false;
                var ip = new IPEndPoint(IPAddress.Parse(textBox_ip.Text), int.Parse(textBox_port.Text));
                client = new TcpClient();
                client.ReceiveBufferSize = BufferSize;
                client.SendBufferSize = BufferSize;
                client.Connect(ip);
                if (!client.Connected)
                {
                    button1.Enabled = true;
                    return;
                }
                Log("接続しました");

                Task.Factory.StartNew(() =>
                {
                    var stream = client.GetStream();

                    var bytes = new byte[BufferSize];
                    int l;

                    //メッセージを受信 
                    try
                    {
                        List<byte> data = new List<byte>();
                        int totalLength = 0;
                        while ((l = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            var magic = bytes.Take(4).ToArray();
                            if (magic.SequenceEqual(Magic))
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
                                TcpReceived(this, data.ToArray());
                                Log("受信:" + BitConverter.ToInt32(data.ToArray(), 0));
                                data.Clear();
                                totalLength = 0;
                            }
                            else if (data.Count > totalLength)
                            {
                                Log("オーバー:" + data.Count);
                                data.Clear();
                                totalLength = 0;
                            }
                        }
                    }
                    catch { }

                    Log("切断しました");
                    button1.Enabled = true;
                });

                Task.Factory.StartNew(() =>
                {
                    udp = new UdpClient(new IPEndPoint(IPAddress.Any, 39393));
                    udp.JoinMulticastGroup(IPAddress.Parse("239.0.0.39"));
                    udp.Client.ReceiveBufferSize = BufferSize;
                    udp.Client.SendBufferSize = BufferSize;

                    while (true)
                    {
                        try
                        {
                            IPEndPoint ep = null;
                            var data = udp.Receive(ref ep);

                            if (!data.Take(4).SequenceEqual(Magic)) continue;

                            var buf = data.Skip(4).Take(data.Length - 4).ToArray();

                            UdpReceived(this, buf.ToArray());
                            Log("受信:" + buf.Length);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                });
            }
            catch(Exception ex)
            {
                Log(ex.ToString());
                button1.Enabled = true;
            }
        }

        public event EventHandler<byte[]> TcpReceived = delegate { };
        public event EventHandler<byte[]> UdpReceived = delegate { };

        private static readonly byte[] Magic = Encoding.UTF8.GetBytes("MMWN");

        public bool SendTcp(byte[] buf)
        {
            try
            {
                int seek = 0;
                var data = Magic.Concat(BitConverter.GetBytes(buf.Length)).Concat(buf).ToArray();
                var stream = client.GetStream();

                while (seek < data.Length)
                {
                    var length = data.Length - seek;
                    if (length > BufferSize) length = BufferSize;
                    var d = data.Skip(seek).Take(length).ToArray();
                    stream.Write(d, 0, d.Length);
                    seek += BufferSize;
                }

                return true;
            }
            catch { }

            return false;
        }

        private delegate void AddMethod(string str);

        public void Log(object str)
        {
            Invoke(new AddMethod(textBox_log.AppendText), str + Environment.NewLine);
        }

        private void textBox_text_TextChanged(object sender, EventArgs e)
        {
            Console.WriteLine(Magic.Length);
        }

        private void button_send_Click(object sender, EventArgs e)
        {
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    bw.Write(100);
                    bw.Write(textBox_text.Text);
                }
                var buf = ms.ToArray();
                SendTcp(buf);
            }
        }

        private void button_sendudp_Click(object sender, EventArgs e)
        {
            var ep = new IPEndPoint(IPAddress.Parse(textBox_ip.Text), UdpPort);
            var udp = new UdpClient();

            var buf = Magic.Concat(BitConverter.GetBytes(SessionID)).Concat(BitConverter.GetBytes(100)).ToArray();

            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    bw.Write(textBox_text.Text);

                    var l = BitConverter.GetBytes((int)ms.Length);
                    buf = buf.Concat(l).Concat(ms.ToArray()).ToArray();
                    udp.Send(buf, buf.Length, ep);
                }
            }
        }
    }
}
