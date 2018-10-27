//
// Miku Miku World License
//
// Copyright (c) 2017 Miku Miku World.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//

using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    /// <summary>
    /// 行列に関する処理のヘルパークラス
    /// </summary>
    public static class MatrixHelper
    {
        /// <summary>
        /// ZXY回転行列を作成する
        /// </summary>
        /// <param name="rot"></param>
        /// <returns></returns>
        public static Matrix4 CreateRotate(Vector3 rot)
        {
            return CreateRotate(rot.X, rot.Y, rot.Z);
        }

        /// <summary>
        /// ZXY回転行列を作成する
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Matrix4 CreateRotate(float x, float y, float z)
        {
            var cosx = (float)Math.Cos(x);
            var cosy = (float)Math.Cos(-y);
            var cosz = (float)Math.Cos(z);
            var sinx = (float)Math.Sin(x);
            var siny = (float)Math.Sin(-y);
            var sinz = (float)Math.Sin(z);
            return new Matrix4(
                cosz * cosy + sinx * siny * sinz,
                sinz * cosx,
                cosz * -siny + sinz * sinx * cosy,
                0.0f,
                -sinz * cosy + cosz * sinx * siny,
                cosz * cosx,
                -sinz * -siny + cosz * sinx * cosy,
                0.0f,
                cosx * siny,
                -sinx,
                cosx * cosy,
                0.0f,
                0.0f,
                0.0f,
                0.0f,
                1.0f
            );
            /*
            return new Matrix4(
                cosy * cosz - sinx * siny * sinz,
                -cosx * sinz,
                siny * cosz + sinx * cosy * sinz,
                0.0f,
                cosy * sinz + sinx * siny * cosz,
                cosx * cosz,
                sinz * siny - sinz * cosy * cosz,
                0.0f,
                -cosx * siny,
                sinx,
                cosx * cosy,
                0.0f,
                0.0f,
                0.0f,
                0.0f,
                1.0f
                );*/
        }

        public static Matrix4 CreateTransform(ref Vector3 pos, ref Quaternion rot)
        {
            Matrix4 m = Matrix4.Identity;
            if (rot != Quaternion.Identity) m = Matrix4.CreateFromQuaternion(rot);
            m.Row3 = new Vector4(pos.X, pos.Y, pos.Z, 1.0f);
            return m;
        }

        public static void CreateTransform(ref Vector3 pos, ref Quaternion rot, out Matrix4 m)
        {
            if (rot != Quaternion.Identity) m = Matrix4.CreateFromQuaternion(rot);
            else m = Matrix4.Identity;
            m.Row3 = new Vector4(pos.X, pos.Y, pos.Z, 1.0f);
        }

        public static Matrix4 CreateTransform(ref Vector3 pos, ref Quaternion rot, ref Vector3 scale)
        {
            Matrix4 m = Matrix4.Identity;
            if (rot != Quaternion.Identity) m = Matrix4.CreateFromQuaternion(rot);
            m.Row0 *= scale.X;
            m.Row1 *= scale.Y;
            m.Row2 *= scale.Z;
            m.Row3 = new Vector4(pos.X, pos.Y, pos.Z, 1.0f);
            return m;
        }
        public static void CreateTransform(ref Vector3 pos, ref Quaternion rot, ref Vector3 scale, out Matrix4 m)
        {
            m = Matrix4.Identity;
            if (rot != Quaternion.Identity) m = Matrix4.CreateFromQuaternion(rot);
            m.Row0 *= scale.X;
            m.Row1 *= scale.Y;
            m.Row2 *= scale.Z;
            m.Row3 = new Vector4(pos.X, pos.Y, pos.Z, 1.0f);
        }

        /// <summary>
        /// 姿勢行列を作成する
        /// </summary>
        public static Matrix4 CreateTransform(ref Vector3 pos, ref Vector3 rot, ref Vector3 scale)
        {
            Matrix4 m = Matrix4.Identity;
            if (rot != Vector3.Zero) m = CreateRotate(rot.X, rot.Y, rot.Z);

            m.Row0 *= scale.X;
            m.Row1 *= scale.Y;
            m.Row2 *= scale.Z;
            m.Row3 = new Vector4(pos.X, pos.Y, pos.Z, 1.0f);
            return m;
        }

        /// <summary>
        /// 姿勢行列を作成する
        /// </summary>
        public static void CreateTransform(ref Vector3 pos, ref Vector3 rot, ref Vector3 scale, out Matrix4 mat)
        {
            mat = Matrix4.Identity;
            if (rot != Vector3.Zero) mat = CreateRotate(rot.X, rot.Y, rot.Z);

            mat.Row0 *= scale.X;
            mat.Row1 *= scale.Y;
            mat.Row2 *= scale.Z;
            mat.Row3 = new Vector4(pos.X, pos.Y, pos.Z, 1.0f);
        }

        /// <summary>
        /// 姿勢行列を作成する
        /// </summary>
        public static Matrix4 CreateTransform(Vector3 pos, Vector3 rot, Vector3 scale)
        {
            Matrix4 m = Matrix4.Identity;
            if (rot != Vector3.Zero) m = CreateRotate(rot.X, rot.Y, rot.Z);

            m.Row0 *= scale.X;
            m.Row1 *= scale.Y;
            m.Row2 *= scale.Z;
            m.Row3 = new Vector4(pos.X, pos.Y, pos.Z, 1.0f);
            return m;
            /*
            var cosx = (float)Math.Cos(rot.X);
            var cosy = (float)Math.Cos(rot.Y);
            var cosz = (float)Math.Cos(rot.Z);
            var sinx = (float)Math.Sin(rot.X);
            var siny = (float)Math.Sin(rot.Y);
            var sinz = (float)Math.Sin(rot.Z);

            return new Matrix4(
                (cosy * cosz - sinx * siny * sinz) * scale.X,
                (-cosx * sinz) * scale.X,
                (siny * cosz + sinx * cosy * sinz) * scale.X,
                0.0f,
                (cosy * sinz + sinx * siny * cosz) * scale.Y,
                (cosx * cosz) * scale.Y,
                (sinz * siny - sinz * cosy * cosz) * scale.Y,
                0.0f,
                (-cosx * siny) * scale.Z,
                sinx * scale.Z,
                (cosx * cosy) * scale.Z,
                0.0f,
                pos.X,
                pos.Y,
                pos.Z,
                1.0f
                );
                */
        }

        public static Vector3 ExtractEulerRotation(this Matrix4 m)
        {
            if (m.M32 == 1.0f)
            {
                var x = MathHelper.PiOver2;
                var y = 0.0f;
                var z = (float)Math.Atan2(m.M21, m.M11);
                return new Vector3(x, -y, z);
            }
            else if (m.M32 == -1.0f)
            {
                var x = -MathHelper.PiOver2;
                var y = 0.0f;
                var z = (float)Math.Atan2(m.M21, m.M11);
                return new Vector3(x, -y, z);
            }
            else
            {
                var x = (float)Math.Asin(m.M32);
                var y = (float)Math.Atan2(-m.M31, m.M33);
                var z = (float)Math.Atan2(-m.M12, m.M22);
                return new Vector3(x, -y, z);
            }
        }

        public static Matrix4 CreateScreen(Vector2 size)
        {
            Matrix4 m = Matrix4.Identity;
            var w = size.X * 0.5f;
            var h = size.Y * 0.5f;
            m.M11 = w;
            m.M22 = -h;
            m.M41 = w;
            m.M42 = h;

            return m;
        }
    }
}
