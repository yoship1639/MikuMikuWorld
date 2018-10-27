using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    internal unsafe static class PhysicsExtensions
    {
        /*
        #region APIs
        [DllImport("Kernel32.dll", EntryPoint ="RtlMoveMemory")]
        private static extern void CopyMemory(IntPtr Destination, IntPtr Source, [MarshalAs(UnmanagedType.U4)] int Length);
        #endregion

        public static OpenTK.Vector3 ToVector3(this BulletSharp.Math.Vector3 v)
        {
            return new OpenTK.Vector3(v.X, v.Y, v.Z);
        }

        public static BulletSharp.Math.Vector3 ToVector3(this OpenTK.Vector3 v)
        {
            return new BulletSharp.Math.Vector3(v.X, v.Y, v.Z);
        }

        public static OpenTK.Vector4 ToVector4(this BulletSharp.Math.Vector4 v)
        {
            return new OpenTK.Vector4(v.X, v.Y, v.Z, v.W);
        }

        public static BulletSharp.Math.Vector4 ToVector4(this OpenTK.Vector4 v)
        {
            return new BulletSharp.Math.Vector4(v.X, v.Y, v.Z, v.W);
        }

        public static OpenTK.Quaternion ToQuaternion(this BulletSharp.Math.Quaternion q)
        {
            return new OpenTK.Quaternion(q.X, q.Y, q.Z, q.W);
        }

        public static BulletSharp.Math.Quaternion ToQuaternion(this OpenTK.Quaternion q)
        {
            return new BulletSharp.Math.Quaternion(q.X, q.Y, q.Z, q.W);
        }

        private static OpenTK.Matrix4 mat4 = OpenTK.Matrix4.Identity;
        public static OpenTK.Matrix4 ToMatrix(this BulletSharp.Math.Matrix m)
        {
            mat4.Row0.X = m.M11;
            mat4.Row0.Y = m.M12;
            mat4.Row0.Z = m.M13;
            mat4.Row0.W = m.M14;
            mat4.Row1.X = m.M21;
            mat4.Row1.Y = m.M22;
            mat4.Row1.Z = m.M23;
            mat4.Row1.W = m.M24;
            mat4.Row2.X = m.M31;
            mat4.Row2.Y = m.M32;
            mat4.Row2.Z = m.M33;
            mat4.Row2.W = m.M34;
            mat4.Row3.X = m.M41;
            mat4.Row3.Y = m.M42;
            mat4.Row3.Z = m.M43;
            mat4.Row3.W = m.M44;
            return mat4;
        }

        private static BulletSharp.Math.Matrix mat = BulletSharp.Math.Matrix.Identity;
        public unsafe static BulletSharp.Math.Matrix ToMatrix(this OpenTK.Matrix4 m)
        {
            mat.M11 = m.Row0.X;
            mat.M12 = m.Row0.Y;
            mat.M13 = m.Row0.Z;
            mat.M14 = m.Row0.W;
            mat.M21 = m.Row1.X;
            mat.M22 = m.Row1.Y;
            mat.M23 = m.Row1.Z;
            mat.M24 = m.Row1.W;
            mat.M31 = m.Row2.X;
            mat.M32 = m.Row2.Y;
            mat.M33 = m.Row2.Z;
            mat.M34 = m.Row2.W;
            mat.M41 = m.Row3.X;
            mat.M42 = m.Row3.Y;
            mat.M43 = m.Row3.Z;
            mat.M44 = m.Row3.W;
            return mat;
        }*/

        /*
        public static void Set(this BulletSharp.Math.Vector3 v, OpenTK.Vector3 vec)
        {
            v.X = vec.X;
            v.Y = vec.Y;
            v.Z = vec.Z;
        }
        public static void Set(this BulletSharp.Math.Vector3 v, ref OpenTK.Vector3 vec)
        {
            v.X = vec.X;
            v.Y = vec.Y;
            v.Z = vec.Z;
        }
        public static void Set(this OpenTK.Vector3 v, BulletSharp.Math.Vector3 vec)
        {
            v.X = vec.X;
            v.Y = vec.Y;
            v.Z = vec.Z;
        }
        public static void Set(this OpenTK.Vector3 v, ref BulletSharp.Math.Vector3 vec)
        {
            v.X = vec.X;
            v.Y = vec.Y;
            v.Z = vec.Z;
        }

        public static void Set(this BulletSharp.Math.Vector4 v, OpenTK.Vector4 vec)
        {
            v.X = vec.X;
            v.Y = vec.Y;
            v.Z = vec.Z;
            v.W = vec.W;
        }
        public static void Set(this BulletSharp.Math.Vector4 v, ref OpenTK.Vector4 vec)
        {
            v.X = vec.X;
            v.Y = vec.Y;
            v.Z = vec.Z;
            v.W = vec.W;
        }
        public static void Set(this OpenTK.Vector4 v, BulletSharp.Math.Vector4 vec)
        {
            v.X = vec.X;
            v.Y = vec.Y;
            v.Z = vec.Z;
            v.W = vec.W;
        }
        public static void Set(this OpenTK.Vector4 v, ref BulletSharp.Math.Vector4 vec)
        {
            v.X = vec.X;
            v.Y = vec.Y;
            v.Z = vec.Z;
            v.W = vec.W;
        }

        public static void Set(this BulletSharp.Math.Matrix m, OpenTK.Matrix4 mat)
        {
            m.M11 = mat.Row0.X;
            m.M12 = mat.Row0.Y;
            m.M13 = mat.Row0.Z;
            m.M14 = mat.Row0.W;
            m.M21 = mat.Row1.X;
            m.M22 = mat.Row1.Y;
            m.M23 = mat.Row1.Z;
            m.M24 = mat.Row1.W;
            m.M31 = mat.Row2.X;
            m.M32 = mat.Row2.Y;
            m.M33 = mat.Row2.Z;
            m.M34 = mat.Row2.W;
            m.M41 = mat.Row3.X;
            m.M42 = mat.Row3.Y;
            m.M43 = mat.Row3.Z;
            m.M44 = mat.Row3.W;
        }
        public static void Set(this BulletSharp.Math.Matrix m, ref OpenTK.Matrix4 mat)
        {
            m.M11 = mat.Row0.X;
            m.M12 = mat.Row0.Y;
            m.M13 = mat.Row0.Z;
            m.M14 = mat.Row0.W;
            m.M21 = mat.Row1.X;
            m.M22 = mat.Row1.Y;
            m.M23 = mat.Row1.Z;
            m.M24 = mat.Row1.W;
            m.M31 = mat.Row2.X;
            m.M32 = mat.Row2.Y;
            m.M33 = mat.Row2.Z;
            m.M34 = mat.Row2.W;
            m.M41 = mat.Row3.X;
            m.M42 = mat.Row3.Y;
            m.M43 = mat.Row3.Z;
            m.M44 = mat.Row3.W;
        }
        public static void Set(this OpenTK.Matrix4 m, BulletSharp.Math.Matrix mat)
        {
            m.Row0.X = mat.M11;
            m.Row0.Y = mat.M12;
            m.Row0.Z = mat.M13;
            m.Row0.W = mat.M14;
            m.Row1.X = mat.M21;
            m.Row1.Y = mat.M22;
            m.Row1.Z = mat.M23;
            m.Row1.W = mat.M24;
            m.Row2.X = mat.M31;
            m.Row2.Y = mat.M32;
            m.Row2.Z = mat.M33;
            m.Row2.W = mat.M34;
            m.Row3.X = mat.M41;
            m.Row3.Y = mat.M42;
            m.Row3.Z = mat.M43;
            m.Row3.W = mat.M44;
        }
        public static void Set(this OpenTK.Matrix4 m, ref BulletSharp.Math.Matrix mat)
        {
            m.Row0.X = mat.M11;
            m.Row0.Y = mat.M12;
            m.Row0.Z = mat.M13;
            m.Row0.W = mat.M14;
            m.Row1.X = mat.M21;
            m.Row1.Y = mat.M22;
            m.Row1.Z = mat.M23;
            m.Row1.W = mat.M24;
            m.Row2.X = mat.M31;
            m.Row2.Y = mat.M32;
            m.Row2.Z = mat.M33;
            m.Row2.W = mat.M34;
            m.Row3.X = mat.M41;
            m.Row3.Y = mat.M42;
            m.Row3.Z = mat.M43;
            m.Row3.W = mat.M44;
        }*/
    }
}
