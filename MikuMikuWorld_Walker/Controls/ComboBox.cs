using MikuMikuWorld.Walker;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Controls
{
    class ComboBox : Control
    {
        public Color4 BackBrush { get; set; } = Color.FromArgb(255, 128, 128, 128);
        public Color4 BackBrushDisabled { get; set; } = Color.FromArgb(255, 64, 64, 64);
        public Brush Brush { get; set; } = Brushes.White;
        public Brush BrushDisabled { get; set; } = new SolidBrush(Color.FromArgb(128, 255, 255, 255));
        public bool Enabled { get; set; } = true;
        public string Text { get; set; }

        public object[] Items { get; set; }
        public string DisplayMember { get; set; }
        private int selectedIndex;
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                selectedIndex = value;
                if (value < 0 || value >= Items.Length) selectedIndex = 0;
                Text = ((Achivement)Items[selectedIndex]).Name;
            }
        }

        public event EventHandler<int> SelectedIndexChanged = delegate { };

        public ComboBox(Control parent, Vector2 location, Vector2 size)
        {
            Parent = parent;
            Size = size;
            LocalLocation = location;

            Clicked += (s, e) =>
            {
                var form = new ComboBoxForm();
                form.Text = "Achivements";
                form.comboBox.Items.AddRange(Items);
                form.comboBox.DisplayMember = DisplayMember;
                form.comboBox.SelectedIndex = SelectedIndex;
                var res = form.ShowDialog();
                if (res == System.Windows.Forms.DialogResult.OK)
                {
                    Text = ((Achivement)form.comboBox.SelectedItem).Name;
                    SelectedIndex = form.comboBox.SelectedIndex;
                    SelectedIndexChanged(this, form.comboBox.SelectedIndex);
                }
            };
        }

        public override void Draw(Graphics g, double deltaTime)
        {
            var l = GetLocation(Size.X, Size.Y, Alignment);
            var x = l.X + WorldLocation.X;
            var y = l.Y + WorldLocation.Y;
            var w = Size.X;
            var h = Size.Y;

            var back = BackBrush;
            var brush = Brush;
            if (!Enabled)
            {
                back = BackBrushDisabled;
                brush = BrushDisabled;
            }
            ControlDrawer.DrawFrame(x, y, w, h, back);

            g.SetClip(new RectangleF(x + 1, y + 1, w - 3 - 6, h - 3));
            g.DrawString(Text, DefaultFont, Brush, new RectangleF(x + 6.0f, y + 3.0f, w - 13.0f, h - 7.0f));
            g.ResetClip();
        }
    }
}
