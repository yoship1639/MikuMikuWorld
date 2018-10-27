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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace MikuMikuWorld.GameComponents
{
    /// <summary>
    /// 位置・回転・拡大率の姿勢情報を保持するコンポーネント
    /// </summary>
    public class Transform : GameComponent
    {
        public Transform()
        {
            getter.Add("Position", (obj) => Position);
            getter.Add("Rotate", (obj) => Rotate);
            getter.Add("Scale", (obj) => Scale);
            getter.Add("Parent", (obj) => Parent);
            getter.Add("WorldPosition", (obj) => WorldPosition);
            getter.Add("WorldRotate", (obj) => WorldRotate);
            getter.Add("WorldScale", (obj) => WorldScale);
            getter.Add("LocalTransform", (obj) => LocalTransform);
            getter.Add("WorldTransform", (obj) => WorldTransform);
            getter.Add("LocalDirectionX", (obj) => LocalDirectionX);
            getter.Add("LocalDirectionY", (obj) => LocalDirectionY);
            getter.Add("LocalDirectionZ", (obj) => LocalDirectionZ);

            setter.Add("Position", (obj, value) => Position = (Vector3)value);
            setter.Add("Rotate", (obj, value) => Rotate = (Vector3)value);
            setter.Add("Scale", (obj, value) => Scale = (Vector3)value);
            setter.Add("Parent", (obj, value) => Parent = (Transform)value);
            setter.Add("LocalTransform", (obj, value) => LocalTransform = (Matrix4)value);
            setter.Add("WorldTransform", (obj, value) => WorldTransform = (Matrix4)value);

            execs.Add("UpdatePhysicalTransform", (gc, args) => { UpdatePhysicalTransform(); return null; });
        }

        public override bool ComponentDupulication => false;

        /// <summary>
        /// 位置
        /// </summary>
        public Vector3 Position = Vector3.Zero;

        /// <summary>
        /// 回転
        /// </summary>
        public Vector3 Rotate = Vector3.Zero;

        /// <summary>
        /// 拡大率
        /// </summary>
        public Vector3 Scale = Vector3.One;

        /// <summary>
        /// 親の姿勢情報
        /// </summary>
        public Transform Parent { get; set; }

        /// <summary>
        /// ワールド座標の位置
        /// </summary>
        public Vector3 WorldPosition
        {
            get
            {
                return WorldTransform.ExtractTranslation();
            }
        }

        /// <summary>
        /// ワールド座標の回転
        /// </summary>
        public Vector3 WorldRotate
        {
            get
            {
                return WorldTransform.ExtractEulerRotation();
            }
        }

        /// <summary>
        /// ワールド座標の拡大率
        /// </summary>
        public Vector3 WorldScale
        {
            get
            {
                return WorldTransform.ExtractScale();
            }
        }

        /// <summary>
        /// ローカルの姿勢行列を取得する
        /// </summary>
        public Matrix4 LocalTransform
        {
            get
            {
                return MatrixHelper.CreateTransform(ref Position, ref Rotate, ref Scale);
            }
            set
            {
                Position = value.ExtractTranslation();
                Rotate = value.ExtractEulerRotation();
                Scale = value.ExtractScale();
            }
        }

        /// <summary>
        /// ワールドの姿勢行列を取得する
        /// </summary>
        public Matrix4 WorldTransform
        {
            get
            {
                Matrix4 wt = LocalTransform;
                Transform p = Parent;
                while (p != null)
                {
                    Matrix4 pl = p.LocalTransform;
                    Matrix4.Mult(ref wt, ref pl, out wt);
                    p = p.Parent;
                }
                return wt;
            }
            set
            {
                var w = ParentWorldTransform;
                Matrix4 local = value;
                if (w != Matrix4.Identity)
                {
                    w.Invert();
                    Matrix4.Mult(ref local, ref w, out local);
                }
                Position = local.ExtractTranslation();
                Rotate = local.ExtractEulerRotation();
                Scale = local.ExtractScale();
            }
        }

        /// <summary>
        /// 親のワールド姿勢行列を取得する
        /// </summary>
        public Matrix4 ParentWorldTransform
        {
            get
            {
                if (Parent != null) return Parent.WorldTransform;
                else return Matrix4.Identity;
            }
        }

        /// <summary>
        /// ローカルのZ向きを取得する
        /// </summary>
        public Vector3 LocalDirectionZ
        {
            get
            {
                return Vector3.TransformVector(Vector3.UnitZ, LocalTransform);
            }
        }

        /// <summary>
        /// ローカルのY向きを取得する
        /// </summary>
        public Vector3 LocalDirectionY
        {
            get
            {
                return Vector3.TransformVector(Vector3.UnitY, LocalTransform);
            }
        }

        /// <summary>
        /// ローカルのX向きを取得する
        /// </summary>
        public Vector3 LocalDirectionX
        {
            get
            {
                return Vector3.TransformVector(Vector3.UnitX, LocalTransform);
            }
        }

        /// <summary>
        /// ワールドのZ向きを取得する
        /// </summary>
        public Vector3 WorldDirectionZ
        {
            get
            {
                return Vector3.TransformVector(Vector3.UnitZ, WorldTransform);
            }
        }

        /// <summary>
        /// ワールドのY向きを取得する
        /// </summary>
        public Vector3 WorldDirectionY
        {
            get
            {
                return Vector3.TransformVector(Vector3.UnitY, WorldTransform);
            }
        }

        /// <summary>
        /// ワールドのX向きを取得する
        /// </summary>
        public Vector3 WorldDirectionX
        {
            get
            {
                return Vector3.TransformVector(Vector3.UnitX, WorldTransform);
            }
        }

        /// <summary>
        /// 物理情報の姿勢を更新する
        /// </summary>
        public void UpdatePhysicalTransform()
        {
            foreach (var phy in GameObject.GetComponents<PhysicalGameComponent>())
            {
                var rb = phy as RigidBody;
                if (rb != null)
                {
                    rb.rigidBody.BulletRigidBody.WorldTransform = WorldTransform;
                    return;
                }

            }
        }

        public Matrix4 OldWorldTransfom { get; set; }

        public override GameComponent Clone()
        {
            return new Transform()
            {
                Position = Position,
                Rotate = Rotate,
                Scale = Scale,
                Parent = Parent,
            };
        }

        public override string Name { get; protected set; } = "Transform";
    }
}
