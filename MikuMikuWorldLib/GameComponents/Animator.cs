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
    public class Animator : AAnimator
    {
        public override bool EnableAsyncCalc => true;

        class MotionData
        {
            public Motion Motion;
            public float Rate;
            
        }
        
        private Dictionary<string, MotionData> motionDic = new Dictionary<string, MotionData>();

        private int ssbo = -1;
        private int ssboOld = -1;
        private bool dataChanged = false;

        public int TransformSize
        {
            get { return transforms.Length; }
            set
            {
                transforms = new Matrix4[value];
                oldTransforms = new Matrix4[value];
                boneTranses = new Matrix4[value];
                bmvs = new BoneMotionValue[value];
                if (ssbo == -1)
                {
                    GL.GenBuffers(1, out ssbo);
                    GL.GenBuffers(1, out ssboOld);
                }
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ssbo);
                GL.BufferData(BufferTarget.ShaderStorageBuffer, transforms.Length * 16 * 4, transforms, BufferUsageHint.StaticDraw);
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ssboOld);
                GL.BufferData(BufferTarget.ShaderStorageBuffer, transforms.Length * 16 * 4, transforms, BufferUsageHint.StaticDraw);
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
            }
        }

        private float frame = 0.0f;
        public override float Frame
        {
            get { return frame; }
            set { dataChanged = (value != frame); frame = value; }
        }

        private Dictionary<string, int> indexDic = new Dictionary<string, int>();
        private BoneMotionValue[] bmvs;
        private Matrix4[] transforms;
        private Matrix4[] oldTransforms;
        private Matrix4[] boneInvOffsets;
        private Matrix4[] boneOffsets;
        private Matrix4[] boneTranses;
        private Bone[] bones;
        public Bone[] Bones
        {
            get { return bones; }
            set
            {
                indexDic.Clear();
                if (value == null) return;

                for (var i = 0; i < value.Length; i++) indexDic.Add(value[i].Name, i);
                TransformSize = indexDic.Values.Max() + 1;

                bones = value;

                boneInvOffsets = new Matrix4[bones.Length];
                boneOffsets = new Matrix4[bones.Length];
                for (var i = 0; i < bones.Length; i++)
                {
                    boneOffsets[i] = Matrix4.CreateTranslation(-bones[i].Position);

                    if (bones[i].Parent == null) boneInvOffsets[i] = Matrix4.CreateTranslation(bones[i].Position);
                    else boneInvOffsets[i] = Matrix4.CreateTranslation(bones[i].Position - bones[i].Parent.Position);
                }

                BoneTransforms = new FastTransform[bones.Length];
                initBoneTransforms = new Matrix4[bones.Length];

                userRotations = new Quaternion[bones.Length];
                for (var i = 0; i < bones.Length; i++) userRotations[i] = Quaternion.Identity;

                userScales = new Vector3[bones.Length];
                for (var i = 0; i < bones.Length; i++) userScales[i] = Vector3.One;

                var roots = Array.FindAll(bones, b => b.Parent == null);
                foreach (var root in roots) CreateTransforms(root, null);
                for (var i = 0; i < bones.Length; i++)
                {
                    initBoneTransforms[i] = BoneTransforms[i].LocalTransform;
                }  
            }
        }

        private Matrix4[] initBoneTransforms;
        FastTransform[] BoneTransforms { get; set; }

        private AMorpher morpher;

        protected internal override void OnLoad()
        {
            base.OnLoad();
            dataChanged = true;
            morpher = GameObject.GetComponent<AMorpher>();
        }

        public override bool AddMotion(string name, Motion motion)
        {
            if (motion == null || string.IsNullOrWhiteSpace(name)) return false;
            if (motionDic.ContainsKey(name)) return false;

            motionDic.Add(name, new MotionData()
            {
                Motion = motion,
                Rate = 0.0f,
            });

            return true;
        }
        public override bool RemoveMotion(string name)
        {
            return motionDic.Remove(name);
        }
        public override bool HasMotion(string name)
        {
            return motionDic.ContainsKey(name);
        }

        public override int GetMaxFrame(string name)
        {
            MotionData m;
            if (motionDic.TryGetValue(name, out m))
            {
                return m.Motion.FrameNoMax;
            }
            return -1;
        }
        public override float GetRate(string name)
        {
            MotionData m;
            if (motionDic.TryGetValue(name, out m))
            {
                return m.Rate;
            }
            return -1.0f;
        }
        public override float SetRate(string name, float value)
        {
            MotionData m;
            if (motionDic.TryGetValue(name, out m))
            {
                if (m.Rate == value) return value;
                dataChanged = true;
                m.Rate = value;
                return value;
            }
            return -1.0f;
        }
        public override float AddRate(string name, float value)
        {
            MotionData m;
            if (motionDic.TryGetValue(name, out m))
            {
                dataChanged = true;
                m.Rate += value;
                return m.Rate;
            }
            return -1.0f;
        }
        public override float AddRate(string name, float value, float min, float max)
        {
            MotionData m;
            if (motionDic.TryGetValue(name, out m))
            {
                dataChanged = true;
                m.Rate = MathHelper.Clamp(m.Rate + value, min, max);
                return m.Rate;
            }
            return m.Rate;
        }

        public override void CalcTransform()
        {
            if (bones == null) return;
            // 計算済みの場合は何もしない
            if (!dataChanged) return;

            for (var i = 0; i < TransformSize; i++) bmvs[i].Init();

            Array.Copy(transforms, oldTransforms, transforms.Length);

            // morph
            if (morpher != null)
            {
                foreach (var m in motionDic.Values)
                {
                    if (m.Rate == 0.0f) continue;

                    foreach (var sm in m.Motion.SkinMotions.Values)
                    {
                        if (!morpher.HasMorph(sm.MorphName)) continue;
                        
                        var values = GetKeyAndRate(sm.Keys, 0.0f);
                        if (values.Item3 == -1.0f) continue;

                        var w = MMWMath.Lerp(values.Item1.Value, values.Item2.Value, values.Item3);
                        morpher.SetRate(sm.MorphName, w);
                    }
                }

                var bms = morpher.GetBoneTransforms();
                foreach (var bm in bms)
                {
                    bmvs[bm.Index].location = bm.Location;
                    bmvs[bm.Index].rotation = bm.Rotation;
                }
            }

            // bone
            foreach (var m in motionDic.Values)
            {
                if (m.Rate == 0.0f) continue;

                foreach (var bm in m.Motion.BoneMotions.Values)
                {
                    int index;
                    if (!indexDic.TryGetValue(bm.BoneName, out index)) continue;

                    var values = GetKeyAndRate(bm.Keys, BoneMotionValue.Identity);
                    if (values.Item3 == -1.0f) continue;

                    BoneMotionValue bmv;
                    BoneMotionValue.Lerp(ref values.Item1.Value, ref values.Item2.Value, values.Item3, out bmv);
                    bmv *= m.Rate;
                    if (bmvs[index] == BoneMotionValue.Identity) bmvs[index] = bmv;
                    else bmvs[index] += bmv;
                }
            }

            // update transform
            for (var i = 0; i < boneTranses.Length; i++)
            {
                BoneTransforms[i].Position = initBoneTransforms[i].ExtractTranslation() + bmvs[i].location;
                BoneTransforms[i].Rotation = bmvs[i].rotation * userRotations[i];
                BoneTransforms[i].Scale = bmvs[i].scale * userScales[i];
            }
            
            // TODO: IKボーン変形
            var iks = Array.FindAll(bones, b => b.BoneType == "IK");
            if (iks.Length > 0)
            {
                foreach (var ik in iks)
                {
                    if (bmvs[ik.Index].location == Vector3.Zero) continue;

                    var ts = new List<FastTransform>();
                    for (var i = ik.IKLinks.Length - 1; i >= 0; i--)
                    {
                        ts.Add(BoneTransforms[ik.IKLinks[i].Bone.Index]);
                    }

                    ts.Add(BoneTransforms[ik.IKTarget.Index]);

                    ts.Reverse();

                    var target = BoneTransforms[ik.Index].WorldPosition;

                    // loop回数繰り返す
                    var loop = 0;
                    var end = ik.IKLoop / 3;
                    for (loop = 0; loop < end; loop++)
                    {
                        for (var c = 1; c < ts.Count; c++)
                        {
                            var t = ts[c];
                            var tPos = t.WorldPosition;
                            var pt = ts[0];

                            //内積, 外積を求める
                            var v = (pt.WorldPosition - tPos).Normalized();
                            var vt = (target - tPos).Normalized();
                            var dot = Vector3.Dot(v, vt);

                            if (dot < 1.0f)
                            {
                                // 角度を求める
                                var rot = (float)Math.Acos(dot);

                                var cross = Vector3.Cross(v, vt);

                                // 制限角度以上ならclamp
                                if (rot > ik.IKRotLimit) rot = ik.IKRotLimit;

                                // 姿勢を変形
                                var r = Quaternion.FromAxisAngle(cross, rot);
                                t.Rotation *= r;
                                if (ik.IKLinks[c - 1].LimitAngle)
                                {
                                    var euler = t.Rotation.ToEuler();
                                    //var p = BoneTransforms[ik.IKLinks[c - 1].Bone.Index].WorldTransform.ExtractRotation();
                                    //if (loop == 0 && ik.Name == "右足ＩＫ") Console.WriteLine(p);
                                    var l = ik.IKLinks[c - 1].LowerLimitAngle;
                                    var u = ik.IKLinks[c - 1].UpperLimitAngle;
                                    euler = Vector3.Clamp(euler, l, u);
                                    t.Rotation = euler.ToQuaternion();
                                }
                            }
                        }

                        var length = (ts[0].WorldPosition - target).Length;
                        if (length < 0.02f) break;
                    }

                    ts.RemoveAt(0);

                    for (var i = 0; i < ik.IKLinks.Length; i++)
                    {
                        var idx = ik.IKLinks[i].Bone.Index;
                        bmvs[idx].rotation = ts[i].Rotation;
                    }
                }
            }
            

            for (var i = 0; i < boneTranses.Length; i++)
            {
                if (bmvs[i] == BoneMotionValue.Identity)
                {
                    boneTranses[i] = Matrix4.Identity;
                }
                else
                {
                    boneTranses[i] = bmvs[i].CreateTransform();
                }
            }

            // FKボーンを確定
            {
                var iden = Matrix4.Identity;
                var all = Array.FindAll(bones, b => b.Parent == null);
                for (var i = all.Length - 1; i >= 0; i--)
                {
                    ConfirmTransforms(all[i], ref iden);
                }
            }

            dataChanged = false;
        }
        private void ConfirmTransforms(Bone current, ref Matrix4 parentTrans)
        {
            Matrix4 trans;
            Matrix4.Mult(ref boneTranses[current.Index], ref boneInvOffsets[current.Index], out trans);
            Matrix4.Mult(ref trans, ref parentTrans, out trans);
            Matrix4.Mult(ref boneOffsets[current.Index], ref trans, out transforms[current.Index]);

            if (current.Children != null)
            {
                for (var i = current.Children.Length - 1; i >= 0; i--)
                {
                    ConfirmTransforms(current.Children[i], ref trans);
                }
            }
        }
        private Tuple<KeyFrame<T>, KeyFrame<T>, float> GetKeyAndRate<T>(List<KeyFrame<T>> keys, T defaultValue = default(T))
        {
            KeyFrame<T> key0 = null;
            KeyFrame<T> key1 = null;

            var idx = keys.Count - 1;
            key0 = BinarySearch(keys, ref idx, keys.Count);
            if (idx < keys.Count - 1) key1 = keys[idx + 1];
            else key1 = key0;

            if (key1.FrameNo == 0 && key1.Value.Equals(defaultValue)) return new Tuple<KeyFrame<T>, KeyFrame<T>, float>(key0, key1, -1.0f);

            float rate = 0.0f;
            {
                var s = frame - key0.FrameNo;
                var l = key1.FrameNo - key0.FrameNo;
                if (l != 0) rate = key0.Interpolate.GetRate(s / l);
            }

            return new Tuple<KeyFrame<T>, KeyFrame<T>, float>(key0, key1, rate);
        }

        private KeyFrame<T> BinarySearch<T>(List<KeyFrame<T>> keys, ref int index, int length)
        {
            if (length <= 2)
            {
                while (keys[index].FrameNo > frame) index--;
                return keys[index];
            }

            var sub = (int)Math.Ceiling(length / 2.0);
            length = (int)Math.Floor(length / 2.0);
            if (keys[index - sub].FrameNo > frame)
            {
                index -= sub;
                return BinarySearch(keys, ref index, length);
            }
            else
            {
                return BinarySearch(keys, ref index, length);
            }
        }

        public override void UpdateData()
        {
            if (ssbo == -1) return;
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ssbo);
            GL.BufferSubData(BufferTarget.ShaderStorageBuffer, (IntPtr)0, TransformSize * 16 * 4, transforms);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ssboOld);
            GL.BufferSubData(BufferTarget.ShaderStorageBuffer, (IntPtr)0, TransformSize * 16 * 4, oldTransforms);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
        }

        public override void BindMotion(int binding, int oldBinding = -1)
        {
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, binding, ssbo);
            if (oldBinding >= 0) GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, oldBinding, ssboOld);
        }
        public override void UnbindMotion(int binding, int oldBinding = -1)
        {
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, binding, 0);
            if (oldBinding >= 0) GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, oldBinding, 0);
        }

        private void CreateTransforms(Bone current, FastTransform parent)
        {
            var t = new FastTransform();

            t.Parent = parent;
            t.WorldTransform = Matrix4.CreateTranslation(current.Position);
            BoneTransforms[current.Index] = t;

            if (current.Children != null)
            {
                foreach (var c in current.Children) CreateTransforms(c, t);
            }
        }

        public override Matrix4 GetLocalTransform(string name)
        {
            var bone = Array.Find(Bones, b => b.Name == name);
            if (bone == null) return Matrix4.Identity;

            return transforms[bone.Index];
        }

        private Quaternion[] userRotations;
        private Vector3[] userScales;
        public override void SetRotation(string name, Vector3 rot)
        {
            var bone = Array.Find(Bones, b => b.Name == name);
            if (bone == null) return;

            userRotations[bone.Index] = rot.ToQuaternion();
        }

        public override void SetScale(string name, Vector3 scale)
        {
            var bone = Array.Find(Bones, b => b.Name == name);
            if (bone == null) return;

            userScales[bone.Index] = scale;
        }

        public override GameComponent Clone()
        {
            return new Animator()
            {
                motionDic = new Dictionary<string, MotionData>(motionDic),
                bones = bones,
                frame = frame,
                indexDic = new Dictionary<string, int>(indexDic),
            };
        }

        protected internal override void Draw(double deltaTime, Camera camera)
        {
            /*
            if (BoneTransforms == null) return;

            var vp = camera.View * camera.Projection;
            var mvp = GameObject.Transform.WorldTransform * vp;
            GL.Disable(EnableCap.DepthTest);

            for (var i = 0; i < bones.Length; i++)
            {
                if (bones[i].Invisible) continue;
                Drawer.DrawWireframeSphere(0.01f, BoneTransforms[i].WorldTransform * mvp, Color4.Red);
            }

            GL.Enable(EnableCap.DepthTest);
            */
        }

        /*
        class FastTransform
        {
            private bool changed;
            private Vector3 position = Vector3.Zero;
            private Quaternion rotation = Quaternion.Identity;
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
            public Vector3 WorldPosition => WorldTransform.ExtractTranslation();

            public FastTransform Parent;

            public Matrix4 LocalTransform
            {
                get
                {
                    if (changed)
                    {
                        MatrixHelper.CreateTransform(ref position, ref rotation, out localTrans);
                        changed = false;
                    }
                    return localTrans;
                }
                set
                {
                    position = value.ExtractTranslation();
                    rotation = value.ExtractRotation();
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
        }*/
    }
}
