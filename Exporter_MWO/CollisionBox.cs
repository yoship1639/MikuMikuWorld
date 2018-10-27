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
    public partial class CollisionBox : UserControl, ICollisionShape
    {
        private NwCollisionBox shape;
        public CollisionBox(NwCollisionBox shape)
        {
            InitializeComponent();
            this.shape = shape;

            if (shape != null)
            {
                numericUpDown_x.Value = (decimal)shape.HalfExtents.X;
                numericUpDown_y.Value = (decimal)shape.HalfExtents.Y;
                numericUpDown_z.Value = (decimal)shape.HalfExtents.Z;

                numericUpDown_x.ValueChanged += (s, e) => shape.HalfExtents.X = (float)numericUpDown_x.Value;
                numericUpDown_y.ValueChanged += (s, e) => shape.HalfExtents.Y = (float)numericUpDown_y.Value;
                numericUpDown_z.ValueChanged += (s, e) => shape.HalfExtents.Z = (float)numericUpDown_z.Value;
            }
        }
    }
}
