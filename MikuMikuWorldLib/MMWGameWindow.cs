//
// Miku Miku World License
//
// Copyright (c) 2017 Miku Miku World.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//

using MikuMikuWorld.GameComponents;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;
using System.ComponentModel;
using System.Windows.Forms;

namespace MikuMikuWorld
{
    public class MMWGameWindow : GameWindow
    {
        public MMWGameWindow(int width, int height, string title, MMWConfiguration conf) :
            base(width, height, GraphicsMode.Default, "MikuMikuWorld - " + title, GameWindowFlags.Default, MMW.HMDCamera.DisplayDevice)
        {
            MMW.Configuration = conf;
        }

        public MMWGameWindow(int width, int height, string title) :
            base(width, height, GraphicsMode.Default, "MikuMikuWorld - " + title, GameWindowFlags.Default, MMW.HMDCamera.DisplayDevice)
        { }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            MMW.ClientSize = ClientSize;
            MMW.WindowSize = Size;
            var size = ClientSize;

            var pos = PointToScreen(new System.Drawing.Point(0, 0));
            MMW.X = pos.X;
            MMW.Y = pos.Y;

            if (MMW.Configuration.Antialias == MMWConfiguration.AntialiasType.SSAAHalf)
            {
                size.Width /= 2;
                size.Height /= 2;
            }
            else if (MMW.Configuration.Antialias == MMWConfiguration.AntialiasType.SSAAx2)
            {
                size.Width *= 2;
                size.Height *= 2;
            }
            else if (MMW.Configuration.Antialias == MMWConfiguration.AntialiasType.SSAAx4)
            {
                size.Width *= 4;
                size.Height *= 4;
            }
            MMW.RenderResolution = size;

            MMW.Window = this;

            MMW.Init();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            MMW.Update(e.Time);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            MMW.Draw(e.Time);
            SwapBuffers();
        }

        protected override void OnMove(EventArgs e)
        {
            base.OnMove(e);
            var pos = PointToScreen(new System.Drawing.Point(0, 0));
            MMW.X = pos.X;
            MMW.Y = pos.Y;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (!ClientSize.IsEmpty)
            {
                MMW.WindowSize = Size;
                MMW.ClientSize = ClientSize;
                MMW.Resize();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            MMW.Destroy();
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Escape)
            {
                //if (MessageBox.Show("ゲームを終了しますか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.Yes)
                //{
                    Close();
                //}
            }
        }
    }
}
