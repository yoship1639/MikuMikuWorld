using MikuMikuWorld.Assets;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.GameComponents
{
    public class Morpher : AMorpher
    {
        public override bool EnableAsyncCalc => true;

        class MorphData
        {
            public Morph Morph;
            public float Rate;
            //public int FromIndex;
            //public int ToIndex;
        }
        Dictionary<string, MorphData> morphDic = new Dictionary<string, MorphData>();
        public int VerticesSize
        {
            get
            {
                return vertices.Length;
            }
            set
            {
                vertices = new Vector4[value];
                if (ssbo == -1)
                {
                    GL.GenBuffers(1, out ssbo);
                } 
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ssbo);
                GL.BufferData(BufferTarget.ShaderStorageBuffer, VerticesSize * 16, vertices, BufferUsageHint.StaticDraw);
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
            }
        }
        private Vector4[] vertices;
        private int ssbo = -1;
        private bool dataChanged = false;

        protected internal override void OnLoad()
        {
            base.OnLoad();

            var max = 65536;
            foreach (var m in morphDic.Values)
            {
                if (m.Morph.Vertices == null) continue;
                var ma = m.Morph.Vertices.Max(v => v.Index);
                if (ma > max) max = ma;
            }

            VerticesSize = max + 1;
        }

        public override bool HasMorph(string name)
        {
            return morphDic.ContainsKey(name);
        }
        public bool HasMorph(string name, Predicate<Morph> match)
        {
            MorphData md;
            var res = morphDic.TryGetValue(name, out md);
            if (!res) return false;

            return match(md.Morph);
        }
        public override bool AddMorph(string name, Morph morph)
        {
            if (morph == null || string.IsNullOrWhiteSpace(name)) return false;
            if (morphDic.ContainsKey(name)) return false;

            if (morph.Vertices != null)
            {
                var to = morph.Vertices.Max(v => v.Index);
                if (to > VerticesSize) VerticesSize = to + 1;
            }

            morphDic.Add(name, new MorphData()
            {
                Morph = morph,
                Rate = 0.0f,
                //FromIndex = from,
                //ToIndex = to,
            });

            return true;
        }
        public override bool RemoveMorph(string name)
        {
            return morphDic.Remove(name);
        }

        public override void SetRate(string name, float value)
        {
            MorphData m;
            if (morphDic.TryGetValue(name, out m))
            {
                if (m.Rate == value) return;
                dataChanged = true;
                m.Rate = value;
            }
        }
        public override void AddRate(string name, float value)
        {
            MorphData m;
            if (morphDic.TryGetValue(name, out m))
            {
                dataChanged = true;
                m.Rate += value;
            }
        }
        public override void AddRate(string name, float value, float min, float max)
        {
            MorphData m;
            if (morphDic.TryGetValue(name, out m))
            {
                dataChanged = true;
                m.Rate = MathHelper.Clamp(m.Rate + value, min, max);
            }
        }

        public override BoneMorph[] GetBoneTransforms()
        {
            var dic = new Dictionary<int, BoneMorph>();

            foreach (var m in morphDic.Values)
            {
                if (m.Rate == 0.0f) continue;
                if (m.Morph.Bones == null) continue;

                foreach (var b in m.Morph.Bones)
                {
                    BoneMorph bm;
                    var r = b.Rotation;
                    r.W /= m.Rate;
                    r.Normalize();
                    if (dic.TryGetValue(b.Index, out bm))
                    {
                        bm.Location += b.Location * m.Rate;
                        bm.Rotation = bm.Rotation * r;
                    }
                    else
                    {
                        dic.Add(b.Index, new BoneMorph()
                        {
                            Index = b.Index,
                            Location = b.Location * m.Rate,
                            Rotation = r,
                        });
                    }
                }
            }

            return dic.Values.ToArray();
        }

        public override void CalcMorph()
        {
            // 計算済みの場合は何もしない
            if (!dataChanged) return;

            Array.Clear(vertices, 0, vertices.Length);
            foreach (var m in morphDic.Values)
            {
                if (m.Rate == 0.0f) continue;
                if (m.Morph.Vertices != null)
                {
                    var l = m.Morph.Vertices.Length;
                    for (var i = 0; i < l; i++)
                    {
                        var idx = m.Morph.Vertices[i].Index;
                        var v = m.Morph.Vertices[i].Offset;
                        vertices[idx].X += v.X * m.Rate;
                        vertices[idx].Y += v.Y * m.Rate;
                        vertices[idx].Z += v.Z * m.Rate;
                    }
                }
            }
            dataChanged = false;
        }
        public override void UpdateData()
        {
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ssbo);
            GL.BufferSubData(BufferTarget.ShaderStorageBuffer, (IntPtr)0, VerticesSize * 16, vertices);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
        }

        public override void UseMorph(int binding)
        {
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, binding, ssbo);
        }
        public override void UnuseMorph(int binding)
        {
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, binding, 0);
        }

        public override GameComponent Clone()
        {
            return new Morpher()
            {
                morphDic = new Dictionary<string, MorphData>(morphDic),
            };
        }

        protected internal override void OnReceivedMessage(string message, params object[] args)
        {
            if (message == "set morph")
            {
                SetRate((string)args[0], (float)args[1]);
            }
            else if (message == "add morph")
            {
                AddRate((string)args[0], (float)args[1]);
            }
        }
    }
}
