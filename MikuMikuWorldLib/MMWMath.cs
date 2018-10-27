using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    public static class MMWMath
    {
        public static int Clamp(int value, int min, int max)
        {
            if (value > max) return max;
            else if (value < min) return min;
            return value;
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value > max) return max;
            else if (value < min) return min;
            return value;
        }

        public static bool Clamped(int value, int min, int max)
        {
            return value >= min && value <= max;
        }

        public static float Saturate(float value)
        {
            return Clamp(value, 0.0f, 1.0f);
        }

        public static float Lerp(float from, float to, float rate)
        {
            return from + (to - from) * rate;
        }

        public static Color4 Lerp(Color4 from, Color4 to, float rate)
        {
            return new Color4(
                Lerp(from.R, to.R, rate),
                Lerp(from.G, to.G, rate),
                Lerp(from.B, to.B, rate),
                Lerp(from.A, to.A, rate));
        }

        public static void Lerp(ref Matrix4 m1, ref Matrix4 m2, float rate, out Matrix4 m)
        {
            m = new Matrix4(
                m1.M11 + (m2.M11 - m1.M11) * rate,
                m1.M12 + (m2.M12 - m1.M12) * rate,
                m1.M13 + (m2.M13 - m1.M13) * rate,
                m1.M14 + (m2.M14 - m1.M14) * rate,

                m1.M21 + (m2.M21 - m1.M21) * rate,
                m1.M22 + (m2.M22 - m1.M22) * rate,
                m1.M23 + (m2.M23 - m1.M23) * rate,
                m1.M24 + (m2.M24 - m1.M24) * rate,

                m1.M31 + (m2.M31 - m1.M31) * rate,
                m1.M32 + (m2.M32 - m1.M32) * rate,
                m1.M33 + (m2.M33 - m1.M33) * rate,
                m1.M34 + (m2.M34 - m1.M34) * rate,

                m1.M41 + (m2.M41 - m1.M41) * rate,
                m1.M42 + (m2.M42 - m1.M42) * rate,
                m1.M43 + (m2.M43 - m1.M43) * rate,
                m1.M44 + (m2.M44 - m1.M44) * rate
            );
        }

        public static int Repeat(int value, int min, int max)
        {
            if (min >= max) return min;
            while (value < min) value += max - min + 1;
            while (value > max) value -= max - min + 1;
            return value;
        }

        public static float Repeat(float value, float min, float max)
        {
            if (min >= max) return min;
            while (value < min) value += max - min;
            while (value >= max) value -= max - min;
            return value;
        }

        public static Vector3 Repeat(Vector3 value, Vector3 min, Vector3 max)
        {
            return new Vector3(
                Repeat(value.X, min.X, max.X),
                Repeat(value.Y, min.Y, max.Y),
                Repeat(value.Z, min.Z, max.Z));
        }

        public static float Smoothstep(float from, float to, float rate)
        {
            var t = Saturate((rate - from) / (to - from));
            return t * t * (3.0f - 2.0f * t);
        }

        public static Vector2 Smoothstep(Vector2 from, Vector2 to, float rate)
        {
            return new Vector2(
                Smoothstep(from.X, to.X, rate),
                Smoothstep(from.Y, to.Y, rate));
        }

        public static Vector3 Smoothstep(Vector3 from, Vector3 to, float rate)
        {
            return new Vector3(
                Smoothstep(from.X, to.X, rate),
                Smoothstep(from.Y, to.Y, rate),
                Smoothstep(from.Z, to.Z, rate));
        }

        public static float Approach(float from, float to, float value)
        {
            if (from < to)
            {
                from += value;
                if (from > to) return to;
            }
            else if (from > to)
            {
                from -= value;
                if (from < to) return to;
            }
            return from;
        }

        public static Vector3 ToEuler(this Quaternion q)
        {
            /*
            Vector3 v = new Vector3();
            // roll (x-axis rotation)
            var sinr = 2.0 * (q.W * q.X + q.Y * q.Z);
            var cosr = 1.0 - 2.0 * (q.X * q.X + q.Y * q.Y);
            v.X = (float)Math.Atan2(sinr, cosr);

            // pitch (y-axis rotation)
            double sinp = 2.0 * (q.W * q.Y - q.Z * q.X);
            if (Math.Abs(sinp) >= 1) v.Y = MathHelper.PiOver2; // use 90 degrees if out of range
            else v.Y = (float)Math.Asin(sinp);

            // yaw (z-axis rotation)
            double siny = 2.0 * (q.W * q.Z + q.X * q.Y);
            double cosy = 1.0 - 2.0 * (q.Y * q.Y + q.Z * q.Z);
            v.Z = (float)Math.Atan2(siny, cosy);

            return v;
            */
            return MatrixHelper.ExtractEulerRotation(Matrix4.CreateFromQuaternion(q)) * -1.0f;
        }

        public static Quaternion ToQuaternion(this Vector3 v)
        {
            /*
            Quaternion q = new Quaternion();
            // Abbreviations for the various angular functions
            double cy = Math.Cos(v.Z * 0.5);
            double sy = Math.Sin(v.Z * 0.5);
            double cr = Math.Cos(v.X * 0.5);
            double sr = Math.Sin(v.X * 0.5);
            double cp = Math.Cos(v.Y * 0.5);
            double sp = Math.Sin(v.Y * 0.5);

            q.W = (float)(cy * cr * cp + sy * sr * sp);
            q.X = (float)(cy * sr * cp - sy * cr * sp);
            q.Y = (float)(cy * cr * sp + sy * sr * cp);
            q.Z = (float)(sy * cr * cp - cy * sr * sp);
            return q;
            */
            return MatrixHelper.CreateRotate(v).ExtractRotation();
        }
    }
}
