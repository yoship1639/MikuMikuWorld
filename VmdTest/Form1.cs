using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VmdTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;

            var importer = new VmdMotionImporter.VmdMotionImporter();
            var res = importer.Import(ofd.FileName);

            if (res.result != VmdMotionImporter.VmdImportResult.Result.Success) return;

            
        }
    }
}
