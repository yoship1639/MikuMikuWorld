using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    [DataContract]
    public class Vector2f
    {
        [DataMember]
        public float X;
        [DataMember]
        public float Y;

        public Vector2f() { }
        public Vector2f(float x, float y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return string.Format("({0:0.000}, {1:0.000})", X, Y);
        }
    }

    [DataContract]
    public class Vector3f
    {
        [DataMember]
        public float X;
        [DataMember]
        public float Y;
        [DataMember]
        public float Z;

        public Vector3f() { }
        public Vector3f(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {
            return string.Format("({0:0.000}, {1:0.000}, {2:0.000})", X, Y, Z);
        }
    }

    [DataContract]
    public class Vector4f
    {
        [DataMember]
        public float X;
        [DataMember]
        public float Y;
        [DataMember]
        public float Z;
        [DataMember]
        public float W;

        public Vector4f() { }
        public Vector4f(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public override string ToString()
        {
            return string.Format("({0:0.000}, {1:0.000}, {2:0.000}, {3:0.000})", X, Y, Z, W);
        }
    }

    [DataContract]
    public class Color4f
    {
        [DataMember]
        public float R;
        [DataMember]
        public float G;
        [DataMember]
        public float B;
        [DataMember]
        public float A;

        public Color4f() { }
        public Color4f(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2}, {3})", (int)(R * 255), (int)(G * 255), (int)(B * 255), (int)(A * 255));
        }
    }

    public class BoneWeight4f
    {
        public int Index0;
        public int Index1;
        public int Index2;
        public int Index3;

        public float Weight0;
        public float Weight1;
        public float Weight2;
        public float Weight3;
    }

    [DataContract]
    public class Matrix4f
    {
        [DataMember]
        public float M00;
        [DataMember]
        public float M01;
        [DataMember]
        public float M02;
        [DataMember]
        public float M03;

        [DataMember]
        public float M10;
        [DataMember]
        public float M11;
        [DataMember]
        public float M12;
        [DataMember]
        public float M13;

        [DataMember]
        public float M20;
        [DataMember]
        public float M21;
        [DataMember]
        public float M22;
        [DataMember]
        public float M23;

        [DataMember]
        public float M30;
        [DataMember]
        public float M31;
        [DataMember]
        public float M32;
        [DataMember]
        public float M33;
    }
}
