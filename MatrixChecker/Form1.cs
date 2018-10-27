using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MatrixChecker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var z = new StringMatrix();
            z.M[0, 0] = "Cz";
            z.M[0, 1] = "Sz";
            z.M[1, 0] = "-Sz";
            z.M[1, 1] = "Cz";

            var x = new StringMatrix();
            x.M[1, 1] = "Cx";
            x.M[1, 2] = "Sx";
            x.M[2, 1] = "-Sx";
            x.M[2, 2] = "Cx";

            var zx = z * x;

            var y = new StringMatrix();
            y.M[0, 0] = "Cy";
            y.M[0, 2] = "-Sy";
            y.M[2, 0] = "Sy";
            y.M[2, 2] = "Cy";

            var zxy = zx * y;

            Console.WriteLine(zxy);
        }
    }
}
