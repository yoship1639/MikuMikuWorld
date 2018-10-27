using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Assets
{
    [DataContract]
    public class Bone : IAsset
    {
        public bool Loaded => true;
        public Result Load() => Result.Success;
        public Result Unload() => Result.Success;

        [DataMember(Name = "name", EmitDefaultValue = false, Order = 0)]
        public string Name { get; set; }

        [DataMember(Name = "index", Order = 1)]
        public int Index { get; set; }

        [DataMember(Name = "type", EmitDefaultValue = false, Order = 2)]
        public string BoneType { get; set; }

        public Vector3 Position { get; set; }
        public Bone Parent { get; set; }

        [DataMember(Name = "children", EmitDefaultValue = false, Order = 4)]
        public Bone[] Children { get; set; }

        public IKLink[] IKLinks { get; set; }
        public Bone IKTarget { get; set; }
        public int IKLoop { get; set; }
        public float IKRotLimit { get; set; }



        [DataMember(Order = 3)]
        private float[] position
        {
            get { return Position.ToFloats(); }
            set { Position = value.ToVector3(); }
        }

        [DataMember(EmitDefaultValue = false, Order = 5)]
        internal int[] iklinks { get; set; }

        public bool Invisible { get; set; }

        public override string ToString()
        {
            return Name + ": " + (Children != null ? Children.Length.ToString() : "0");
        }
    }

    public class IKLink
    {
        public Bone Bone;
        public bool LimitAngle;
        public Vector3 UpperLimitAngle;
        public Vector3 LowerLimitAngle;
    }

    public enum BoneType
    {
        Standard,
        IK,
        UnderIK,
        IKConnect,
    }
}
