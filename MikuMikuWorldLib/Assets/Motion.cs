using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Assets
{
    public class Motion : IAsset
    {
        public bool Loaded { get; protected set; }
        public string Name { get; set; }
        public Result Load() => Result.Success;
        public Result Unload() => Result.Success;

        public int FrameNoMax;
        public Dictionary<string, BoneMotion> BoneMotions = new Dictionary<string, BoneMotion>();
        public Dictionary<string, SkinMotion> SkinMotions = new Dictionary<string, SkinMotion>();
    }

    public class KeyFrame<T>
    {
        public int FrameNo;
        public T Value;
        public IInterpolate Interpolate;

        public override string ToString()
        {
            return $"{FrameNo}";
        }
    }

    public class BoneMotion
    {
        public string BoneName;
        public List<KeyFrame<BoneMotionValue>> Keys;

        public override string ToString()
        {
            return $"{BoneName}: {Keys.Count}";
        }
    }
    public struct BoneMotionValue
    {
        public Vector3 location;
        public Quaternion rotation;
        public Vector3 scale;

        public static BoneMotionValue operator *(BoneMotionValue left, float scale)
        {
            var rot = left.rotation;
            rot.W /= scale;
            rot.Normalize();
            return new BoneMotionValue()
            {
                location = left.location * scale,
                rotation = rot,
                scale = Vector3.Lerp(Vector3.One, left.scale, scale),
            };
        }
        public static BoneMotionValue operator +(BoneMotionValue left, BoneMotionValue right)
        {
            return new BoneMotionValue()
            {
                location = left.location + right.location,
                rotation = left.rotation * right.rotation,
                scale = left.scale + right.scale,
            };
        }
        public static bool operator !=(BoneMotionValue left, BoneMotionValue right)
        {
            return !(left == right);
        }
        public static bool operator ==(BoneMotionValue left, BoneMotionValue right)
        {
            return left.rotation == right.rotation && left.location == right.location && left.scale == right.scale;
        }

        public override bool Equals(object obj)
        {
            return this == (BoneMotionValue)obj;
        }
        public override int GetHashCode()
        {
            return location.GetHashCode() + rotation.GetHashCode() + scale.GetHashCode();
        }

        public static void Lerp(ref BoneMotionValue a, ref BoneMotionValue b, float rate, out BoneMotionValue bmv)
        {
            Vector3.Lerp(ref a.location, ref b.location, rate, out bmv.location);
            bmv.rotation = Quaternion.Slerp(a.rotation, b.rotation, rate);
            Vector3.Lerp(ref a.scale, ref b.scale, rate, out bmv.scale);
        }

        public Matrix4 CreateTransform()
        {
            return MatrixHelper.CreateTransform(ref location, ref rotation, ref scale);
        }

        public void Init()
        {
            location = Vector3.Zero;
            rotation = Quaternion.Identity;
            scale = Vector3.One;
        }

        public static readonly BoneMotionValue Identity = new BoneMotionValue()
        {
            location = Vector3.Zero,
            rotation = Quaternion.Identity,
            scale = Vector3.One,
        };
    }

    public class SkinMotion
    {
        public string MorphName;
        public List<KeyFrame<float>> Keys;
    }
}
