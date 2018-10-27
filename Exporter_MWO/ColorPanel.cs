using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Exporter_MMW
{
    public partial class ColorPanel : UserControl
    {
        public ColorPanel()
        {
            InitializeComponent();
        }

        private void ColorPanel_Load(object sender, EventArgs e)
        {
            BackColor = Color.Black;
        }

        private void ColorPanel_Click(object sender, EventArgs e)
        {
            var cd = new ColorDialog();
            cd.Color = BackColor;

            if (cd.ShowDialog() == DialogResult.OK)
            {
                BackColor = cd.Color;
                ColorChanged(this, cd.Color);
            }
        }

        public event EventHandler<Color> ColorChanged = delegate { };
    }
}
