using MikuMikuWorldScript;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Object_MoveTest
{
    public class MoveTest : GameObjectScript
    {
        public override string ScriptName => "Move Test";
        public override string ScriptDesc => "移動のテストです.";

        public override void OnUpdate(double deltaTime)
        {
            Position += new Vector3((float)deltaTime * 0.5f, 0.0f, 0.0f);
            UpdatePhysicalTransform();
        }
    }
}
