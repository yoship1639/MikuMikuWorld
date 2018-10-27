using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MikuMikuWorld.Network;

namespace Exporter_MMW
{
    public partial class CollisionSphere : UserControl, ICollisionShape
    {
        private NwCollisionSphere shape;
        public CollisionSphere(NwCollisionSphere shape)
        {
            InitializeComponent();
            this.shape = shape;

            if (shape != null)
            {
                numericUpDown_radius.Value = (decimal)shape.Radius;
                numericUpDown_radius.ValueChanged += (s, e) => shape.Radius = (float)numericUpDown_radius.Value;
            }
        }
    }
}
