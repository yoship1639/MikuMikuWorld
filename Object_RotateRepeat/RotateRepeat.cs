using MikuMikuWorldScript;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Object_RotateRepeat
{
    public class RotateRepeat : GameObjectScript
    {
        public override string ScriptName => "Rotate Repeat";
        public override string ScriptDesc => "常にY軸回転を続ける";

        public float RotateSpeed { get; set; } = MathHelper.Pi;

        public override void OnLoad(string status)
        {
            //var value = GetProiperty("rot_speed");
            //RotateSpeed = float.Parse(value);
        }

        public override void OnUpdate(double deltaTime)
        {
            Rotate += new Vector3(0.0f, (float)deltaTime * RotateSpeed, 0.0f);
        }
    }
}
