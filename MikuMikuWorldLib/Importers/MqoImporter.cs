using MikuMikuWorld.Assets;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Importers
{
    public class MqoImporter : IImporter
    {
        public bool DirectoryImporter { get { return false; } }
        public string[] Extensions { get { return new string[] { ".mqo" }; } }

        public bool CombineMeshes { get; set; } = true;
        public bool CombineTextures { get; set; } = false;
        public bool CombineMaterials { get; set; } = true;

        public ImportedObject[] Import(string path, ImportType type)
        {
            var obj = new ImportedObject()
            {
                Result = Result.Failed,
                Type = ImportedObjectType.Model,
                Path = path,
            };

            var importer = new MqoModelImporter.MqoModelImporter();
            var res = importer.Import(path, type == ImportType.Full);
            if (res == null || res.result != MqoModelImporter.MqoImportResult.Result.Success)
            {
                return new ImportedObject[] { obj };
            }

            var mqo = res.mqo;

            obj.Name = Path.GetFileNameWithoutExtension(path);
            obj.Version = mqo.Version.ToString();

            if (type == ImportType.OverviewOnly)
            {
                obj.Result = Result.Success;
                return new ImportedObject[] { obj };
            }

            #region Material
            // material
            var materials = new List<Material>();
            var textures = new List<Texture2D>();
            var texturePaths = new List<string>();
            foreach (var m in mqo.Materials)
            {
                var mat = new Material(m.Name);
                
                mat.AddParam("albedo", "Albedo", new OpenTK.Graphics.Color4(m.Color.R * m.Diffuse, m.Color.G * m.Diffuse, m.Color.B * m.Diffuse, m.Color.A));
                //mat.AddParam("specular", "Specular", new OpenTK.Graphics.Color4(m.Specular, m.Specular, m.Specular, 1.0f));
                //mat.AddParam("shininess", "Shininess", m.SpecularPower);
                //mat.AddParam("reflect", "Reflect", m.Reflect);
                //mat.AddParam("refract", "Refract", m.Refract);
                //mat.AddParam("ambient", "Ambient", new OpenTK.Graphics.Color4(m.Ambient, m.Ambient, m.Ambient, 1.0f));
                mat.AddParam("emissive", "Emissive", new OpenTK.Graphics.Color4(m.Emissive, m.Emissive, m.Emissive, 0.0f));

                mat.AddParam("roughness", "Roughness", 1.0f - MMWMath.Clamp(m.SpecularPower / 32.0f, 0.0f, 1.0f));
                mat.AddParam("metallic", "Metallic", m.SpecularPower > 14.0f ? 1.0f : 0.0f);
                mat.AddParam("reflectance", "Reflectance", m.Reflect);
                //m.AddParam("f0", "F0", (mat.SpecularColor + new Vector3(0.15f, 0.15f, 0.15f)).ToColor4());
                //mat.AddParam("f0", "F0", new OpenTK.Graphics.Color4(m.Specular, m.Specular, m.Specular, 1.0f));

                Texture2D difMap = null;
                if (!string.IsNullOrEmpty(m.TextureFullpath) && File.Exists(m.TextureFullpath))
                {
                    var index = texturePaths.IndexOf(m.TextureFullpath);
                    if (index == -1)
                    {
                        var bitmap = (Bitmap)Image.FromFile(m.TextureFullpath);
                        difMap = new Texture2D(bitmap, Path.GetFileNameWithoutExtension(m.TextureFullpath));
                        difMap.UseMipmap = true;
                        difMap.WrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat;
                        difMap.MinFilter = OpenTK.Graphics.OpenGL4.TextureMinFilter.LinearMipmapLinear;
                        textures.Add(difMap);
                        texturePaths.Add(m.TextureFullpath);
                    }
                    else difMap = textures[index];
                    mat.AddParam("albedoMap", "AlbedoMap", difMap);
                }
                else mat.AddParam<Texture2D>("albedoMap", "AlbedoMap", null);
                mat.AddParam<Texture2D>("specularMap", "SpecularMap", null);
                mat.AddParam<Texture2D>("normalMap", "NormalMap", null);
                mat.AddParam<Texture2D>("physicalMap", "PhysicalMap", null);

                Texture2D bumpMap = null;
                if (!string.IsNullOrEmpty(m.BumpMapFullpath) && File.Exists(m.BumpMapFullpath))
                {
                    var index = texturePaths.IndexOf(m.BumpMapFullpath);
                    if (index == -1)
                    {
                        var bitmap = (Bitmap)Image.FromFile(m.BumpMapFullpath);
                        bumpMap = new Texture2D(bitmap, Path.GetFileNameWithoutExtension(m.BumpMapFullpath));
                        bumpMap.WrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat;
                        textures.Add(difMap);
                        texturePaths.Add(m.BumpMapFullpath);
                    }
                    else difMap = textures[index];
                    mat.AddParam("bumpMap", "BumpMap", bumpMap);
                }
                

                Texture2D alphaMap = null;
                if (!string.IsNullOrEmpty(m.AlphaMapFullpath) && File.Exists(m.AlphaMapFullpath))
                {
                    var index = texturePaths.IndexOf(m.AlphaMapFullpath);
                    if (index == -1)
                    {
                        var bitmap = (Bitmap)Image.FromFile(m.AlphaMapFullpath);
                        alphaMap = new Texture2D(bitmap, Path.GetFileNameWithoutExtension(m.AlphaMapFullpath));
                        alphaMap.WrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat;
                        textures.Add(difMap);
                        texturePaths.Add(m.AlphaMapFullpath);
                    }
                    else alphaMap = textures[index];
                    mat.AddParam("alphaMap", "AlphaMap", alphaMap);
                }

                materials.Add(mat);
            }
            obj.Materials = materials.ToArray();
            obj.Textures = textures.ToArray();
            #endregion

            if (!CombineMeshes)
            {
                // mesh
                var meshes = new List<Mesh>();
                foreach (var o in mqo.Objects)
                {
                    // vertices
                    var vertices = new List<OpenTK.Vector3>();
                    for (int i = 0; i < o.Vertices.Length; i++)
                    {
                        vertices.Add(new OpenTK.Vector3(o.Vertices[i].X, o.Vertices[i].Y, o.Vertices[i].Z));
                    }

                    // indices
                    var matNum = mqo.Materials.Length;
                    var indicesList = new List<int>[matNum];
                    var subs = new List<SubMesh>();

                    // init
                    for (int i = 0; i < matNum; i++)
                    {
                        indicesList[i] = new List<int>();
                        subs.Add(new SubMesh());
                        subs[i].materialIndex = i;
                        subs[i].mode = BeginMode.Triangles;
                    }

                    // uvs
                    var uvs = new List<OpenTK.Vector2>();
                    for (int i = 0; i < o.Vertices.Length; i++)
                    {
                        uvs.Add(new OpenTK.Vector2(-1.0f, -1.0f));
                    }

                    // face
                    foreach (var face in o.Faces)
                    {
                        var matIndex = face.MaterialIndex;
                        var offset = indicesList[matIndex].Count;

                        indicesList[matIndex].Add(face.Indices[0]);
                        indicesList[matIndex].Add(face.Indices[1]);
                        indicesList[matIndex].Add(face.Indices[2]);

                        if (face.UVs != null)
                        {
                            for (var i = 0; i < 3; i++)
                            {
                                var faceIndex = face.Indices[i];
                                var newUV = new OpenTK.Vector2(face.UVs[i].X, face.UVs[i].Y);
                                if (CombineTextures)
                                {
                                    while (newUV.X < 0.0f) newUV.X += 1.0f;
                                    while (newUV.Y < 0.0f) newUV.Y += 1.0f;
                                    while (newUV.X >= 1.0f) newUV.X -= 1.0f;
                                    while (newUV.Y >= 1.0f) newUV.Y -= 1.0f;
                                }
                                if (uvs[faceIndex].X == -1.0f && uvs[faceIndex].Y == -1.0f)
                                {
                                    uvs[faceIndex] = newUV;
                                }
                                else
                                {
                                    // UVがかぶったら新しい頂点を追加
                                    var newIndex = vertices.Count;
                                    vertices.Add(vertices[faceIndex]);
                                    indicesList[matIndex][offset + i] = newIndex;
                                    uvs.Add(newUV);
                                }
                            }
                        }
                    }

                    for (int i = 0; i < matNum; i++) subs[i].indices = indicesList[i].ToArray();

                    subs.RemoveAll((s) => s.indices.Length == 0);

                    var mesh = new Mesh(o.Name)
                    {
                        Vertices = vertices.ToArray(),
                        UVs = uvs.ToArray(),
                    };

                    mesh.subMeshes = subs;
                    meshes.Add(mesh);
                }
                obj.Meshes = meshes.ToArray();
            }
            else
            {
                // mesh
                var meshes = new List<Mesh>();
                // vertices
                var vertices = new List<OpenTK.Vector3>();
                var uvs = new List<OpenTK.Vector2>();
                foreach (var o in mqo.Objects)
                {
                    var vertexOffset = vertices.Count;
                    for (int i = 0; i < o.Vertices.Length; i++)
                    {
                        vertices.Add(new OpenTK.Vector3(o.Vertices[i].X, o.Vertices[i].Y, o.Vertices[i].Z));
                        uvs.Add(new OpenTK.Vector2(-1.0f, -1.0f));
                    }

                    // indices
                    var matNum = mqo.Materials.Length;
                    var indicesList = new List<int>[matNum];
                    var subs = new List<SubMesh>();

                    // init
                    for (int i = 0; i < matNum; i++)
                    {
                        indicesList[i] = new List<int>();
                        subs.Add(new SubMesh());
                        subs[i].materialIndex = i;
                        subs[i].mode = BeginMode.Triangles;
                    }

                    // face
                    foreach (var face in o.Faces)
                    {
                        var matIndex = face.MaterialIndex;
                        var offset = indicesList[matIndex].Count;

                        indicesList[matIndex].Add(vertexOffset + face.Indices[0]);
                        indicesList[matIndex].Add(vertexOffset + face.Indices[1]);
                        indicesList[matIndex].Add(vertexOffset + face.Indices[2]);

                        if (face.UVs != null)
                        {
                            for (var i = 0; i < 3; i++)
                            {
                                var faceIndex = vertexOffset + face.Indices[i];
                                var newUV = new OpenTK.Vector2(face.UVs[i].X, face.UVs[i].Y);
                                if (CombineTextures)
                                {
                                    while (newUV.X < 0.0f) newUV.X += 1.0f;
                                    while (newUV.Y < 0.0f) newUV.Y += 1.0f;
                                    while (newUV.X >= 1.0f) newUV.X -= 1.0f;
                                    while (newUV.Y >= 1.0f) newUV.Y -= 1.0f;
                                }
                                if (uvs[faceIndex].X == -1.0f && uvs[faceIndex].Y == -1.0f)
                                {
                                    uvs[faceIndex] = newUV;
                                }
                                else
                                {
                                    // UVがかぶったら新しい頂点を追加
                                    var newIndex = vertices.Count;
                                    vertices.Add(vertices[faceIndex]);
                                    indicesList[matIndex][offset + i] = newIndex;
                                    uvs.Add(newUV);
                                }
                            }
                        }
                    }

                    for (int i = 0; i < matNum; i++) subs[i].indices = indicesList[i].ToArray();

                    subs.RemoveAll((s) => s.indices.Length == 0);

                    var m = new Mesh(o.Name)
                    {
                        Vertices = vertices.ToArray(),
                        UVs = uvs.ToArray(),
                    };

                    m.subMeshes = subs;
                    meshes.Add(m);
                }
                obj.Meshes = meshes.ToArray();

                // メッシュを一つにまとめる
                {
                    var mesh = new Mesh(obj.Name);
                    mesh.Vertices = vertices.ToArray();
                    mesh.UVs = uvs.ToArray();
                    var indicesList = new List<int>[materials.Count];
                    for (var i = 0; i < materials.Count; i++)
                    {
                        indicesList[i] = new List<int>();
                    }

                    foreach (var m in obj.Meshes)
                    {
                        foreach (var sub in m.subMeshes)
                        {
                            indicesList[sub.materialIndex].AddRange(sub.indices);
                        }
                    }

                    for (var i = 0; i < indicesList.Length; i++)
                    {
                        if (indicesList[i].Count == 0) continue;
                        mesh.SetIndices(i, indicesList[i].ToArray(), BeginMode.Triangles);
                    }

                    obj.Meshes = new Mesh[] { mesh };
                }
            }

            // テクスチャを一つにまとめる
            if (CombineTextures)
            {
                var gridCount = 1;
                while (gridCount * gridCount < textures.Count) gridCount++;

                var bitmaps = new Bitmap[textures.Count];
                for (var i = 0; i < bitmaps.Length; i++) bitmaps[i] = textures[i].SrcBitmap;

                var bitmap = BitmapHelper.CombineBitmaps(bitmaps, gridCount);
                obj.Textures = new Texture2D[] { new Texture2D(bitmap)
                {
                    WrapMode = TextureWrapMode.Repeat,
                } };

                // 全メッシュのUVを補正する
                foreach (var mesh in obj.Meshes)
                {
                    foreach (var sub in mesh.subMeshes)
                    {
                        var mat = materials[sub.materialIndex];
                        if (!mat.HasParam<Texture2D>("albedoMap")) continue;
                        var tex = mat.GetParam<Texture2D>("albedoMap");
                        var texIndex = textures.IndexOf(tex);
                        var x = texIndex % gridCount;
                        var y = texIndex / gridCount;

                        foreach (var index in sub.indices)
                        {
                            var uv = mesh.UVs[index] / gridCount;
                            uv += (new OpenTK.Vector2(x, y) / gridCount);
                            mesh.UVs[index] = uv;
                        }
                    }
                }

                foreach (var mat in obj.Materials)
                {
                    if (mat.HasParam<Texture2D>("albedoMap"))
                    {
                        mat.SetParam("albedoMap", obj.Textures[0]);
                    }
                }
            }

            // マテリアルを一つにまとめる
            if (CombineMaterials)
            {
                // パラメータが被っているマテリアルを１つにまとめる
                var mats = new List<Material>();
                var indexTable = new int[materials.Count];
                for (var i=0; i<materials.Count; i++)
                {
                    var diffuse = materials[i].GetParam<OpenTK.Graphics.Color4>("albedo");
                    //var specular = materials[i].GetParam<OpenTK.Graphics.Color4>("specular");
                    //var shininess = materials[i].GetParam<float>("shininess");
                    //var reflect = materials[i].GetParam<float>("reflect");
                    //var refract = materials[i].GetParam<float>("refract");
                    //var ambient = materials[i].GetParam<float>("ambient");
                    var emissive = materials[i].GetParam<float>("emissive");
                    var difMap = materials[i].GetParam<Texture2D>("albedoMap");
                    var alphaMap = materials[i].GetParam<Texture2D>("alphaMap");
                    var bumpMap = materials[i].GetParam<Texture2D>("bumpMap");

                    var m = mats.Find((mat) =>
                    {
                        bool same = true;
                        same = same && diffuse == mat.GetParam<OpenTK.Graphics.Color4>("albedo");
                        //same = same && specular == mat.GetParam<OpenTK.Graphics.Color4>("specular");
                        //same = same && shininess == mat.GetParam<float>("shininess");
                        //same = same && reflect == mat.GetParam<float>("reflect");
                        //same = same && refract == mat.GetParam<float>("refract");
                        //same = same && ambient == mat.GetParam<float>("ambient");
                        same = same && emissive == mat.GetParam<float>("emissive");
                        if (difMap == null) same = same && mat.GetParam<Texture2D>("albedoMap") == null;
                        else same = same && difMap == mat.GetParam<Texture2D>("albedoMap");
                        if (alphaMap == null) same = same && mat.GetParam<Texture2D>("alphaMap") == null;
                        else same = same && alphaMap == mat.GetParam<Texture2D>("alphaMap");
                        if (bumpMap == null) same = same && mat.GetParam<Texture2D>("bumpMap") == null;
                        else same = same && bumpMap == mat.GetParam<Texture2D>("bumpMap");

                        return same;
                    });

                    if (m != null) indexTable[i] = mats.IndexOf(m);
                    else
                    {
                        mats.Add(materials[i]);
                        indexTable[i] = mats.Count - 1;
                    } 
                }

                foreach (var mesh in obj.Meshes)
                {
                    foreach (var sub in mesh.subMeshes)
                    {
                        sub.materialIndex = indexTable[sub.materialIndex];
                    }
                }

                obj.Materials = mats.ToArray();
            }

            if (CombineMeshes && CombineMaterials)
            {
                var indices = new List<int>[materials.Count];
                foreach (var sub in obj.Meshes[0].subMeshes)
                {
                    if (indices[sub.materialIndex] == null) indices[sub.materialIndex] = new List<int>();
                    indices[sub.materialIndex].AddRange(sub.indices);
                }

                obj.Meshes[0].subMeshes.Clear();

                for (var i = 0; i < materials.Count; i++)
                {
                    if (indices[i] == null) continue;
                    obj.Meshes[0].SetIndices(i, indices[i].ToArray(), BeginMode.Triangles);
                }
            }

            return new ImportedObject[] { obj };
        }
    }
}
