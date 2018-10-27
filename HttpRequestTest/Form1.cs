using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HttpRequestTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button_req_Click(object sender, EventArgs e)
        {
            try
            {
                var web = new WebClient();
                textBox_log.AppendText("Start\r\n");
                var str = web.DownloadString(new Uri(textBox_path.Text));
                textBox_log.AppendText($"End:{str.Length}\r\n");
            }
            catch(Exception ex)
            {
                textBox_log.AppendText(ex.Message + "\r\n");
            }
        }
    }
}
