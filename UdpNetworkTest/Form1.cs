using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UdpNetworkTest
{
    public partial class Form1 : Form
    {
        UdpClient udp;
        UdpClient client;

        Task task;

        private static readonly int BufferSize = 256 * 256;

        public Form1()
        {
            InitializeComponent();

            udp = new UdpClient();
            udp.Client.ReceiveBufferSize = BufferSize;
            udp.Client.SendBufferSize = BufferSize;

            task = Task.Factory.StartNew(() =>
            {
                client = new UdpClient(new IPEndPoint(IPAddress.Any, (int)numericUpDown_port.Value));
                client.Client.ReceiveBufferSize = BufferSize;
                client.Client.SendBufferSize = BufferSize;

                while (true)
                {
                    try
                    {
                        IPEndPoint ip = null;
                        var data = client.Receive(ref ip);

                        Invoke(new Method(method), data);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    Thread.Sleep(100);
                }
            });
        }

        public delegate void Method(byte[] data);

        private void method(byte[] data)
        {
            textBox_recv.AppendText(data.Length + Environment.NewLine);
        }

        private void button_send_Click(object sender, EventArgs e)
        {
            var bytes = new byte[(int)numericUpDown_size.Value];
            var l = udp.Send(bytes, bytes.Length, new IPEndPoint(IPAddress.Parse("127.0.0.1"), (int)numericUpDown_port2.Value));
        }

        private void numericUpDown_port_ValueChanged(object sender, EventArgs e)
        {
            client.Close();
            client = new UdpClient(new IPEndPoint(IPAddress.Any, (int)numericUpDown_port.Value));
            client.Client.ReceiveBufferSize = BufferSize;
            client.Client.SendBufferSize = BufferSize;
        }
    }
}
