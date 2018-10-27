using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MikuMikuWorld
{
    public partial class PictSaveForm : Form
    {
        public PictSaveForm()
        {
            InitializeComponent();
        }

        private void textBox_name_TextChanged(object sender, EventArgs e)
        {
            button_ok.Enabled = !string.IsNullOrWhiteSpace(textBox_name.Text);
        }
    }
}
