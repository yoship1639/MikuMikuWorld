using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    public static class Extensions
    {
        public static float[] ToFloats(this Vector2 v, bool nullIfZero = false)
        {
            if (nullIfZero && v == Vector2.Zero) return null;
            return new float[] { v.X, v.Y };
        }
        public static float[] ToFloats(this Vector3 v, bool nullIfZero = false)
        {
            if (nullIfZero && v == Vector3.Zero) return null;
            return new float[] { v.X, v.Y, v.Z };
        }
        public static float[] ToFloats(this Vector4 v, bool nullIfZero = false)
        {
            if (nullIfZero && v == Vector4.Zero) return null;
            return new float[] { v.X, v.Y, v.Z, v.W };
        }
        public static float[] ToFloats(this Color4 c)
        {
            return new float[] { c.R, c.G, c.B, c.A };
        }
        public static float[] ToFloats(this Matrix4 m)
        {
            return new float[]
            {
                m.M11, m.M12, m.M13, m.M14,
                m.M21, m.M22, m.M23, m.M24,
                m.M31, m.M32, m.M33, m.M34,
                m.M41, m.M42, m.M43, m.M44,
            };
        }

        public static Vector2 ToVector2(this float[] f)
        {
            return new Vector2(f[0], f[1]);
        }
        public static Vector3 ToVector3(this float[] f)
        {
            return new Vector3(f[0], f[1], f[2]);
        }
        public static Vector4 ToVector4(this float[] f)
        {
            return new Vector4(f[0], f[1], f[2], f[3]);
        }
        public static Color4 ToColor4(this float[] f)
        {
            return new Color4(f[0], f[1], f[2], f[3]);
        }
        public static Matrix4 ToMatrix4(this float[] f)
        {
            return new Matrix4(f[0], f[1], f[2], f[3], f[4], f[5], f[6], f[7], f[8], f[9], f[10], f[11], f[12], f[13], f[14], f[15]);
        }
        public static Vector4 ToVector4(this RectangleF rect)
        {
            return new Vector4(rect.X, rect.Y, rect.Width, rect.Height);
        }
        public static Vector4 ToVector4(this Color4 color)
        {
            return new Vector4(color.R, color.G, color.B, color.A);
        }

        #region Binary

        public static byte[] ToBytes(this Vector2 v)
        {
            var buf = new List<byte>();
            buf.AddRange(BitConverter.GetBytes(v.X));
            buf.AddRange(BitConverter.GetBytes(v.Y));
            return buf.ToArray();
        }
        public static Vector2 ToVector2(this byte[] buf)
        {
            var v = new Vector2();
            v.X = BitConverter.ToSingle(buf, 0);
            v.Y = BitConverter.ToSingle(buf, 4);
            return v;
        }

        public static byte[] ToBytes(this Vector3 v)
        {
            var buf = new List<byte>();
            buf.AddRange(BitConverter.GetBytes(v.X));
            buf.AddRange(BitConverter.GetBytes(v.Y));
            buf.AddRange(BitConverter.GetBytes(v.Z));
            return buf.ToArray();
        }
        public static Vector3 ToVector3(this byte[] buf)
        {
            var v = new Vector3();
            v.X = BitConverter.ToSingle(buf, 0);
            v.Y = BitConverter.ToSingle(buf, 4);
            v.Z = BitConverter.ToSingle(buf, 8);
            return v;
        }

        public static byte[] ToBytes(this Vector4 v)
        {
            var buf = new List<byte>();
            buf.AddRange(BitConverter.GetBytes(v.X));
            buf.AddRange(BitConverter.GetBytes(v.Y));
            buf.AddRange(BitConverter.GetBytes(v.Z));
            buf.AddRange(BitConverter.GetBytes(v.W));
            return buf.ToArray();
        }
        public static Vector4 ToVector4(this byte[] buf)
        {
            var v = new Vector4();
            v.X = BitConverter.ToSingle(buf, 0);
            v.Y = BitConverter.ToSingle(buf, 4);
            v.Z = BitConverter.ToSingle(buf, 8);
            v.Z = BitConverter.ToSingle(buf, 12);
            return v;
        }

        public static byte[] ToBytes(this Color4 c)
        {
            var buf = new List<byte>();
            buf.AddRange(BitConverter.GetBytes(c.R));
            buf.AddRange(BitConverter.GetBytes(c.G));
            buf.AddRange(BitConverter.GetBytes(c.B));
            buf.AddRange(BitConverter.GetBytes(c.A));
            return buf.ToArray();
        }
        public static Color4 ToColor4(this byte[] buf)
        {
            var c = new Color4();
            c.R = BitConverter.ToSingle(buf, 0);
            c.G = BitConverter.ToSingle(buf, 4);
            c.B = BitConverter.ToSingle(buf, 8);
            c.A = BitConverter.ToSingle(buf, 12);
            return c;
        }

        public static byte[] ToBytes(this Matrix4 m)
        {
            var buf = new List<byte>();
            var fs = m.ToFloats();
            foreach (var f in fs) buf.AddRange(BitConverter.GetBytes(f));
            return buf.ToArray();
        }
        public static Matrix4 ToMatrix4(this byte[] buf)
        {
            var m = new Matrix4();
            m.M11 = BitConverter.ToSingle(buf, (4 * 0));
            m.M12 = BitConverter.ToSingle(buf, (4 * 1));
            m.M13 = BitConverter.ToSingle(buf, (4 * 2));
            m.M14 = BitConverter.ToSingle(buf, (4 * 3));

            m.M21 = BitConverter.ToSingle(buf, (4 * 4));
            m.M22 = BitConverter.ToSingle(buf, (4 * 5));
            m.M23 = BitConverter.ToSingle(buf, (4 * 6));
            m.M24 = BitConverter.ToSingle(buf, (4 * 7));

            m.M31 = BitConverter.ToSingle(buf, (4 * 8));
            m.M32 = BitConverter.ToSingle(buf, (4 * 9));
            m.M33 = BitConverter.ToSingle(buf, (4 * 10));
            m.M34 = BitConverter.ToSingle(buf, (4 * 11));

            m.M41 = BitConverter.ToSingle(buf, (4 * 12));
            m.M42 = BitConverter.ToSingle(buf, (4 * 13));
            m.M43 = BitConverter.ToSingle(buf, (4 * 14));
            m.M44 = BitConverter.ToSingle(buf, (4 * 15));

            return m;
        }

        public static byte[] ToBytes(this int[] ints)
        {
            if (ints == null) return null;
            var buf = new List<byte>();
            foreach (var i in ints) buf.AddRange(BitConverter.GetBytes(i));
            return buf.ToArray();
        }
        public static int[] ToInts(this byte[] buf)
        {
            var ints = new int[buf.Length / 4];
            for (var i = 0; i < ints.Length; i++)
            {
                ints[i] = BitConverter.ToInt32(buf, i * 4);
            }
            return ints;
        }

        public static byte[] ToBytes(this float[] fs)
        {
            if (fs == null) return null;
            var buf = new List<byte>();
            foreach (var f in fs) buf.AddRange(BitConverter.GetBytes(f));
            return buf.ToArray();
        }
        public static float[] ToFloats(this byte[] buf)
        {
            var ints = new float[buf.Length / 4];
            for (var i = 0; i < ints.Length; i++)
            {
                ints[i] = BitConverter.ToSingle(buf, i * 4);
            }
            return ints;
        }

        public static byte[] ToBytes(this Vector2[] vs)
        {
            var buf = new List<byte>();
            foreach (var v in vs)
            {
                buf.AddRange(BitConverter.GetBytes(v.X));
                buf.AddRange(BitConverter.GetBytes(v.Y));
            }
            return buf.ToArray();
        }
        public static Vector2[] ToVector2Array(this byte[] buf)
        {
            var vs = new Vector2[buf.Length / (4 * 2)];
            for (var i = 0; i < vs.Length; i++)
            {
                vs[i].X = BitConverter.ToSingle(buf, (i * 4 * 2) + 0);
                vs[i].Y = BitConverter.ToSingle(buf, (i * 4 * 2) + 4);
            }
            return vs;
        }

        public static byte[] ToBytes(this Vector3[] vs)
        {
            var buf = new List<byte>();
            foreach (var v in vs)
            {
                buf.AddRange(BitConverter.GetBytes(v.X));
                buf.AddRange(BitConverter.GetBytes(v.Y));
                buf.AddRange(BitConverter.GetBytes(v.Z));
            }
            return buf.ToArray();
        }
        public static Vector3[] ToVector3Array(this byte[] buf)
        {
            var vs = new Vector3[buf.Length / (4 * 3)];
            for (var i = 0; i < vs.Length; i++)
            {
                vs[i].X = BitConverter.ToSingle(buf, (i * 4 * 3) + 0);
                vs[i].Y = BitConverter.ToSingle(buf, (i * 4 * 3) + 4);
                vs[i].Z = BitConverter.ToSingle(buf, (i * 4 * 3) + 8);
            }
            return vs;
        }

        public static byte[] ToBytes(this Vector4[] vs)
        {
            var buf = new List<byte>();
            foreach (var v in vs)
            {
                buf.AddRange(BitConverter.GetBytes(v.X));
                buf.AddRange(BitConverter.GetBytes(v.Y));
                buf.AddRange(BitConverter.GetBytes(v.Z));
                buf.AddRange(BitConverter.GetBytes(v.W));
            }
            return buf.ToArray();
        }
        public static Vector4[] ToVector4Array(this byte[] buf)
        {
            var vs = new Vector4[buf.Length / (4 * 4)];
            for (var i = 0; i < vs.Length; i++)
            {
                vs[i].X = BitConverter.ToSingle(buf, (i * 4 * 4) + 0);
                vs[i].Y = BitConverter.ToSingle(buf, (i * 4 * 4) + 4);
                vs[i].Z = BitConverter.ToSingle(buf, (i * 4 * 4) + 8);
                vs[i].W = BitConverter.ToSingle(buf, (i * 4 * 4) + 12);
            }
            return vs;
        }

        public static byte[] ToBytes(this Color4[] cs)
        {
            var buf = new List<byte>();
            foreach (var c in cs)
            {
                buf.AddRange(BitConverter.GetBytes(c.R));
                buf.AddRange(BitConverter.GetBytes(c.G));
                buf.AddRange(BitConverter.GetBytes(c.B));
                buf.AddRange(BitConverter.GetBytes(c.A));
            }
            return buf.ToArray();
        }
        public static Color4[] ToColor4Array(this byte[] buf)
        {
            var cs = new Color4[buf.Length / (4 * 4)];
            for (var i = 0; i < cs.Length; i++)
            {
                cs[i].R = BitConverter.ToSingle(buf, (i * 4 * 4) + 0);
                cs[i].G = BitConverter.ToSingle(buf, (i * 4 * 4) + 4);
                cs[i].B = BitConverter.ToSingle(buf, (i * 4 * 4) + 8);
                cs[i].A = BitConverter.ToSingle(buf, (i * 4 * 4) + 12);
            }
            return cs;
        }

        public static byte[] ToBytes(this Matrix4[] ms)
        {
            var buf = new List<byte>();
            foreach (var m in ms)
            {
                var fs = m.ToFloats();
                foreach (var f in fs) buf.AddRange(BitConverter.GetBytes(f));
            }
            return buf.ToArray();
        }
        public static Matrix4[] ToMatrix4Array(this byte[] buf)
        {
            var ms = new Matrix4[buf.Length / (4 * 16)];
            for (var i = 0; i < ms.Length; i++)
            {
                var ii = i * 4 * 16;
                ms[i].M11 = BitConverter.ToSingle(buf, ii + (4 * 0));
                ms[i].M12 = BitConverter.ToSingle(buf, ii + (4 * 1));
                ms[i].M13 = BitConverter.ToSingle(buf, ii + (4 * 2));
                ms[i].M14 = BitConverter.ToSingle(buf, ii + (4 * 3));

                ms[i].M21 = BitConverter.ToSingle(buf, ii + (4 * 4));
                ms[i].M22 = BitConverter.ToSingle(buf, ii + (4 * 5));
                ms[i].M23 = BitConverter.ToSingle(buf, ii + (4 * 6));
                ms[i].M24 = BitConverter.ToSingle(buf, ii + (4 * 7));

                ms[i].M31 = BitConverter.ToSingle(buf, ii + (4 * 8));
                ms[i].M32 = BitConverter.ToSingle(buf, ii + (4 * 9));
                ms[i].M33 = BitConverter.ToSingle(buf, ii + (4 * 10));
                ms[i].M34 = BitConverter.ToSingle(buf, ii + (4 * 11));

                ms[i].M41 = BitConverter.ToSingle(buf, ii + (4 * 12));
                ms[i].M42 = BitConverter.ToSingle(buf, ii + (4 * 13));
                ms[i].M43 = BitConverter.ToSingle(buf, ii + (4 * 14));
                ms[i].M44 = BitConverter.ToSingle(buf, ii + (4 * 15));
            }
            return ms;
        }

        #endregion

        public static string ToBase64String(this int[] ints) { return Convert.ToBase64String(ToBytes(ints)); }
        public static string ToBase64String(this float[] fs) { return Convert.ToBase64String(ToBytes(fs)); }
        public static string ToBase64String(this Vector2[] vs) { return Convert.ToBase64String(ToBytes(vs)); }
        public static string ToBase64String(this Vector3[] vs) { return Convert.ToBase64String(ToBytes(vs)); }
        public static string ToBase64String(this Vector4[] vs) { return Convert.ToBase64String(ToBytes(vs)); }
        public static string ToBase64String(this Color4[] cs) { return Convert.ToBase64String(ToBytes(cs)); }
        public static string ToBase64String(this Matrix4[] ms) { return Convert.ToBase64String(ToBytes(ms)); }

        public static int[] ToInts(this string s) { return ToInts(Convert.FromBase64String(s)); }
        public static float[] ToFloats(this string s) { return ToFloats(Convert.FromBase64String(s)); }
        public static Vector2[] ToVector2Array(this string s) { return ToVector2Array(Convert.FromBase64String(s)); }
        public static Vector3[] ToVector3Array(this string s) { return ToVector3Array(Convert.FromBase64String(s)); }
        public static Vector4[] ToVector4Array(this string s) { return ToVector4Array(Convert.FromBase64String(s)); }
        public static Color4[] ToColor4Array(this string s) { return ToColor4Array(Convert.FromBase64String(s)); }
        public static Matrix4[] ToMatrix4Array(this string s) { return ToMatrix4Array(Convert.FromBase64String(s)); }
        

        public static Vector2 ToVector2(this Size size)
        {
            return new Vector2(size.Width, size.Height);
        }

        public static double ToRadian(this double deg)
        {
            return MathHelper.DegreesToRadians(deg);
        }

        public static double ToDegree(this double rad)
        {
            return MathHelper.RadiansToDegrees(rad);
        }

        public static Vector2 Inverse(this Vector2 v)
        {
            return new Vector2(1.0f / v.X, 1.0f / v.Y);
        }

        public static Vector3 Inverse(this Vector3 v)
        {
            return new Vector3(1.0f / v.X, 1.0f / v.Y, 1.0f / v.Z);
        }

        public static Vector4 Inverse(this Vector4 v)
        {
            return new Vector4(1.0f / v.X, 1.0f / v.Y, 1.0f / v.Z, 1.0f / v.W);
        }

        public static T[] Init<T>(this T[] array, T value)
        {
            for (var i = 0; i < array.Length; i++) array[i] = value;
            return array;
        }

        public static Size Mul(this Size size, float s)
        {
            return new Size((int)(size.Width * s), (int)(size.Height * s));
        }

        public static T2 FindValue<T1, T2>(this Dictionary<T1, T2> dic, Predicate<T2> p)
        {
            foreach (var v in dic.Values) if (p(v)) return v;
            return default(T2);
        }
    }
}
