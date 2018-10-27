using MikuMikuWorld.Assets;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.GameComponents
{
    public abstract class AAnimator : DrawableGameComponent
    {
        public abstract float Frame { get; set; }
        public abstract bool EnableAsyncCalc { get; }

        public abstract bool AddMotion(string name, Motion motion);
        public abstract bool RemoveMotion(string name);
        public abstract bool HasMotion(string name);

        public abstract int GetMaxFrame(string name);
        public abstract float GetRate(string name);
        public abstract float SetRate(string name, float value);
        public abstract float AddRate(string name, float value);
        public abstract float AddRate(string name, float value, float min, float max);

        public abstract void CalcTransform();
        public abstract void UpdateData();

        public abstract void BindMotion(int binding, int oldBinding = -1);
        public abstract void UnbindMotion(int binding, int oldBinding = -1);

        public abstract Matrix4 GetLocalTransform(string name);
        public abstract void SetRotation(string name, Vector3 rot);
        public abstract void SetScale(string name, Vector3 scale);

        private static int ssboInit = -1;
        private static Matrix4[] Identities = new Matrix4[512];
        protected internal override void OnLoad()
        {
            base.OnLoad();

            if (ssboInit == -1)
            {
                GL.GenBuffers(1, out ssboInit);
                for (var i = 0; i < Identities.Length; i++) Identities[i] = Matrix4.Identity;
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ssboInit);
                GL.BufferData(BufferTarget.ShaderStorageBuffer, Identities.Length * 16 * 4, Identities, BufferUsageHint.StaticDraw);
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
            }
        }

        public static void BindIdentity(int binding, int oldBinding = -1)
        {
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, binding, ssboInit);
            if (oldBinding >= 0) GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, oldBinding, ssboInit);
        }
        public static void UnbindIdentity(int binding, int oldBinding = -1)
        {
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, binding, 0);
            if (oldBinding >= 0) GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, oldBinding, 0);
        }

        protected internal override void OnReceivedMessage(string message, params object[] args)
        {
            if (message == "set bone rotation")
            {
                SetRotation((string)args[0], (Vector3)args[1]);
            }
            else if (message == "set bone scale")
            {
                SetScale((string)args[0], (Vector3)args[1]);
            }
        }

        protected internal override RequestResult<T> OnReceivedRequest<T>(string request, params object[] args)
        {
            if (request == "get bone transform")
            {
                return new RequestResult<T>(this, (T)(object)GetLocalTransform((string)args[0]));
            }

            return null;
        }

        protected class FastTransform
        {
            private bool changed;
            private Vector3 position = Vector3.Zero;
            private Quaternion rotation = Quaternion.Identity;
            private Vector3 scale = Vector3.One;
            private Matrix4 localTrans = Matrix4.Identity;

            public Vector3 Position
            {
                get { return position; }
                set { position = value; changed = true; }
            }
            public Quaternion Rotation
            {
                get { return rotation; }
                set { rotation = value; changed = true; }
            }
            public Vector3 Scale
            {
                get { return scale; }
                set { scale = value; changed = true; }
            }
            public Vector3 WorldPosition => WorldTransform.ExtractTranslation();

            public FastTransform Parent;

            public Matrix4 LocalTransform
            {
                get
                {
                    if (changed)
                    {
                        MatrixHelper.CreateTransform(ref position, ref rotation, ref scale, out localTrans);
                        changed = false;
                    }
                    return localTrans;
                }
                set
                {
                    position = value.ExtractTranslation();
                    rotation = value.ExtractRotation();
                    scale = value.ExtractScale();
                    localTrans = value;
                    changed = false;
                }
            }

            public Matrix4 WorldTransform
            {
                get
                {
                    Matrix4 wt = LocalTransform;
                    FastTransform p = Parent;
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
                    w.Invert();
                    Matrix4.Mult(ref value, ref w, out localTrans);
                    position = localTrans.ExtractTranslation();
                    rotation = localTrans.ExtractRotation();
                    scale = localTrans.ExtractScale();
                    changed = false;
                }
            }

            public Matrix4 ParentWorldTransform
            {
                get
                {
                    if (Parent != null) return Parent.WorldTransform;
                    else return Matrix4.Identity;
                }
            }
        }
    }
}
