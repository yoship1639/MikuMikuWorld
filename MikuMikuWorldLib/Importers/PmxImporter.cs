using MikuMikuWorld.Assets;
using MikuMikuWorld.Properties;
using OpenTK.Graphics;
using PmxModelImporter;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MikuMikuWorld.Importers
{
    public class PmxImporter : IImporter
    {
        public bool DirectoryImporter { get { return false; } }

        public string[] Extensions { get { return new string[] { ".pmx" }; } }

        public float ImportScale { get; set; } = 0.0795f;

        public ImportedObject[] Import(string path, ImportType type)
        {
            ImportedObject obj = new ImportedObject()
            {
                Result = Result.Failed,
                Type = ImportedObjectType.Model,
                Path = path,
            };

            PmxImportResult res;
            {
                var importer = new PmxModelImporter.PmxModelImporter();
                res = importer.Import(path, type == ImportType.Full);
                if (res == null || res.result != PmxImportResult.Result.Success)
                {
                    return new ImportedObject[] { obj };
                }
            }
            
            var pmx = res.pmx;

            obj.Result = Result.Success;
            obj.Name = pmx.ModelInfo.ModelName;
            obj.Version = pmx.Header.Version.ToString();
            obj.Description = pmx.ModelInfo.Comment;

            if (type == ImportType.OverviewOnly)
            {
                obj.Result = Result.Success;
                return new ImportedObject[] { obj };
            }

            // texture
            var textures = new Texture2D[pmx.TextureList.TextureNum];
            for (var i = 0; i < pmx.TextureList.TextureNum; i++)
            {
                var texname = pmx.TextureList.Textures[i];
                if (string.IsNullOrEmpty(texname)) continue;

                var texPath = Path.GetDirectoryName(path) + "\\" + texname;

                if (!File.Exists(texPath)) continue;

                Texture2D difMap = null;
                Bitmap bitmap = null;
                try
                {
                    bitmap = (Bitmap)Image.FromFile(texPath);
                }
                catch
                {
                    var ext = Path.GetExtension(texPath).ToLower();
                    if (ext == ".tga")
                    {
                        using (FileStream fs = new FileStream(texPath, FileMode.Open))
                        {
                            using (BinaryReader br = new BinaryReader(fs))
                            {
                                var tga = new TgaLib.TgaImage(br);
                                bitmap = tga.GetBitmap().ToBitmap(PixelFormat.Format32bppArgb);
                            }
                        }
                    }
                }
                difMap = new Texture2D(bitmap, Path.GetFileNameWithoutExtension(texPath), false);
                difMap.UseMipmap = true;
                difMap.WrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat;
                difMap.MinFilter = OpenTK.Graphics.OpenGL4.TextureMinFilter.LinearMipmapLinear;
                textures[i] = difMap;
            }
            obj.Textures = textures;

            // material
            var materials = new List<Material>();
            var colliders = new List<int>();
            for (var i = 0; i < pmx.MaterialList.MaterialNum; i++)
            {
                var mat = pmx.MaterialList.Materials[i];

                // 当たり判定用マテリアル
                if (mat.Name.Contains("collider")) colliders.Add(i);

                var m = new Material(mat.Name);
                m.AddParam("albedo", "Albedo", mat.Diffuse.ToColor4());
                //m.AddParam("specular", "Specular", mat.Specular.ToColor4());
                //m.AddParam("shininess", "Shininess", mat.SpecularPower);
                m.AddParam("emissive", "Emissive", new Color4(0, 0, 0, 0));
                //m.AddParam("ambient", "Ambient", mat.Ambient.ToColor4());

                m.AddParam("roughness", "Roughness", 1.0f - MMWMath.Clamp(mat.SpecularPower / 32.0f, 0.0f, 1.0f));
                m.AddParam("metallic", "Metallic", mat.SpecularPower > 14.0f ? 1.0f : 0.0f);
                m.AddParam("reflectance", "Reflectance", 0.0f);
                //m.AddParam("f0", "F0", (mat.Specular + new Vector3(0.15f, 0.15f, 0.15f)).ToColor4());
                //m.AddParam("f0", "F0", mat.Specular.ToColor4());
                
                if (mat.AlbedoMapIndex != -1)
                {
                    m.AddParam("albedoMap", "AlbedoMap", textures[mat.AlbedoMapIndex]);
                }
                else m.AddParam<Texture2D>("albedoMap", "AlbedoMap", null);
                m.AddParam<Texture2D>("specularMap", "SpecularMap", null);
                m.AddParam<Texture2D>("normalMap", "NormalMap", null);
                m.AddParam<Texture2D>("physicalMap", "PhysicalMap", null);

                materials.Add(m);
            }
            obj.Materials = materials.ToArray();

            // mesh
            var mesh = new Mesh(pmx.ModelInfo.ModelName);
            var collideMesh = new Mesh("collider");
            {
                mesh.Vertices = new OpenTK.Vector3[pmx.VertexList.VertexNum];
                mesh.UVs = new OpenTK.Vector2[pmx.VertexList.VertexNum];
                mesh.Normals = new OpenTK.Vector3[pmx.VertexList.VertexNum];
                mesh.BoneWeights = new BoneWeight[pmx.VertexList.VertexNum];

                // 追加UV
                for (var i = 0; i < pmx.Header.AddUVCound; i++)
                {
                    if (i == 0) mesh.UV1s = new OpenTK.Vector4[pmx.VertexList.VertexNum];
                    if (i == 1) mesh.UV2s = new OpenTK.Vector4[pmx.VertexList.VertexNum];
                    if (i == 2) mesh.UV3s = new OpenTK.Vector4[pmx.VertexList.VertexNum];
                    if (i == 3) mesh.UV4s = new OpenTK.Vector4[pmx.VertexList.VertexNum];
                }

                for (var i = 0; i < pmx.VertexList.VertexNum; i++)
                {
                    var vert = pmx.VertexList.Vertices[i];
                    var v = vert.Position.ToVec3(true) * ImportScale;
                    mesh.Vertices[i] = v;
                    mesh.UVs[i] = vert.UV.ToVec2();
                    mesh.Normals[i] = vert.Normal.ToVec3(true);

                    for (var j = 0; j < pmx.Header.AddUVCound; j++)
                    {
                        if (j == 0) mesh.UV1s[i] = vert.UV1.ToVec4();
                        if (j == 1) mesh.UV2s[i] = vert.UV2.ToVec4();
                        if (j == 2) mesh.UV3s[i] = vert.UV3.ToVec4();
                        if (j == 3) mesh.UV4s[i] = vert.UV4.ToVec4();
                    }
                    
                    mesh.BoneWeights[i].boneIndex0 = vert.BoneIndex0;
                    mesh.BoneWeights[i].boneIndex1 = vert.BoneIndex1;
                    mesh.BoneWeights[i].boneIndex2 = vert.BoneIndex2;
                    mesh.BoneWeights[i].boneIndex3 = vert.BoneIndex3;
                    mesh.BoneWeights[i].weight0 = vert.Weight0;
                    mesh.BoneWeights[i].weight1 = vert.Weight1;
                    mesh.BoneWeights[i].weight2 = vert.Weight2;
                    mesh.BoneWeights[i].weight3 = vert.Weight3;
                }
                collideMesh.Vertices = mesh.Vertices;

                var offset = 0;
                for (var i = 0; i < pmx.MaterialList.MaterialNum; i++)
                {
                    var mat = pmx.MaterialList.Materials[i];
                    var indices = new int[mat.FaceVertNum];
                    for (var j = 0; j < mat.FaceVertNum / 3; j++)
                    {
                        indices[j * 3 + 0] = pmx.FaceList.Faces[offset++];
                        indices[j * 3 + 1] = pmx.FaceList.Faces[offset++];
                        indices[j * 3 + 2] = pmx.FaceList.Faces[offset++];
                    }
                    if (mat.FaceVertNum > 0)
                    {
                        if (colliders.Exists((c) => c == i))
                        {
                            collideMesh.SetIndices(i, indices, OpenTK.Graphics.OpenGL4.BeginMode.Triangles);
                        }
                        else mesh.SetIndices(i, indices, OpenTK.Graphics.OpenGL4.BeginMode.Triangles);
                    }
                }
            }
            var meshes = new List<Mesh>();
            meshes.Add(mesh);
            if (collideMesh.SubMeshCount > 0) meshes.Add(collideMesh);
            obj.Meshes = meshes.ToArray();

            // bone
            if (pmx.BoneList != null && pmx.BoneList.BoneNum > 0)
            {
                obj.Bones = CreateBones(pmx.BoneList.Bones);
                BindBones(obj.Bones, pmx.BoneList.Bones);
            }

            // morphs
            var morphs = new List<Morph>();
            // 頂点モーフ
            var vs = CreateVertexMorphs(pmx.MorphList.VertexList);
            if (vs != null) morphs.AddRange(vs);
            // ボーンモーフ
            var bs = CreateBoneMorphs(pmx.MorphList.BoneList);
            if (bs != null) morphs.AddRange(bs);

            obj.Morphs = morphs.ToArray();

            return new ImportedObject[] { obj };
        }

        private void BindBones(Bone[] bones, PmxBone[] pmxBones)
        {
            for (var i = 0; i < bones.Length; i++)
            {
                if (pmxBones[i].ParentBoneIndex != -1) bones[i].Parent = bones[pmxBones[i].ParentBoneIndex];
            }

            for (var i = 0; i < bones.Length; i++)
            {
                bones[i].Children = Array.FindAll(bones, b => b.Parent == bones[i]);
                if (bones[i].Children.Length == 0) bones[i].Children = null;
            }

            for (var i = 0; i < bones.Length; i++)
            {
                if (bones[i].BoneType != "IK") continue;

                bones[i].IKLoop = pmxBones[i].IKLoop;
                if (pmxBones[i].IKBoneIndex >= 0) bones[i].IKTarget = bones[pmxBones[i].IKBoneIndex];
                bones[i].IKRotLimit = pmxBones[i].IKLoopLimitAngle;

                if (pmxBones[i].IKLinkNum == 0) continue;

                bones[i].IKLinks = new IKLink[pmxBones[i].IKLinkNum];
                for (var j = 0; j < pmxBones[i].IKLinkNum; j++)
                {
                    bones[pmxBones[i].IKLinks[j].BoneIndex].BoneType = "UnderIK";
                    bones[i].IKLinks[j] = new IKLink();
                    bones[i].IKLinks[j].Bone = bones[pmxBones[i].IKLinks[j].BoneIndex];
                    bones[i].IKLinks[j].LimitAngle = pmxBones[i].IKLinks[j].LimitAngle;
                    bones[i].IKLinks[j].LowerLimitAngle = -pmxBones[i].IKLinks[j].UpperLimitAngle.ToVec3();
                    bones[i].IKLinks[j].UpperLimitAngle = -pmxBones[i].IKLinks[j].LowerLimitAngle.ToVec3();
                }
                
            }
        }

        private Bone[] CreateBones(PmxBone[] pmxBones)
        {
            var bones = new Bone[pmxBones.Length];
            for (var i = 0; i < bones.Length; i++)
            {
                var b = new Bone()
                {
                    Name = pmxBones[i].BoneName,
                    Index = i,
                    Position = pmxBones[i].Position.ToVec3(true) * ImportScale,
                    
                };
                b.BoneType = pmxBones[i].IsIK ? "IK" : "Standard";
                bones[i] = b;
            }
            return bones;
        }

        /*
        private Bone CreateBones(PmxBone[] bones, Bone parent = null)
        {
            if (parent == null)
            {
                var root = Array.Find(bones, (b) => b.ParentBoneIndex == -1);
                parent = new Bone()
                {
                    Name = root.BoneName,
                    Index = Array.IndexOf(bones, root),
                    BoneType = BoneType.Standard,
                };
                if (root.IsIK) parent.BoneType = BoneType.IK;

                parent.Position = root.Position.ToVec3(true) * ImportScale;

                //if (root.FixedAxis) parent.FixedAxis = root.Axis.ToVec3(true);
                if (root.LocalAxis)
                {
                    //parent.AxisX = root.AxisX.ToVec3(true);
                    //parent.AxisZ = root.AxisZ.ToVec3(true);
                }
            }

            var child = Array.FindAll(bones, (b) => b.ParentBoneIndex == parent.Index);
            if (child != null && child.Length > 0)
            {
                var children = new Bone[child.Length];
                for (var i = 0; i < child.Length; i++)
                {
                    children[i] = new Bone()
                    {
                        Name = child[i].BoneName,
                        Index = Array.IndexOf(bones, child[i]),
                        Parent = parent,
                        BoneType = BoneType.Standard,
                    };
                    if (child[i].IsIK) children[i].BoneType = BoneType.IK;

                    children[i].Position = child[i].Position.ToVec3(true) * ImportScale;

                    //if (child[i].FixedAxis) children[i].FixedAxis = child[i].Axis.ToVec3(true);
                    if (child[i].LocalAxis)
                    {
                        //children[i].AxisX = child[i].AxisX.ToVec3(true);
                        //children[i].AxisZ = child[i].AxisZ.ToVec3(true);
                    }

                    CreateBones(bones, children[i]);
                }
                parent.Children = children;
            }

            return parent;
        }*/

        private Morph[] CreateVertexMorphs(PmxMorph<PmxMorphVertex>[] vs)
        {
            if (vs == null) return null;
            var list = new List<Morph>();
            foreach (var v in vs)
            {
                var m = new Morph()
                {
                    Name = v.Name,
                };

                m.Vertices = new VertexMorph[v.Data.Length];
                for (var i = 0; i < m.Vertices.Length; i++)
                {
                    m.Vertices[i] = new VertexMorph();
                    m.Vertices[i].Index = v.Data[i].Index;
                    m.Vertices[i].Offset = v.Data[i].Position.ToVec3(true) * ImportScale;
                }
                list.Add(m);
            }
            return list.ToArray();
        }

        private Morph[] CreateBoneMorphs(PmxMorph<PmxMorphBone>[] bs)
        {
            if (bs == null) return null;
            var list = new List<Morph>();
            foreach (var b in bs)
            {
                var m = new Morph() { Name = b.Name };

                m.Bones = new BoneMorph[b.Data.Length];
                for (var i = 0; i < m.Bones.Length; i++)
                {
                    m.Bones[i] = new BoneMorph();
                    m.Bones[i].Index = b.Data[i].Index;
                    m.Bones[i].Location = b.Data[i].Translation.ToVec3(true) * ImportScale;
                    m.Bones[i].Rotation = b.Data[i].Quaternion.ToQuaternion();
                }
                list.Add(m);
            }

            return list.ToArray();
        }
    }

    static class PmxExtension
    {
        public static OpenTK.Vector2 ToVec2(this Vector2 v)
        {
            return new OpenTK.Vector2(v.X, v.Y);
        }

        public static OpenTK.Vector3 ToVec3(this Vector3 v, bool flipZ = false)
        {
            return new OpenTK.Vector3(v.X, v.Y, flipZ ? -v.Z : v.Z);
        }

        public static OpenTK.Vector4 ToVec4(this Vector4 v)
        {
            return new OpenTK.Vector4(v.X, v.Y, v.Z, v.W);
        }

        public static OpenTK.Quaternion ToQuaternion(this Vector4 v, bool flipZ = false)
        {
            return new OpenTK.Quaternion(-v.X, -v.Y, flipZ ? -v.Z : v.Z, v.W);
        }

        public static Color4 ToColor4(this Vector4 v)
        {
            return new Color4(v.X, v.Y, v.Z, v.W);
        }

        public static Color4 ToColor4(this Vector3 v, float alpha = 1.0f)
        {
            return new Color4(v.X, v.Y, v.Z, alpha);
        }
    }
}
