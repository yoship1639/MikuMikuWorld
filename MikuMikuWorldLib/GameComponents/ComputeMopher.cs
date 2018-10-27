using MikuMikuWorld.Assets;
using MikuMikuWorld.Assets.Shaders;
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
    public class ComputeMorpher : AMorpher
    {
        public override bool EnableAsyncCalc => true;

        class MorphData
        {
            public Morph Morph;
            public float Rate;
            public int Ssbo;
            public Vector4[] Data;
            public int Size;
            public int Start;
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
                if (ssboDst == -1)
                {
                    GL.GenBuffers(1, out ssboDst);
                } 
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ssboDst);
                GL.BufferData(BufferTarget.ShaderStorageBuffer, VerticesSize * 16, vertices, BufferUsageHint.DynamicDraw);
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
            }
        }
        private Vector4[] vertices;
        private int ssboDst = -1;
        private bool dataChanged = false;

        private ComputeMorphShader comShader;

        protected internal override void OnLoad()
        {
            base.OnLoad();

            VerticesSize = 65536;
            comShader = new ComputeMorphShader();
            comShader.Load();
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

            var start = 0;
            var size = 0;
            var ssbo = 0;
            Vector4[] data = null;

            if (morph.Vertices != null)
            {
                //start = morph.Vertices.Min(v => v.Index);
                var to = morph.Vertices.Max(v => v.Index);
                //size = to - start + 1;
                start = 0;
                size = to + 1;
                data = new Vector4[size];
                foreach (var v in morph.Vertices)
                {
                    data[v.Index] = new Vector4(v.Offset, 0.0f);
                }

                GL.GenBuffers(1, out ssbo);
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ssbo);
                GL.BufferData(BufferTarget.ShaderStorageBuffer, size * 16, data, BufferUsageHint.StaticRead);
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
            }

            morphDic.Add(name, new MorphData()
            {
                Morph = morph,
                Rate = 0.0f,
                Start = start,
                Size = size,
                Ssbo = ssbo,
                Data = data,
            });

            return true;
        }
        public override bool RemoveMorph(string name)
        {
            MorphData data;
            if (morphDic.TryGetValue(name, out data))
            {
                GL.DeleteBuffer(data.Ssbo);
            }
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
            dataChanged = false;
        }
        public override void UpdateData()
        {
            var index = 0;
            var offsets = new int[4];
            var sizes = new int[4];
            var w = new float[4];
            foreach (var m in morphDic.Values)
            {
                if (m.Rate == 0.0f) continue;
                if (m.Morph.Vertices != null)
                {
                    GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, index + 1, m.Ssbo);
                    offsets[index] = m.Start;
                    sizes[index] = m.Size;
                    w[index] = m.Rate;
                }
                index++;
                if (index >= 4) break;
            }

            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, ssboDst);

            comShader.UseShader();
            //comShader.SetParameter(comShader.loc_offset, offsets);
            //comShader.SetParameter(comShader.loc_size, sizes);
            comShader.SetParameter(comShader.loc_weight, new Vector4(w[0], w[1], w[2], w[3]));
            GL.DispatchCompute(VerticesSize / 64 + 1, 1, 1);
            comShader.UnuseShader();

            for (var i = 0; i < 5; i++) GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, i, 0);
        }

        public override void UseMorph(int binding)
        {
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, binding, ssboDst);
        }
        public override void UnuseMorph(int binding)
        {
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, binding, 0);
        }

        public override GameComponent Clone()
        {
            return new ComputeMorpher()
            {
                morphDic = new Dictionary<string, MorphData>(morphDic),
            };
        }
    }
}
