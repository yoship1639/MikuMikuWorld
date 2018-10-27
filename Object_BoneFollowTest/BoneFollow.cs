using MikuMikuWorldScript;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Object_BoneFollowTest
{
    public class BoneFollow : GameObjectScript
    {
        public override string ScriptName => "Bone Follow";
        public override string ScriptDesc => "ボーン追従テスト";

        private double time = 2.0;
        public override void OnUpdate(double deltaTime)
        {
            LocalTransform = ParentScript.GetBoneLocalTransform("頭");

            time -= deltaTime;
            if (time < 0.0)
            {
                time += 2.0;

                UpdateStatus(new byte[1]);
            }
        }

        public override void OnReceivedUpdateStatus(byte[] status)
        {
            Log("update");
        }
    }
}
