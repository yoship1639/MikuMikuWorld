using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;

namespace MikuMikuWorld
{
    public partial class MMWGLControl : GLControl
    {
        private System.Timers.Timer timer = new System.Timers.Timer();

        private new bool DesignMode
        {
            get
            {
                bool design = base.DesignMode;

                Control parent = this.Parent;
                while (parent != null)
                {
                    ISite site = parent.Site;
                    if (site != null) design |= site.DesignMode;
                    parent = parent.Parent;
                }

                return design;
            }
        }

        public MMWGLControl() : base(new GraphicsMode(32, 24, 8, 0))
        {
            InitializeComponent();
        }
    }
}
