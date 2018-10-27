using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Network
{
    public class NwMotion
    {
        public string Hash;
        public string Name { get; set; }
        public string Key;

        public Dictionary<string, NwBoneMotion> BoneMotion;
        public Dictionary<string, NwMorphMotion> MorphMotion;
    }

    public class NwBoneMotion
    {
        public string BoneName;
        public List<NwBoneMotionValue> Keys;
    }
    public class NwBoneMotionValue
    {
        public int FrameNo;

        public Vector3f Location;
        public Vector4f Rotation;
        public Vector3f Scale;

        public NwInterpolate Interpolate;
    }

    public class NwMorphMotion
    {
        public string MorphName;
        public List<NwMorphMotionValue> Keys;
    }
    public class NwMorphMotionValue
    {
        public int FrameNo;

        public float Rate;

        public NwInterpolate Interpolate;
    }

    public class NwInterpolate
    {
        public Vector2f P1;
        public Vector2f P2;
    }
}
