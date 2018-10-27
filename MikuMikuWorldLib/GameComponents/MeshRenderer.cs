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

using MikuMikuWorld.Assets;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.GameComponents
{
    /// <summary>
    /// メッシュを描画するためのコンポーネント
    /// </summary>
    public class MeshRenderer : DrawableGameComponent
    {
        public MeshRenderer() : this(null) { }
        public MeshRenderer(Mesh mesh)
        {
            Mesh = mesh;

            getter.Add("MaterialCount", (obj) => MaterialCount);
            getter.Add("Visible", (obj) => Visible);
            getter.Add("CastShadow", (obj) => CastShadow);
            getter.Add("Mesh", (obj) => Mesh);
            getter.Add("Materials", (obj) => Materials);

            setter.Add("Visible", (obj, value) => Visible = (bool)value);
            setter.Add("CastShadow", (obj, value) => CastShadow = (bool)value);
        }

        /// <summary>
        /// 描画するメッシュ
        /// </summary>
        public Mesh Mesh { get; set; }

        /// <summary>
        /// スキンメッシュに用いるボーンリスト
        /// </summary>
        public Bone[] Bones { get; set; }

        /// <summary>
        /// メッシュを描画するか
        /// </summary>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// 画面外でもメッシュを描画するか
        /// </summary>
        public bool ForceRendering { get; set; } = false;

        /// <summary>
        /// メッシュが影を生成するか
        /// </summary>
        public bool CastShadow { get; set; } = true;

        /// <summary>
        /// 遮蔽オブジェクト
        /// </summary>
        public bool SieldObject { get; set; } = false;

        public BeginMode BeginMode { get; set; } = BeginMode.Triangles;

        internal int query = -1;

        public int MaterialCount { get { return materials.Keys.Count; } }
        public Material[] Materials
        {
            get { return materials.Values.ToArray(); }
        }

        protected internal Dictionary<int, Material> materials = new Dictionary<int, Material>();

        protected internal override void OnLoad()
        {
            base.OnLoad();
        }

        public Result SetMaterial(int index, Material mat, bool shared = false)
        {
            if (index < 0) return Result.OutOfIndex;
            if (mat == null) return Result.ObjectIsNull;
            if (!shared) mat = mat.Clone();
            if (!materials.ContainsKey(index)) materials.Add(index, mat);
            else materials[index] = mat;
            return Result.Success;
        }
        public Material GetMaterial(int index)
        {
            if (!materials.Keys.Contains(index)) return null;
            return materials[index];
        }
        public Material GetMaterialAt(int index)
        {
            var idx = materials.Keys.ElementAt(index);
            return materials[idx];
        }
        public Material GetMaterial(string name)
        {
            return materials.FindValue(m => m.Name == name);
        }

        protected internal override void Draw(double deltaTime, Camera camera) { }

        public override GameComponent Clone()
        {
            return new MeshRenderer()
            {
                Mesh = Mesh,
                materials = new Dictionary<int, Material>(materials),
            };
        }

        protected internal override void OnReceivedMessage(string message, params object[] args)
        {
            if (message == "set material")
            {
                var mat = GetMaterial((string)args[0]);
                Type t = (Type)args[2];
                if (t == typeof(float)) mat.TrySetParam((string)args[1], (float)args[3]);
                else if (t == typeof(Vector2)) mat.TrySetParam((string)args[1], (Vector2)args[3]);
                else if (t == typeof(Vector3)) mat.TrySetParam((string)args[1], (Vector3)args[3]);
                else if (t == typeof(Vector4)) mat.TrySetParam((string)args[1], (Vector4)args[3]);
                else if (t == typeof(Color4)) mat.TrySetParam((string)args[1], (Color4)args[3]);
            }
            else if (message == "set all material")
            {
                foreach (var mat in materials.Values)
                {
                    Type t = (Type)args[1];
                    if (t == typeof(float)) mat.TrySetParam((string)args[0], (float)args[2]);
                    else if (t == typeof(Vector2)) mat.TrySetParam((string)args[0], (Vector2)args[2]);
                    else if (t == typeof(Vector3)) mat.TrySetParam((string)args[0], (Vector3)args[2]);
                    else if (t == typeof(Vector4)) mat.TrySetParam((string)args[0], (Vector4)args[2]);
                    else if (t == typeof(Color4)) mat.TrySetParam((string)args[0], (Color4)args[2]);
                }
            }
            else if (message == "reset material")
            {
                var mat = GetMaterial((string)args[0]);
                var srcMat = mat.srcMat;
                if (srcMat == null) srcMat = mat;
                var name = (string)args[1];
                Type t = (Type)args[2];
                if (t == typeof(float)) mat.TrySetParam(name, srcMat.GetParam<float>(name));
                else if (t == typeof(Vector2)) mat.TrySetParam(name, srcMat.GetParam<Vector2>(name));
                else if (t == typeof(Vector3)) mat.TrySetParam(name, srcMat.GetParam<Vector3>(name));
                else if (t == typeof(Vector4)) mat.TrySetParam(name, srcMat.GetParam<Vector4>(name));
                else if (t == typeof(Color4)) mat.TrySetParam(name, srcMat.GetParam<Color4>(name));
            }
            else if (message == "reset all material")
            {
                foreach (var mat in materials.Values)
                {
                    var srcMat = mat.srcMat;
                    if (srcMat == null) srcMat = mat;
                    var name = (string)args[1];
                    Type t = (Type)args[1];
                    if (t == typeof(float)) mat.TrySetParam(name, srcMat.GetParam<float>(name));
                    else if (t == typeof(Vector2)) mat.TrySetParam(name, srcMat.GetParam<Vector2>(name));
                    else if (t == typeof(Vector3)) mat.TrySetParam(name, srcMat.GetParam<Vector3>(name));
                    else if (t == typeof(Vector4)) mat.TrySetParam(name, srcMat.GetParam<Vector4>(name));
                    else if (t == typeof(Color4)) mat.TrySetParam(name, srcMat.GetParam<Color4>(name));
                }
            }
        }

        protected internal override RequestResult<T> OnReceivedRequest<T>(string request, params object[] args)
        {
            if (request == "get bone count")
            {
                if (Bones != null) return new RequestResult<T>(this, (T)(object)Bones.Length);
                else return new RequestResult<T>(this, (T)(object)0);
            }
            else if (request == "get bone name")
            {
                if (Bones == null || (int)args[0] >= Bones.Length) return new RequestResult<T>(this, (T)(object)null);
                return new RequestResult<T>(this, (T)(object)Bones[(int)args[0]].Name);
            }
            else if (request == "has bone")
            {
                if (Bones == null) return new RequestResult<T>(this, (T)(object)false);
                return new RequestResult<T>(this, (T)(object)(Array.Exists(Bones, b => b.Name == (string)args[0])));
            }

            return null;
        }
    }
}
