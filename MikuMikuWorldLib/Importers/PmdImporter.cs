using MikuMikuWorld.Assets;
using OpenTK.Graphics;
using PmdModelImporter;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MikuMikuWorld.Importers
{
    public class PmdImporter : IImporter
    {
        public bool DirectoryImporter { get { return false; } }

        public string[] Extensions { get { return new string[] { ".pmd" }; } }

        public float ImportScale { get; set; } = 0.0795f;

        public ImportedObject[] Import(string path, ImportType type)
        {
            ImportedObject obj = new ImportedObject()
            {
                Result = Result.Failed,
                Type = ImportedObjectType.Model,
                Path = path,
            };

            PmdImportResult res;
            {
                var importer = new PmdModelImporter.PmdModelImporter();
                res = importer.Import(path, type == ImportType.Full);
                if (res == null || res.result != PmdImportResult.Result.Success)
                {
                    return new ImportedObject[] { obj };
                }
            }

            var pmd = res.pmd;

            obj.Result = Result.Success;
            obj.Name = pmd.Header.Name;
            obj.Version = pmd.Header.Version.ToString();
            obj.Description = pmd.Header.Comment;

            if (type == ImportType.OverviewOnly)
            {
                obj.Result = Result.Success;
                return new ImportedObject[] { obj };
            }

            // texture
            var texturePathes = new List<string>();
            var textures = new List<Texture2D>();

            // material
            var materials = new List<Material>();
            for (var i = 0; i < pmd.MaterialList.MaterialNum; i++)
            {
                var mat = pmd.MaterialList.Materials[i];
                var m = new Material();
                m.AddParam("albedo", "Albedo", mat.DiffuseColor.ToColor4(mat.Alpha));
                //m.AddParam("specular", "Specular", mat.SpecularColor.ToColor4());
                //m.AddParam("shininess", "Shininess", mat.SpecularPower);
                m.AddParam("emissive", "Emissive", new Color4(0, 0, 0, 0));
               // m.AddParam("ambient", "Ambient", mat.AmbientColor.ToColor4());

                m.AddParam("roughness", "Roughness", 1.0f - MMWMath.Clamp(mat.SpecularPower / 32.0f, 0.0f, 1.0f));
                m.AddParam("metallic", "Metallic", mat.SpecularPower > 14.0f ? 1.0f : 0.0f);
                m.AddParam("reflectance", "Reflectance", 0.0f);
                //m.AddParam("f0", "F0", (mat.SpecularColor + new Vector3(0.15f, 0.15f, 0.15f)).ToColor4());
                //m.AddParam("f0", "F0", mat.SpecularColor.ToColor4());

                if (!string.IsNullOrEmpty(mat.TextureFileName))
                {
                    mat.TextureFileName = Path.GetDirectoryName(path) + "\\" + mat.TextureFileName;

                    if (File.Exists(mat.TextureFileName))
                    {
                        Texture2D difMap = null;
                        var index = texturePathes.IndexOf(mat.TextureFileName);
                        if (index == -1)
                        {
                            Bitmap bitmap = null;
                            try
                            {
                                bitmap = (Bitmap)Image.FromFile(mat.TextureFileName);
                            }
                            catch
                            {
                                var ext = Path.GetExtension(mat.TextureFileName).ToLower();
                                if (ext == ".tga")
                                {
                                    using (FileStream fs = new FileStream(mat.TextureFileName, FileMode.Open))
                                    {
                                        using (BinaryReader br = new BinaryReader(fs))
                                        {
                                            var tga = new TgaLib.TgaImage(br);
                                            bitmap = tga.GetBitmap().ToBitmap(PixelFormat.Format32bppPArgb);
                                        }  
                                    }
                                }
                            }
                            difMap = new Texture2D(bitmap, Path.GetFileNameWithoutExtension(mat.TextureFileName), false);
                            difMap.UseMipmap = true;
                            difMap.WrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat;
                            difMap.MinFilter = OpenTK.Graphics.OpenGL4.TextureMinFilter.LinearMipmapLinear;
                            textures.Add(difMap);
                            texturePathes.Add(mat.TextureFileName);
                        }
                        else difMap = textures[index];
                        m.AddParam("albedoMap", "AlbedoMap", difMap);
                    }
                }
                else m.AddParam<Texture2D>("albedoMap", "AlbedoMap", null);
                m.AddParam<Texture2D>("specularMap", "SpecularMap", null);
                m.AddParam<Texture2D>("normalMap", "NormalMap", null);
                m.AddParam<Texture2D>("physicalMap", "PhysicalMap", null);

                materials.Add(m);
            }
            obj.Materials = materials.ToArray();
            obj.Textures = textures.ToArray();

            // mesh
            var mesh = new Mesh(pmd.Header.Name);
            {
                mesh.Vertices = new OpenTK.Vector3[pmd.VertexList.VertexNum];
                mesh.UVs = new OpenTK.Vector2[pmd.VertexList.VertexNum];
                mesh.Normals = new OpenTK.Vector3[pmd.VertexList.VertexNum];
                mesh.BoneWeights = new BoneWeight[pmd.VertexList.VertexNum];

                for (var i = 0; i < pmd.VertexList.VertexNum; i++)
                {
                    var vert = pmd.VertexList.Vertices[i];
                    var v = vert.Position.ToVec3() * ImportScale;
                    mesh.Vertices[i] = v;
                    mesh.Vertices[i].Z *= -1.0f;
                    mesh.UVs[i] = vert.UV.ToVec2();
                    mesh.Normals[i] = vert.Normal.ToVec3();
                    mesh.Normals[i].Z *= -1.0f;
                    mesh.BoneWeights[i].boneIndex0 = vert.BoneIndex0;
                    mesh.BoneWeights[i].boneIndex1 = vert.BoneIndex1;
                    if (mesh.BoneWeights[i].boneIndex1 == 0)
                        mesh.BoneWeights[i].boneIndex1 = -1;
                    mesh.BoneWeights[i].boneIndex2 = -1;
                    mesh.BoneWeights[i].boneIndex3 = -1;
                    mesh.BoneWeights[i].weight0 = 1.0f - vert.Weight;
                    mesh.BoneWeights[i].weight1 = vert.Weight;
                }

                var offset = 0;
                for (var i = 0; i < pmd.MaterialList.MaterialNum; i++)
                {
                    var mat = pmd.MaterialList.Materials[i];
                    var indices = new int[mat.FaceVertNum];
                    for (var j = 0; j < mat.FaceVertNum / 3; j++)
                    {
                        indices[j * 3 + 0] = pmd.FaceList.Indices[offset++];
                        indices[j * 3 + 1] = pmd.FaceList.Indices[offset++];
                        indices[j * 3 + 2] = pmd.FaceList.Indices[offset++];
                    }
                    if (mat.FaceVertNum > 0)
                    {
                        mesh.SetIndices(i, indices, OpenTK.Graphics.OpenGL4.BeginMode.Triangles);
                    }
                }
            }
            obj.Meshes = new Mesh[] { mesh };

            // bone
            if (pmd.BoneList != null && pmd.BoneList.BoneNum > 0)
            {
                obj.Bones = CreateBones(pmd.BoneList.Bones);
                BindBones(obj.Bones, pmd.BoneList.Bones, pmd.IKList.IKs);
            }

            // morph
            if (pmd.MorphList != null && pmd.MorphList.MorphNum > 0)
            {
                obj.Morphs = new Morph[pmd.MorphList.MorphNum];

                for (var i = 0; i < pmd.MorphList.MorphNum; i++)
                {
                    obj.Morphs[i] = new Morph();

                    var m = pmd.MorphList.Morphs[i];

                    obj.Morphs[i].Name = m.Name;
                    obj.Morphs[i].Vertices = new VertexMorph[m.SkinVertCount];
                    for (var j = 0; j < m.SkinVertCount; j++)
                    {
                        obj.Morphs[i].Vertices[j] = new VertexMorph();
                        obj.Morphs[i].Vertices[j].Index = (int)m.Data[j].Index;
                        obj.Morphs[i].Vertices[j].Offset = m.Data[j].Offset.ToVec3(true) * ImportScale;
                    }
                }
            }

            return new ImportedObject[] { obj };
        }

        private void BindBones(Bone[] bones, PmdBone[] pmdBones, PmdIK[] iks)
        {
            // bind parent
            for (var i = 0; i < bones.Length; i++)
            {
                if (pmdBones[i].ParentIndex != -1) bones[i].Parent = bones[pmdBones[i].ParentIndex];
            }

            // bind children
            for (var i = 0; i < bones.Length; i++)
            {
                bones[i].Children = Array.FindAll(bones, b => b.Parent == bones[i]);
                if (bones[i].Children.Length == 0) bones[i].Children = null;
            }

            // bind ik
            if (iks != null)
            {
                foreach (var ik in iks)
                {
                    var bone = bones[ik.Index];
                    
                    bone.IKTarget = bones[ik.TargetIndex];
                    bone.IKLoop = ik.Itarations;
                    bone.IKRotLimit = ik.ControlWeight * (float)Math.PI * 0.25f;
                    bone.IKLinks = new IKLink[ik.ChainLength];
                    for (var i = 0; i < ik.ChainLength; i++)
                    {
                        bone.IKLinks[i] = new IKLink();
                        bone.IKLinks[i].Bone = bones[ik.ChildIndices[i]];

                        if (bone.IKLinks[i].Bone.Name.Contains("ひざ"))
                        {
                            bone.IKLinks[i].LimitAngle = true;
                            bone.IKLinks[i].UpperLimitAngle = OpenTK.Vector3.UnitX * OpenTK.MathHelper.Pi;
                        }
                    }
                }
            }
        }

        private Bone[] CreateBones(PmdBone[] pmdBones)
        {
            var bones = new Bone[pmdBones.Length];
            for (var i = 0; i < bones.Length; i++)
            {
                var b = new Bone()
                {
                    Name = pmdBones[i].Name,
                    Index = i,
                    Position = pmdBones[i].HeadPos.ToVec3(true) * ImportScale,
                    BoneType = "Standard",
                };
                b.Invisible = pmdBones[i].Type == PmdBone.BoneType.Invisible;
                if (pmdBones[i].Type == PmdBone.BoneType.IK) b.BoneType = "IK";
                if (pmdBones[i].Type == PmdBone.BoneType.UnderIK) b.BoneType = "UnderIK";
                if (pmdBones[i].Type == PmdBone.BoneType.IKConnect) b.BoneType = "IKConnect";
                bones[i] = b;
            }
            return bones;
        }
        /*
        private Bone CreateBones(PmdBone[] bones, Bone parent = null)
        {
            if (parent == null)
            {
                var root = Array.Find(bones, (b) => b.ParentIndex == -1);
                parent = new Bone()
                {
                    Name = root.Name,
                    Index = Array.IndexOf(bones, root),
                    BoneType = BoneType.Standard,
                };
                if (bones[parent.Index].Type == PmdBone.BoneType.IK) parent.BoneType = BoneType.IK;
                if (bones[parent.Index].Type == PmdBone.BoneType.UnderIK) parent.BoneType = BoneType.UnderIK;
                if (bones[parent.Index].Type == PmdBone.BoneType.IKConnect) parent.BoneType = BoneType.IKConnect;
                parent.Position = root.HeadPos.ToVec3(true) * ImportScale;

                
                //if (CheckLocal(parent.Name))
                //{
                //    var dir = bones[root.TailIndex].HeadPos - bones[root.ParentIndex].HeadPos;
                //    dir.Z = 0.0f;
                //    parent.AxisX = dir.ToVec3().Normalized();
                //    parent.AxisZ = OpenTK.Vector3.UnitZ;
                //}
            }

            var child = Array.FindAll(bones, (b) => b.ParentIndex == parent.Index);
            if (child != null && child.Length > 0)
            {
                var children = new Bone[child.Length];
                for (var i = 0; i < child.Length; i++)
                {
                    children[i] = new Bone()
                    {
                        Name = child[i].Name,
                        Index = Array.IndexOf(bones, child[i]),
                        Parent = parent,
                        BoneType = BoneType.Standard,
                    };
                    if (bones[children[i].Index].Type == PmdBone.BoneType.IK) children[i].BoneType = BoneType.IK;
                    if (bones[children[i].Index].Type == PmdBone.BoneType.UnderIK) children[i].BoneType = BoneType.UnderIK;
                    if (bones[children[i].Index].Type == PmdBone.BoneType.IKConnect) children[i].BoneType = BoneType.IKConnect;

                    children[i].Position = child[i].HeadPos.ToVec3(true) * ImportScale;

                    
                    //if (CheckLocal(children[i].Name))
                    //{
                    //    var dir = bones[child[i].TailIndex].HeadPos - bones[child[i].ParentIndex].HeadPos;
                    //    dir.Z = 0.0f;
                    //    children[i].AxisX = dir.ToVec3().Normalized();
                    //    children[i].AxisZ = OpenTK.Vector3.UnitZ;
                    //}

                    CreateBones(bones, children[i]);
                }
                parent.Children = children;
            }

            return parent;
        }
        private bool CheckLocal(string name)
        {
            var locals = new string[]
            {
                "左腕",
                "左ひじ",
                "左手首",
                "右腕",
                "右ひじ",
                "右手首",
            };

            var localContains = new string[]
            {
                "親指",
                "人指",
                "中指",
                "薬指",
                "子指",
            };

            if (Array.Exists(locals, s => s == name)) return true;

            foreach (var s in localContains)
            {
                if (name.Contains(s)) return true;
            }

            return false;
        }*/
    }

    static class PmdExtension
    {
        public static OpenTK.Vector2 ToVec2(this Vector2 v)
        {
            return new OpenTK.Vector2(v.X, v.Y);
        }

        public static OpenTK.Vector3 ToVec3(this Vector3 v, bool flipZ = false)
        {
            return new OpenTK.Vector3(v.X, v.Y, flipZ ? -v.Z : v.Z);
        }

        public static Color4 ToColor4(this Vector3 v, float alpha = 1.0f)
        {
            return new Color4(v.X, v.Y, v.Z, alpha);
        }
    }
}
