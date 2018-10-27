using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikuMikuWorld.Assets;
using MikuMikuWorld.Assets.Shaders;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace MikuMikuWorld.GameComponents
{
    public class ComputeAnimator : AAnimator
    {
        public override bool EnableAsyncCalc => true;

        class MotionData
        {
            public Motion Motion;
            public float Rate;
        }

        private Dictionary<string, MotionData> motionDic = new Dictionary<string, MotionData>();

        private int ssboDst = -1;
        private int ssboOldDst = -1;
        private int ssboBone = -1;
        private int ssboTrans = -1;
        private int ssboBufferSize;
        private bool dataChanged = true;

        public int TransformSize
        {
            get { return transforms.Length; }
            set
            {
                transforms = new Matrix4[value];
                bmvs = new BoneMotionValue[value];

                if (ssboDst == -1)
                {
                    GL.GenBuffers(1, out ssboDst);
                    GL.GenBuffers(1, out ssboOldDst);
                    GL.GenBuffers(1, out ssboTrans);
                }

                ssboBufferSize = transforms.Length * 16 * 4;

                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ssboDst);
                GL.BufferData(BufferTarget.ShaderStorageBuffer, ssboBufferSize, transforms, BufferUsageHint.DynamicDraw);

                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ssboOldDst);
                GL.BufferData(BufferTarget.ShaderStorageBuffer, ssboBufferSize, transforms, BufferUsageHint.DynamicDraw);

                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ssboTrans);
                GL.BufferData(BufferTarget.ShaderStorageBuffer, ssboBufferSize, transforms, BufferUsageHint.DynamicDraw);

                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
            }
        }

        private float frame = 0.0f;
        public override float Frame
        {
            get { return frame; }
            set { frame = value; dataChanged = true; }
        }

        private ComputeTransformShader transShader;

        private Dictionary<string, int> boneIndexDic = new Dictionary<string, int>();
        private BoneMotionValue[] bmvs;
        private Matrix4[] transforms;
        private Matrix4[] initBoneTransforms;
        FastTransform[] BoneTransforms { get; set; }
        private Bone[] bones;
        public Bone[] Bones
        {
            get { return bones; }
            set
            {
                boneIndexDic.Clear();
                if (value == null) return;

                for (var i = 0; i < value.Length; i++) boneIndexDic.Add(value[i].Name, i);
                TransformSize = boneIndexDic.Values.Max() + 1;

                bones = value;

                byte[] buffer = null;
                using (var ms = new MemoryStream())
                {
                    using (var bw = new BinaryWriter(ms))
                    {
                        foreach (var b in bones)
                        {
                            bw.Write(b.Position.X);
                            bw.Write(b.Position.Y);
                            bw.Write(b.Position.Z);
                            if (b.Parent != null) bw.Write((float)b.Parent.Index);
                            else bw.Write((float)-1);
                        }
                    }
                    buffer = ms.ToArray();
                }

                GL.GenBuffers(1, out ssboBone);
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ssboBone);
                GL.BufferData(BufferTarget.ShaderStorageBuffer, buffer.Length, buffer, BufferUsageHint.StaticDraw);
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

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
                dataChanged = true;
            }
        }

        private AMorpher morpher;

        protected internal override void OnLoad()
        {
            base.OnLoad();
            dataChanged = true;
            morpher = GameObject.GetComponent<AMorpher>();
            transShader = new ComputeTransformShader();
            transShader.Load();
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
                        //if (w > 0.0f && w < 1.0f) Debug.WriteLine(sm.MorphName + w);
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
            Parallel.ForEach(motionDic.Values.Where(m => m.Rate > 0.0f), m =>
            {
                foreach (var bm in m.Motion.BoneMotions.Values)
                {
                    int index;
                    if (!boneIndexDic.TryGetValue(bm.BoneName, out index)) continue;

                    var values = GetKeyAndRate(bm.Keys, BoneMotionValue.Identity);
                    if (values.Item3 == -1.0f) continue;

                    BoneMotionValue bmv;
                    BoneMotionValue.Lerp(ref values.Item1.Value, ref values.Item2.Value, values.Item3, out bmv);
                    bmv *= m.Rate;
                    if (bmvs[index] == BoneMotionValue.Identity) bmvs[index] = bmv;
                    else bmvs[index] += bmv;
                }
            });

            // update transform
            for (var i = 0; i < BoneTransforms.Length; i++)
            {
                bmvs[i].rotation *= userRotations[i];
                bmvs[i].scale *= userScales[i];
                BoneTransforms[i].Position = initBoneTransforms[i].ExtractTranslation() + bmvs[i].location;
                BoneTransforms[i].Rotation = bmvs[i].rotation;
                BoneTransforms[i].Scale = bmvs[i].scale;
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


            for (var i = 0; i < transforms.Length; i++)
            {
                transforms[i] = bmvs[i].CreateTransform();
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
            // コピー
            GL.BindBuffer(BufferTarget.CopyReadBuffer, ssboDst);
            GL.BindBuffer(BufferTarget.CopyWriteBuffer, ssboOldDst);
            GL.CopyBufferSubData(
                BufferTarget.CopyReadBuffer,
                BufferTarget.CopyWriteBuffer,
                (IntPtr)0,
                (IntPtr)0,
                ssboBufferSize);
            GL.BindBuffer(BufferTarget.CopyReadBuffer, 0);
            GL.BindBuffer(BufferTarget.CopyWriteBuffer, 0);

            if (!dataChanged) return;

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ssboTrans);
            GL.BufferSubData(BufferTarget.ShaderStorageBuffer, (IntPtr)0, ssboBufferSize, transforms);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, ssboDst);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, ssboBone);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, ssboTrans);

            transShader.UseShader();
            GL.DispatchCompute(transforms.Length / 16 + 1, 1, 1);
            transShader.UnuseShader();

            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, 0);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, 0);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, 0);

            dataChanged = false;
        }

        public override void BindMotion(int binding, int oldBinding = -1)
        {
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, binding, ssboDst);
            if (oldBinding >= 0) GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, oldBinding, ssboOldDst);
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

            Matrix4 res = Matrix4.Identity;
            //res = transforms[bone.Index];
            //var test = new Matrix4[transforms.Length];
            //GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ssboDst);
            //GL.GetBufferSubData(BufferTarget.ShaderStorageBuffer, (IntPtr)(64 * bone.Index), 64, ref res);
            //GL.GetBufferSubData(BufferTarget.ShaderStorageBuffer, (IntPtr)0, test.Length * 64, test);
            //GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

            res = BoneTransforms[bone.Index].WorldTransform;
            var rot = res.ExtractEulerRotation();
            var pos = res.ExtractTranslation();
            var scale = Vector3.One;

            return MatrixHelper.CreateTransform(ref pos, ref rot, ref scale);

        }

        protected internal override void Draw(double deltaTime, Camera camera)
        { }

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
            return new ComputeAnimator()
            {
                motionDic = new Dictionary<string, MotionData>(motionDic),
                bones = bones,
                frame = frame,
                boneIndexDic = new Dictionary<string, int>(boneIndexDic),
            };
        }

        
    }
}
