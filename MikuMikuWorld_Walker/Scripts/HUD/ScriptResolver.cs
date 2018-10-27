using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Scripts.HUD
{
    class ScriptResolver : GameComponent
    {
        private ScriptForm form;

        protected override void Update(double deltaTime)
        {
            if (form == null && Input.Ctrl && !Input.Alt && !Input.Shift && Input.IsKeyPressed(OpenTK.Input.Key.Enter))
            {
                if (form == null)
                {
                    form = new ScriptForm();
                    form.Show(System.Windows.Forms.NativeWindow.FromHandle(MMW.Window.WindowInfo.Handle));
                    form.TopLevel = true;

                    form.FormClosed += (s, e) =>
                    {
                        form = null;
                    };
                }
                else form.TopMost = true;
            }
        }

        protected override void OnReceivedMessage(string message, params object[] args)
        {
            if (message == "enable scripting")
            {
                Enabled = true;
            }
            else if (message == "disable scripting")
            {
                Enabled = false;
            }
            else if (message == "show dialog")
            {
                Enabled = false;
            }
            else if (message == "close dialog")
            {
                Enabled = true;
            }
        }
    }
}
