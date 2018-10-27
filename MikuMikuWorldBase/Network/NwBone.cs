using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Network
{
    public class NwBone
    {
        public string Name;
        public int Index;
        public Vector3f Position;
        public int Parent = -1;
        public string Type;

        public NwIKLink[] IKLinks { get; set; }
        public int IKTarget { get; set; } = -1;
        public int IKLoop { get; set; }
        public float IKRotLimit { get; set; }
    }

    public class NwIKLink
    {
        public int Bone = -1;
        public bool LimitAngle;
        public Vector3f UpperLimitAngle;
        public Vector3f LowerLimitAngle;
    }
}
