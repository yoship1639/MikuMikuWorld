using MikuMikuWorldScript;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.TestScripts
{
    class BigHead : GameObjectScript
    {
        public override string ScriptName => "Big Head";

        public override string ScriptDesc => "Big Head";

        public override void OnLoad(byte[] status)
        {
            base.OnLoad(status);
        }

        private double time = 0.0;
        bool flag = false;
        public override void OnUpdate(double deltaTime)
        {
            base.OnUpdate(deltaTime);

            time += deltaTime;
            if (time >= 1.0)
            {
                time -= 1.0;
                flag = !flag;
                //if (flag) PauseMotion();
                //else ResumeMotion();
            }
        }
    }
}
