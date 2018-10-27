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
    public partial class CollisionCylinder : UserControl, ICollisionShape
    {
        private NwCollisionCylinder shape;
        public CollisionCylinder(NwCollisionCylinder shape)
        {
            InitializeComponent();
            this.shape = shape;

            if (shape != null)
            {
                numericUpDown_height.Value = (decimal)shape.Height;
                numericUpDown_radius.Value = (decimal)shape.Radius;

                numericUpDown_height.ValueChanged += (s, e) => shape.Height = (float)numericUpDown_height.Value;
                numericUpDown_radius.ValueChanged += (s, e) => shape.Radius = (float)numericUpDown_radius.Value;
            }
        }
    }
}
