using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Controls
{
    public class TabControl : Control
    {
        public Tab[] Tabs { get; set; }
        public int TabIndex { get; set; }
        public int SelectIndex { get; set; }
        public object SelectedObject
        {
            get
            {
                if (Tabs == null || !MMWMath.Clamped(TabIndex, 0, Tabs.Length - 1)) return null;
                if (Tabs[TabIndex].Items == null || !MMWMath.Clamped(SelectIndex, 0, Tabs[TabIndex].Items.Length - 1)) return null;
                return Tabs[TabIndex].Items[SelectIndex];
            }
        }

        public override void Draw(Graphics g, double deltaTime)
        {
            if (Tabs == null || Tabs.Length == 0) return;

            var widths = new int[Tabs.Length];
            for (var i = 0; i < Tabs.Length; i++)
            {
                widths[i] = (int)ControlDrawer.MeasureString(g, Tabs[i].Name).Width + 24;
            }

            ControlDrawer.DrawTabControl(g, widths, TabIndex, WorldLocation.X, WorldLocation.Y, Size.X, Size.Y, 30, Focus);

            var offset = WorldLocation.X;
            for (var i = 0; i < Tabs.Length; i++)
            {
                ControlDrawer.DrawString(g, Tabs[i].Name, offset + 12, WorldLocation.Y + 2, Focus && TabIndex == i);
                offset += widths[i];
            }

            var tab = Tabs[TabIndex];
            if (tab.Items != null)
            {
                for (var i = 0; i < tab.Items.Length; i++)
                {
                    ControlDrawer.DrawString(g, tab.Items[i].ToString(), WorldLocation.X + 32, WorldLocation.Y + 64 + (24 * i), Focus && SelectIndex == i);
                }
            }
        }

        public void NextTab()
        {
            if (Tabs == null) return;
            TabIndex = MMWMath.Repeat(TabIndex + 1, 0, 1);
            if (Tabs[TabIndex].Items == null || Tabs[TabIndex].Items.Length == 0) return;
            SelectIndex = MMWMath.Clamp(SelectIndex, 0, Tabs[TabIndex].Items.Length - 1);
        }

        public void PrevTab()
        {
            if (Tabs == null) return;
            TabIndex = MMWMath.Repeat(TabIndex - 1, 0, 1);
            if (Tabs[TabIndex].Items == null || Tabs[TabIndex].Items.Length == 0) return;
            SelectIndex = MMWMath.Clamp(SelectIndex, 0, Tabs[TabIndex].Items.Length - 1);
        }

        public void NextSelect()
        {
            if (Tabs == null) return;
            if (Tabs[TabIndex].Items == null || Tabs[TabIndex].Items.Length == 0) return;
            SelectIndex = MMWMath.Repeat(SelectIndex + 1, 0, Tabs[TabIndex].Items.Length - 1);
        }

        public void PrevSelect()
        {
            if (Tabs == null) return;
            if (Tabs[TabIndex].Items == null || Tabs[TabIndex].Items.Length == 0) return;
            SelectIndex = MMWMath.Repeat(SelectIndex - 1, 0, Tabs[TabIndex].Items.Length - 1);
        }
    }

    public class Tab
    {
        public string Name { get; set; }
        public object[] Items { get; set; }
    }
}
