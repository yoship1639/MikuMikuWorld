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
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Assets
{
    [DataContract]
    public class Mesh : IAsset
    {
        static protected internal readonly int VertexFloatSize =
            3 + // Vertex
            3 + // Normal
            3 + // Tangent
            4 + // Color
            2 + // UV
            4 + // UV1
            4 + // UV2
            4 + // UV3
            4 + // UV4
            4 + // BoneIndex
            4 + // BoneWeight
            1; // Index

        [DataMember(Name = "name", EmitDefaultValue = false, Order = 0)]
        public string Name { get; set; }

        public Vector3[] Vertices { get; set; }
        public Vector3[] Normals { get; set; }
        public Vector3[] Tangents { get; set; }
        public Color4[] Colors { get; set; }
        public Vector2[] UVs { get; set; }
        public Vector4[] UV1s { get; set; }
        public Vector4[] UV2s { get; set; }
        public Vector4[] UV3s { get; set; }
        public Vector4[] UV4s { get; set; }
        public BoneWeight[] BoneWeights { get; set; }
        public int SubMeshCount { get { return subMeshes.Count; } }

        [DataMember(Name = "bounds", EmitDefaultValue = false, Order = 11)]
        public Bounds Bounds { get; set; }

        [DataMember(Name = "submeshes", EmitDefaultValue = false, Order = 12)]
        public List<SubMesh> subMeshes = new List<SubMesh>();

        public bool Loaded { get; private set; } = false;

        protected internal int vbo = -1;
        //protected internal bool createSubmeshBounds = true;

        public Mesh() : this("Mesh") { }
        public Mesh(string name)
        {
            Name = name;
        }

        public Result SetIndices(int materialIndex, int[] indices, BeginMode mode)
        {
            if (materialIndex < 0) return Result.OutOfIndex;
            if (indices == null) return Result.ObjectIsNull;
            var sub = subMeshes.Find((s) => { return s.materialIndex == materialIndex; });
            if (sub != null)
            {
                sub.indices = indices;
                sub.mode = mode;
            }
            else subMeshes.Add(new SubMesh()
            {
                materialIndex = materialIndex,
                indices = indices,
                mode = mode
            });
            return Result.Success;
        }

        public int GetMaterialIndex(int index)
        {
            if (index < 0 || index >= SubMeshCount) return -1;
            return subMeshes[index].materialIndex;
        }

        public Result Load()
        {
            Unload();

            if (Vertices == null) return Result.ObjectIsNull;

            for (var i = 0; i < SubMeshCount; i++)
            {
                if (subMeshes[i] == null) return Result.ObjectIsNull;
            }

            if (Normals == null) CalculateNormals();
            if (Tangents == null) CalculateTangents();
            CalculateBounds();

            // Vertex Buffer Objectを生成
            float[] verts = new float[Vertices.Length * VertexFloatSize];
            for (var i = 0; i < Vertices.Length; i++)
            {
                var offset = i * VertexFloatSize;
                // Vertex
                verts[offset++] = Vertices[i].X;
                verts[offset++] = Vertices[i].Y;
                verts[offset++] = Vertices[i].Z;

                // Normal
                verts[offset++] = Normals[i].X;
                verts[offset++] = Normals[i].Y;
                verts[offset++] = Normals[i].Z;

                // Tangent
                if (Tangents != null && i < Tangents.Length)
                {
                    verts[offset++] = Tangents[i].X;
                    verts[offset++] = Tangents[i].Y;
                    verts[offset++] = Tangents[i].Z;
                }
                else
                {
                    verts[offset++] = 1.0f;
                    verts[offset++] = 0.0f;
                    verts[offset++] = 0.0f;
                }

                // Color
                if (Colors != null && i < Colors.Length)
                {
                    verts[offset++] = Colors[i].R;
                    verts[offset++] = Colors[i].G;
                    verts[offset++] = Colors[i].B;
                    verts[offset++] = Colors[i].A;
                }
                else
                {
                    verts[offset++] = 1.0f;
                    verts[offset++] = 1.0f;
                    verts[offset++] = 1.0f;
                    verts[offset++] = 1.0f;
                }

                // UV
                if (UVs != null && i < UVs.Length)
                {
                    verts[offset++] = UVs[i].X;
                    verts[offset++] = UVs[i].Y;
                }
                else
                {
                    verts[offset++] = 0.0f;
                    verts[offset++] = 0.0f;
                }

                // UV1
                if (UV1s != null && i < UV1s.Length)
                {
                    verts[offset++] = UV1s[i].X;
                    verts[offset++] = UV1s[i].Y;
                    verts[offset++] = UV1s[i].Z;
                    verts[offset++] = UV1s[i].W;
                }
                else
                {
                    verts[offset++] = 0.0f;
                    verts[offset++] = 0.0f;
                    verts[offset++] = 0.0f;
                    verts[offset++] = 0.0f;
                }

                // UV2
                if (UV2s != null && i < UV2s.Length)
                {
                    verts[offset++] = UV2s[i].X;
                    verts[offset++] = UV2s[i].Y;
                    verts[offset++] = UV2s[i].Z;
                    verts[offset++] = UV2s[i].W;
                }
                else
                {
                    verts[offset++] = 0.0f;
                    verts[offset++] = 0.0f;
                    verts[offset++] = 0.0f;
                    verts[offset++] = 0.0f;
                }

                // UV3
                if (UV3s != null && i < UV3s.Length)
                {
                    verts[offset++] = UV3s[i].X;
                    verts[offset++] = UV3s[i].Y;
                    verts[offset++] = UV3s[i].Z;
                    verts[offset++] = UV3s[i].W;
                }
                else
                {
                    verts[offset++] = 0.0f;
                    verts[offset++] = 0.0f;
                    verts[offset++] = 0.0f;
                    verts[offset++] = 0.0f;
                }

                // UV4
                if (UV4s != null && i < UV4s.Length)
                {
                    verts[offset++] = UV4s[i].X;
                    verts[offset++] = UV4s[i].Y;
                    verts[offset++] = UV4s[i].Z;
                    verts[offset++] = UV4s[i].W;
                }
                else
                {
                    verts[offset++] = 0.0f;
                    verts[offset++] = 0.0f;
                    verts[offset++] = 0.0f;
                    verts[offset++] = 0.0f;
                }

                // Bone Weight
                if (BoneWeights != null && i < BoneWeights.Length)
                {
                    verts[offset++] = BoneWeights[i].boneIndex0;
                    verts[offset++] = BoneWeights[i].boneIndex1;
                    verts[offset++] = BoneWeights[i].boneIndex2;
                    verts[offset++] = BoneWeights[i].boneIndex3;
                    verts[offset++] = BoneWeights[i].weight0;
                    verts[offset++] = BoneWeights[i].weight1;
                    verts[offset++] = BoneWeights[i].weight2;
                    verts[offset++] = BoneWeights[i].weight3;
                }
                else
                {
                    verts[offset++] = 0.0f;
                    verts[offset++] = 0.0f;
                    verts[offset++] = 0.0f;
                    verts[offset++] = 0.0f;
                    verts[offset++] = 1.0f;
                    verts[offset++] = 0.0f;
                    verts[offset++] = 0.0f;
                    verts[offset++] = 0.0f;
                }

                // Index (モーフィング等用)
                verts[offset++] = i;
            }

            GL.GenBuffers(1, out vbo);
            MakeBuffer(vbo, BufferTarget.ArrayBuffer, verts);

            for (var i = 0; i < SubMeshCount; i++)
            {
                // Index Buffer Objectを生成
                GL.GenBuffers(1, out subMeshes[i].ibo);
                MakeBuffer(subMeshes[i].ibo, BufferTarget.ElementArrayBuffer, subMeshes[i].indices);

                // Vertex Array Objectにvbo, iboを登録
                GL.GenVertexArrays(1, out subMeshes[i].vao);
                GL.BindVertexArray(subMeshes[i].vao);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                {
                    var offset = 0;
                    var size = VertexFloatSize * 4;
                    // Vertex
                    GL.EnableVertexAttribArray(0);
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, size, offset);
                    // Normal
                    GL.EnableVertexAttribArray(1);
                    GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, size, offset += Vector3.SizeInBytes);
                    // Tangent
                    GL.EnableVertexAttribArray(2);
                    GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, size, offset += Vector3.SizeInBytes);
                    // Color
                    GL.EnableVertexAttribArray(3);
                    GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, size, offset += Vector3.SizeInBytes);
                    // UV
                    GL.EnableVertexAttribArray(4);
                    GL.VertexAttribPointer(4, 2, VertexAttribPointerType.Float, false, size, offset += Vector4.SizeInBytes);
                    // UV1
                    GL.EnableVertexAttribArray(5);
                    GL.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, size, offset += Vector2.SizeInBytes);
                    // UV2
                    GL.EnableVertexAttribArray(6);
                    GL.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, size, offset += Vector4.SizeInBytes);
                    // UV3
                    GL.EnableVertexAttribArray(7);
                    GL.VertexAttribPointer(7, 4, VertexAttribPointerType.Float, false, size, offset += Vector4.SizeInBytes);
                    // UV4
                    GL.EnableVertexAttribArray(8);
                    GL.VertexAttribPointer(8, 4, VertexAttribPointerType.Float, false, size, offset += Vector4.SizeInBytes);
                    // Bone Index
                    GL.EnableVertexAttribArray(9);
                    GL.VertexAttribPointer(9, 4, VertexAttribPointerType.Float, false, size, offset += Vector4.SizeInBytes);
                    // Bone Weight
                    GL.EnableVertexAttribArray(10);
                    GL.VertexAttribPointer(10, 4, VertexAttribPointerType.Float, false, size, offset += Vector4.SizeInBytes);
                    // Index
                    GL.EnableVertexAttribArray(11);
                    GL.VertexAttribPointer(11, 1, VertexAttribPointerType.Float, false, size, offset += Vector4.SizeInBytes);
                }
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, subMeshes[i].ibo);
                GL.BindVertexArray(0);

                //if (createSubmeshBounds)
                //{
                    // オクルージョン用バウンズを作成
                    //subMeshes[i].bounds = CreateBounds(subMeshes[i].indices);
                    // サブメッシュバウンズを作成
                    //subMeshes[i].boundsMesh = CreateSimpleBoxMesh(subMeshes[i].bounds.Extents, subMeshes[i].bounds.Center);
                //}
            }

            Loaded = true;
            return Result.Success;
        }

        private void MakeBuffer<T>(int obj, BufferTarget target, T[] array) where T : struct
        {
            GL.BindBuffer(target, obj);
            var size = array.Length * Marshal.SizeOf(typeof(T));
            GL.BufferData(target, new IntPtr(size), array, BufferUsageHint.StaticDraw);
            GL.BindBuffer(target, 0);
        }

        public Result CalculateNormals()
        {
            if (Vertices == null) return Result.ObjectIsNull;

            for (var i = 0; i < SubMeshCount; i++)
            {
                if (subMeshes[i] == null) return Result.ObjectIsNull;
            }

            Vector3[] normals = new Vector3[Vertices.Length];
            for (var i = 0; i < normals.Length; i++) normals[i] = Vector3.Zero;

            for (var idx = 0; idx < SubMeshCount; idx++)
            {
                var indices = subMeshes[idx].indices;
                var triangleNum = indices.Length / 3;
                for (var i = 0; i < triangleNum; i++)
                {
                    var offset = i * 3;
                    var i0 = indices[offset + 0];
                    var i1 = indices[offset + 2];
                    var i2 = indices[offset + 1];
                    var v0 = Vertices[i0];
                    var v1 = Vertices[i1];
                    var v2 = Vertices[i2];

                    var d0 = v1 - v0;
                    var d1 = v2 - v0;
                    var norm = Vector3.Cross(d0, d1).Normalized();

                    normals[i0] += norm;
                    normals[i1] += norm;
                    normals[i2] += norm;
                }
            }

            for (var i = 0; i < normals.Length; i++) normals[i].Normalize();

            Normals = normals;
            return Result.Success;
        }

        public Result CalculateTangents()
        {
            if (Vertices == null) return Result.ObjectIsNull;
            if (UVs == null) return Result.ObjectIsNull;
            if (Normals == null) return Result.ObjectIsNull;

            for (var i = 0; i < SubMeshCount; i++)
            {
                if (subMeshes[i] == null) return Result.ObjectIsNull;
            }

            Vector3[] tangents = new Vector3[Vertices.Length];

            for (var idx = 0; idx < SubMeshCount; idx++)
            {
                var indices = subMeshes[idx].indices;
                var triangleNum = indices.Length / 3;
                for (var i = 0; i < triangleNum; i++)
                {
                    var offset = i * 3;
                    var i0 = indices[offset + 0];
                    var i1 = indices[offset + 2];
                    var i2 = indices[offset + 1];
                    var v0 = Vertices[i0];
                    var v1 = Vertices[i1];
                    var v2 = Vertices[i2];
                    var uv0 = UVs[i0];
                    var uv1 = UVs[i1];
                    var uv2 = UVs[i2];

                    var tan = calcTangent(ref v0, ref v1, ref v2, ref uv0, ref uv1, ref uv2);
                    tangents[i0] += tan;
                    tangents[i1] += tan;
                    tangents[i2] += tan;
                }
            }

            for (var i = 0; i < tangents.Length; i++) tangents[i].Normalize();

            Tangents = tangents;
            return Result.Success;
        }

        private Vector3 calcTangent(ref Vector3 v0, ref Vector3 v1, ref Vector3 v2, ref Vector2 uv0, ref Vector2 uv1, ref Vector2 uv2)
        {
            var cp0 = new Vector3[]
            {
                new Vector3(v0.X, uv0.X, uv0.Y),
                new Vector3(v0.Y, uv0.X, uv0.Y),
                new Vector3(v0.Z, uv0.X, uv0.Y),
            };
            var cp1 = new Vector3[]
            {
                new Vector3(v1.X, uv1.X, uv1.Y),
                new Vector3(v1.Y, uv1.X, uv1.Y),
                new Vector3(v1.Z, uv1.X, uv1.Y),
            };
            var cp2 = new Vector3[]
            {
                new Vector3(v2.X, uv2.X, uv2.Y),
                new Vector3(v2.Y, uv2.X, uv2.Y),
                new Vector3(v2.Z, uv2.X, uv2.Y),
            };

            // 平面パラメータからUV軸座標算出
            float[] u = new float[3];
            float[] v = new float[3];

            for (int i = 0; i < 3; ++i)
            {
                var V1 = cp1[i] - cp0[i];
                var V2 = cp2[i] - cp1[i];
                var abc = Vector3.Cross(V1, V2);

                if (abc.X == 0.0f) return Vector3.UnitX;


                u[i] = -abc.Y / abc.X;
                v[i] = -abc.Z / abc.X;
            }

            return new Vector3(u[0], u[1], u[2]).Normalized();
        }

        public Result CalculateBounds()
        {
            if (Vertices == null) return Result.ObjectIsNull;

            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);

            foreach (var vert in Vertices)
            {
                if (vert.X < min.X) min.X = vert.X;
                if (vert.Y < min.Y) min.Y = vert.Y;
                if (vert.Z < min.Z) min.Z = vert.Z;
                if (vert.X > max.X) max.X = vert.X;
                if (vert.Y > max.Y) max.Y = vert.Y;
                if (vert.Z > max.Z) max.Z = vert.Z;
            }

            var bounds = new Bounds()
            {
                Center = Vector3.Lerp(min, max, 0.5f),
                Extents = (max - min) * 0.5f,
            };
            Bounds = bounds;

            return Result.Success;
        }

        public Bounds CreateBounds(int[] indices)
        {
            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);

            foreach (var i in indices)
            {
                var vert = Vertices[i];

                if (vert.X < min.X) min.X = vert.X;
                if (vert.Y < min.Y) min.Y = vert.Y;
                if (vert.Z < min.Z) min.Z = vert.Z;
                if (vert.X > max.X) max.X = vert.X;
                if (vert.Y > max.Y) max.Y = vert.Y;
                if (vert.Z > max.Z) max.Z = vert.Z;
            }

            return new Bounds()
            {
                Center = Vector3.Lerp(min, max, 0.5f),
                Extents = (max - min) * 0.5f,
            };
        }

        public Result Unload()
        {
            if (!Loaded) return Result.NotLoaded;

            for (var i = 0; i < SubMeshCount; i++)
            {
                GL.DeleteBuffer(subMeshes[i].vao);
                subMeshes[i].vao = -1;
                GL.DeleteBuffer(subMeshes[i].ibo);
                subMeshes[i].ibo = -1;
            }
            
            GL.DeleteBuffer(vbo);
            vbo = -1;

            Loaded = false;
            return Result.Success;
        }

        public override string ToString()
        {
            return string.Format("{0}: VertNum:{1}, SubMeshNum:{2}", Name, Vertices != null ? Vertices.Length : 0, SubMeshCount);
        }

        public static int[] ModifyIndices(int[] srcIndices, BeginMode srcMode, BeginMode dstMode)
        {
            if (srcMode == BeginMode.Triangles && dstMode == BeginMode.Lines)
            {
                var indices = new int[srcIndices.Length * 2];
                for (var i = 0; i < srcIndices.Length / 3; i++)
                {
                    indices[i * 6 + 0] = srcIndices[i * 3 + 0];
                    indices[i * 6 + 1] = srcIndices[i * 3 + 1];
                    indices[i * 6 + 2] = srcIndices[i * 3 + 1];
                    indices[i * 6 + 3] = srcIndices[i * 3 + 2];
                    indices[i * 6 + 4] = srcIndices[i * 3 + 2];
                    indices[i * 6 + 5] = srcIndices[i * 3 + 0];
                }
                return indices;
            }
            if (srcMode == BeginMode.Lines && dstMode == BeginMode.Triangles)
            {
                var indices = new int[srcIndices.Length / 2];
                for (var i = 0; i < srcIndices.Length / 6; i++)
                {
                    indices[i * 3 + 0] = srcIndices[i * 6 + 0];
                    indices[i * 3 + 1] = srcIndices[i * 6 + 2];
                    indices[i * 3 + 2] = srcIndices[i * 6 + 4];
                }
                return indices;
            }
            if (srcMode == BeginMode.Quads && dstMode == BeginMode.Lines)
            {
                var indices = new int[srcIndices.Length * 2];
                for (var i = 0; i < srcIndices.Length / 4; i++)
                {
                    indices[i * 8 + 0] = srcIndices[i * 4 + 0];
                    indices[i * 8 + 1] = srcIndices[i * 4 + 1];
                    indices[i * 8 + 2] = srcIndices[i * 4 + 1];
                    indices[i * 8 + 3] = srcIndices[i * 4 + 2];
                    indices[i * 8 + 4] = srcIndices[i * 4 + 2];
                    indices[i * 8 + 5] = srcIndices[i * 4 + 3];
                    indices[i * 8 + 6] = srcIndices[i * 4 + 3];
                    indices[i * 8 + 7] = srcIndices[i * 4 + 0];
                }
                return indices;
            }
            throw new NotImplementedException();
        }

        public static Mesh CreateSimpleBoxMesh(Vector3 halfExtents, bool wireframeMesh = false)
        {
            var mesh = new Mesh("Simple Box Mesh");
            mesh.Vertices = new Vector3[]
            {
                new Vector3(-halfExtents.X, -halfExtents.Y,  halfExtents.Z),
                new Vector3(-halfExtents.X,  halfExtents.Y,  halfExtents.Z),
                new Vector3( halfExtents.X,  halfExtents.Y,  halfExtents.Z),
                new Vector3( halfExtents.X, -halfExtents.Y,  halfExtents.Z),
                new Vector3(-halfExtents.X, -halfExtents.Y, -halfExtents.Z),
                new Vector3(-halfExtents.X,  halfExtents.Y, -halfExtents.Z),
                new Vector3( halfExtents.X,  halfExtents.Y, -halfExtents.Z),
                new Vector3( halfExtents.X, -halfExtents.Y, -halfExtents.Z),
            };
            if (!wireframeMesh)
            {
                mesh.SetIndices(0, new int[]
                {
                    0, 1, 2,
                    0, 2, 3,
                    3, 2, 6,
                    3, 6, 7,
                    7, 6, 5,
                    7, 5, 4,
                    4, 5, 1,
                    4, 1, 0,
                    1, 5, 6,
                    1, 6, 2,
                    4, 0, 3,
                    4, 3, 7,
                }, BeginMode.Triangles);
            }
            else
            {
                mesh.SetIndices(0, new int[]
                {
                    0, 1, 1, 2, 2, 3, 3, 0,
                    4, 5, 5, 6, 6, 7, 7, 4,
                    0, 4, 1, 5, 2, 6, 3, 7,
                }, BeginMode.Lines);
            }
            mesh.Load();
            return mesh;
        }
        public static Mesh CreateSimpleBoxMesh(Vector3 halfExtents, Vector3 location, bool wireframeMesh = false)
        {
            var mesh = new Mesh("Simple Box Mesh");
            mesh.Vertices = new Vector3[]
            {
                new Vector3(-halfExtents.X, -halfExtents.Y,  halfExtents.Z),
                new Vector3(-halfExtents.X,  halfExtents.Y,  halfExtents.Z),
                new Vector3( halfExtents.X,  halfExtents.Y,  halfExtents.Z),
                new Vector3( halfExtents.X, -halfExtents.Y,  halfExtents.Z),
                new Vector3(-halfExtents.X, -halfExtents.Y, -halfExtents.Z),
                new Vector3(-halfExtents.X,  halfExtents.Y, -halfExtents.Z),
                new Vector3( halfExtents.X,  halfExtents.Y, -halfExtents.Z),
                new Vector3( halfExtents.X, -halfExtents.Y, -halfExtents.Z),
            };

            for (var i = 0; i < mesh.Vertices.Length; i++) mesh.Vertices[i] += location;

            if (!wireframeMesh)
            {
                mesh.SetIndices(0, new int[]
                {
                    0, 1, 2,
                    0, 2, 3,
                    3, 2, 6,
                    3, 6, 7,
                    7, 6, 5,
                    7, 5, 4,
                    4, 5, 1,
                    4, 1, 0,
                    1, 5, 6,
                    1, 6, 2,
                    4, 0, 3,
                    4, 3, 7,
                }, BeginMode.Triangles);
            }
            else
            {
                mesh.SetIndices(0, new int[]
                {
                    0, 1, 1, 2, 2, 3, 3, 0,
                    4, 5, 5, 6, 6, 7, 7, 4,
                    0, 4, 1, 5, 2, 6, 3, 7,
                }, BeginMode.Lines);
            }
            //mesh.createSubmeshBounds = false;
            mesh.Load();
            return mesh;
        }
        public static Mesh CreateSimplePlaneMesh(float halfExtentsX, float halfExtentsZ, bool wireframeMesh = false)
        {
            var mesh = new Mesh("Simple Plane Mesh");
            mesh.Vertices = new Vector3[]
            {
                new Vector3(-halfExtentsX, 0.0f,  halfExtentsZ),
                new Vector3(-halfExtentsX, 0.0f, -halfExtentsZ),
                new Vector3( halfExtentsX, 0.0f, -halfExtentsZ),
                new Vector3( halfExtentsX, 0.0f,  halfExtentsZ),
            };
            if (!wireframeMesh)
            {
                mesh.SetIndices(0, new int[]
                {
                    0, 1, 2,
                    0, 2, 3,
                }, BeginMode.Triangles);
            }
            else
            {
                mesh.SetIndices(0, new int[]
                {
                    0, 1, 1, 2, 2, 3, 3, 0
                }, BeginMode.Lines);
            }
            mesh.UVs = new Vector2[]
            {
                new Vector2(0.0f, 1.0f),
                new Vector2(0.0f, 0.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(1.0f, 1.0f),
            };
            mesh.Load();
            return mesh;
        }
        public static Mesh CreateSimpleSphereMesh(float radius, int uFace = 8, int vFace = 6, bool wireframeMesh = false)
        {
            var mesh = new Mesh("Simple Sphere Mesh");

            // vertices
            var vertices = new List<Vector3>();
            vertices.Add(new Vector3(0.0f, radius, 0.0f));
            for (var u = 0; u < vFace - 1; u++)
            {
                var vy = (float)Math.Cos(((u + 1) * 180.0 / vFace).ToRadian());
                var sin = (float)Math.Sin(((u + 1) * 180.0 / vFace).ToRadian());
                for (var v = 0; v < uFace; v++)
                {
                    var vx = (float)Math.Cos((v * 360.0 / uFace).ToRadian()) * sin;
                    var vz = (float)Math.Sin((v * 360.0 / uFace).ToRadian()) * sin;
                    vertices.Add(new Vector3(vx, vy, vz) * radius);
                }
            }
            vertices.Add(new Vector3(0.0f, -radius, 0.0f));
            mesh.Vertices = vertices.ToArray();

            // indices
            var indices = new List<int>();
            if (!wireframeMesh)
            {
                // top
                for (var v = 1; v <= uFace; v++)
                {
                    var v1 = v + 1;
                    if (v1 == uFace + 1) v1 = 1;
                    indices.Add(0);
                    indices.Add(v);
                    indices.Add(v1);
                }

                // side
                for (var v = 0; v < vFace - 2; v++)
                {
                    var uoff = v * uFace + 1;
                    for (var u = 0; u < uFace; u++)
                    {
                        var i1 = uoff + uFace + u + 1;
                        if ((i1 - 1) % uFace == 0) i1 = uoff + uFace;
                        var i2 = uoff + u + 1;
                        if ((i2 - 1) % uFace == 0) i2 = uoff;
                        indices.Add(uoff + u);
                        indices.Add(uoff + uFace + u);
                        indices.Add(i1);
                        indices.Add(uoff + u);
                        indices.Add(i1);
                        indices.Add(i2);
                    }
                }

                // bottom
                var end = (uFace * (vFace - 1)) + 1;
                var off = end - uFace;
                for (var v = 0; v < uFace; v++)
                {
                    var off1 = off + v + 1;
                    if (off1 == end) off1 = off;
                    indices.Add(off + v);
                    indices.Add(end);
                    indices.Add(off1);
                }

                mesh.SetIndices(0, indices.ToArray(), BeginMode.Triangles);
            }
            else
            {
                // top
                for (var v = 1; v <= uFace; v++)
                {
                    var v1 = v + 1;
                    if (v1 == uFace + 1) v1 = 1;
                    indices.Add(0);
                    indices.Add(v);
                    indices.Add(v);
                    indices.Add(v1);
                }

                // side
                for (var v = 0; v < vFace - 2; v++)
                {
                    var uoff = v * uFace + 1;
                    for (var u = 0; u < uFace; u++)
                    {
                        var i1 = uoff + uFace + u + 1;
                        if ((i1 - 1) % uFace == 0) i1 = uoff + uFace;
                        var i2 = uoff + u + 1;
                        if ((i2 - 1) % uFace == 0) i2 = uoff;
                        indices.Add(uoff + u);
                        indices.Add(uoff + uFace + u);
                        indices.Add(uoff + uFace + u);
                        indices.Add(i1);
                    }
                }

                // bottom
                var end = (uFace * (vFace - 1)) + 1;
                var off = end - uFace;
                for (var v = 0; v < uFace; v++)
                {
                    var off1 = off + v + 1;
                    if (off1 == end) off1 = off;
                    indices.Add(off + v);
                    indices.Add(end);
                    indices.Add(end);
                    indices.Add(off1);
                }

                mesh.SetIndices(0, indices.ToArray(), BeginMode.Lines);
            }

            mesh.Load();
            return mesh;
        }

        [DataMember(Name = "vertices", EmitDefaultValue = false, Order = 1)]
        private string vertices
        {
            get { if (Vertices == null) return null; return Vertices.ToBase64String(); }
            set { Vertices = value.ToVector3Array(); }
        }

        [DataMember(Name = "normals", EmitDefaultValue = false, Order = 2)]
        private string normals
        {
            get { if (Normals == null) return null; return Normals.ToBase64String(); }
            set { Normals = value.ToVector3Array(); }
        }

        [DataMember(Name = "tangents", EmitDefaultValue = false, Order = 3)]
        private string tangents
        {
            get { if (Tangents == null) return null; return Tangents.ToBase64String(); }
            set { Tangents = value.ToVector3Array(); }
        }

        [DataMember(Name = "colors", EmitDefaultValue = false, Order = 4)]
        private string colors
        {
            get { if (Colors == null) return null; return Colors.ToBase64String(); }
            set { Colors = value.ToColor4Array(); }
        }

        [DataMember(Name = "uvs", EmitDefaultValue = false, Order = 5)]
        private string uvs
        {
            get { if (UVs == null) return null; return UVs.ToBase64String(); }
            set { UVs = value.ToVector2Array(); }
        }

        [DataMember(Name = "uv1s", EmitDefaultValue = false, Order = 6)]
        private string uv1s
        {
            get { if (UV1s == null) return null; return UVs.ToBase64String(); }
            set { UV1s = value.ToVector4Array(); }
        }

        [DataMember(Name = "uv2s", EmitDefaultValue = false, Order = 7)]
        private string uv2s
        {
            get { if (UV2s == null) return null; return UVs.ToBase64String(); }
            set { UV2s = value.ToVector4Array(); }
        }

        [DataMember(Name = "uv3s", EmitDefaultValue = false, Order = 8)]
        private string uv3s
        {
            get { if (UV3s == null) return null; return UVs.ToBase64String(); }
            set { UV3s = value.ToVector4Array(); }
        }

        [DataMember(Name = "uv4s", EmitDefaultValue = false, Order = 9)]
        private string uv4s
        {
            get { if (UV4s == null) return null; return UVs.ToBase64String(); }
            set { UV4s = value.ToVector4Array(); }
        }

        [DataMember(Name = "boneweights", EmitDefaultValue = false, Order = 10)]
        private string boneweights
        {
            get
            {
                if (BoneWeights == null) return null;
                var fs = new List<float>();
                foreach (var w in BoneWeights) fs.AddRange(w.ToFloats());
                return fs.ToArray().ToBase64String();
            }
            set
            {
                var fs = value.ToFloats();
                BoneWeights = new BoneWeight[fs.Length / 8];
                for (var i = 0; i < BoneWeights.Length; i++)
                {
                    BoneWeights[i].boneIndex0 = (int)fs[i * 8 + 0];
                    BoneWeights[i].boneIndex1 = (int)fs[i * 8 + 1];
                    BoneWeights[i].boneIndex2 = (int)fs[i * 8 + 2];
                    BoneWeights[i].boneIndex3 = (int)fs[i * 8 + 3];
                    BoneWeights[i].weight0 = fs[i * 8 + 4];
                    BoneWeights[i].weight1 = fs[i * 8 + 5];
                    BoneWeights[i].weight2 = fs[i * 8 + 6];
                    BoneWeights[i].weight3 = fs[i * 8 + 7];
                }
            }
        }
    }

    [DataContract]
    public class SubMesh
    {
        [DataMember(Name = "material_index", Order = 0)]
        public int materialIndex;

        [DataMember(Order = 1)]
        public BeginMode mode;

        [DataMember(EmitDefaultValue = false, Order = 2)]
        public int[] indices;

        internal int ibo = -1;
        internal int vao = -1;
    }
}
