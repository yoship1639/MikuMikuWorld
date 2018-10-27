using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PmdModelImporter
{
    public class PmdImportResult
    {
        public enum Result
        {
            Success,
            FileNotFound,
            NotPmdFormat,
            Failed,
        }

        public PmdModelData pmd;
        public Result result;
    }

    public class PmdModelImporter
    {
        public string Extension { get { return ".pmd"; } }

        public PmdImportResult Import(string filename, bool full)
        {
            PmdImportResult result = new PmdImportResult()
            {
                result = PmdImportResult.Result.FileNotFound,
            };

            if (!File.Exists(filename)) return result;

            result.result = PmdImportResult.Result.Failed;

            using (BinaryReader br = new BinaryReader(new MemoryStream(File.ReadAllBytes(filename))))
            {
                // ヘッダの読み込み
                PmdHeader header = ReadPmdHeader(br);
                if (header == null)
                {
                    result.result = PmdImportResult.Result.NotPmdFormat;
                    return result;
                }

                if (!full)
                {
                    var m = new PmdModelData();
                    m.Header = header;
                    result.pmd = m;
                    result.result = PmdImportResult.Result.Success;
                    return result;
                }

                // 頂点リストの読み込み
                PmdVertexList vertexList = ReadPmdVertexList(br);
                if (vertexList == null) return result;

                // 面リストの読み込み
                PmdFaceList faceList = ReadPmdFaceList(br);
                if (faceList == null) return result;

                // マテリアルの読み込み
                PmdMaterialList materialList = ReadPmdMaterialList(br);
                if (materialList == null) return result;

                // ボーンの読み込み
                PmdBoneList boneList = ReadPmdBoneList(br);
                if (boneList == null) return result;

                // IKデータの読み込み
                PmdIKList iKList = ReadPmdIKList(br);
                if (iKList == null) return result;

                // 表情データの読み込み
                PmdMorphList morphList = ReadPmdMorphList(br);

                result.result = PmdImportResult.Result.Success;
                
                var model = new PmdModelData();
                model.Header = header;
                model.VertexList = vertexList;
                model.FaceList = faceList;
                model.MaterialList = materialList;
                model.BoneList = boneList;
                model.IKList = iKList;
                model.MorphList = morphList;

                result.pmd = model;
            }

            return result;
        }

        PmdHeader ReadPmdHeader(BinaryReader br)
        {
            Encoding sjis = Encoding.GetEncoding("Shift_JIS");

            // マジック読み込み
            string magic = new string(br.ReadChars(3));
            if (magic != "Pmd") return null;

            // バージョン読み込み
            float version = br.ReadSingle();

            // 名前読み込み
            var bname = br.ReadBytes(20);
            string name = sjis.GetString(GetRange(bname, 0x00));

            // コメント読み込み
            var bcomment = br.ReadBytes(256);
            string comment = sjis.GetString(GetRange(bcomment, 0x00));

            return new PmdHeader(magic, version, name, comment);
        }

        PmdVertexList ReadPmdVertexList(BinaryReader br)
        {
            // 頂点数読み込み
            uint vertNum = br.ReadUInt32();

            // 頂点データの読み込み
            PmdVertex[] vertices = new PmdVertex[vertNum];
            for (int i = 0; i < vertNum; i++)
            {
                var vertex = new PmdVertex();

                vertex.Position = ReadVector3(br);
                vertex.Normal = ReadVector3(br);
                vertex.UV = ReadVector2(br);
                vertex.BoneIndex0 = br.ReadUInt16();
                vertex.BoneIndex1 = br.ReadUInt16();
                vertex.Weight = br.ReadByte() / 255.0f;
                vertex.HasEdge = br.ReadByte() == 0;

                vertices[i] = vertex;
            }

            return new PmdVertexList((int)vertNum, vertices);
        }

        PmdFaceList ReadPmdFaceList(BinaryReader br)
        {
            // 面数を読み込み
            uint vertNum = br.ReadUInt32();
            if (vertNum % 3 != 0) return null;
            uint faceNum = vertNum / 3;

            // 面の読み込み
            ushort[] indices = new ushort[vertNum];
            for (int i = 0; i < indices.Length; i++)
            {
                indices[i] = br.ReadUInt16();
            }

            return new PmdFaceList((int)faceNum, indices);
        }

        PmdMaterialList ReadPmdMaterialList(BinaryReader br)
        {
            Encoding sjis = Encoding.GetEncoding("Shift_JIS");

            // マテリアルの数を読み込み
            uint matNum = br.ReadUInt32();
            //if (matNum > 128) return null;

            PmdMaterial[] materials = new PmdMaterial[matNum];
            for (int i = 0; i < matNum; i++)
            {
                PmdMaterial mat = new PmdMaterial();

                mat.DiffuseColor = ReadVector3(br);
                mat.Alpha = br.ReadSingle();
                mat.SpecularPower = br.ReadSingle();
                mat.SpecularColor = ReadVector3(br);
                mat.AmbientColor = ReadVector3(br);
                mat.ToonIndex = br.ReadByte();
                mat.EdgeFlag = br.ReadByte();
                mat.FaceVertNum = br.ReadUInt32();
                var name = GetRange(br.ReadBytes(20), 0x00);
                mat.TextureFileName = sjis.GetString(name);

                materials[i] = mat;
            }

            return new PmdMaterialList((int)matNum, materials);
        }

        PmdBoneList ReadPmdBoneList(BinaryReader br)
        {
            Encoding sjis = Encoding.GetEncoding("Shift_JIS");

            // ボーンの数を読み込み
            ushort boneNum = br.ReadUInt16();

            // ボーンの読み込み
            PmdBone[] bones = new PmdBone[boneNum];
            for (ushort i = 0; i < boneNum; i++)
            {
                bones[i] = new PmdBone();

                var name = br.ReadBytes(20);
                bones[i].Name = sjis.GetString(GetRange(name, 0x00));

                bones[i].ParentIndex = br.ReadInt16();
                bones[i].TailIndex = br.ReadInt16();
                bones[i].Type = (PmdBone.BoneType)br.ReadByte();
                bones[i].IKIndex = br.ReadInt16();
                bones[i].HeadPos = ReadVector3(br);
            }

            return new PmdBoneList(boneNum, bones);
        }

        PmdIKList ReadPmdIKList(BinaryReader br)
        {
            // IKデータの数を取得
            ushort iKNum = br.ReadUInt16();

            PmdIK[] iKs = new PmdIK[iKNum];
            for (ushort i = 0; i < iKNum; i++)
            {
                iKs[i] = new PmdIK();

                iKs[i].Index = br.ReadUInt16();
                iKs[i].TargetIndex = br.ReadUInt16();
                iKs[i].ChainLength = br.ReadByte();
                iKs[i].Itarations = br.ReadUInt16();
                iKs[i].ControlWeight = br.ReadSingle();
                iKs[i].ChildIndices = new ushort[iKs[i].ChainLength];
                for (ushort j = 0; j < iKs[i].ChainLength; j++)
                {
                    iKs[i].ChildIndices[j] = br.ReadUInt16();
                }
            }

            return new PmdIKList(iKNum, iKs);
        }

        PmdMorphList ReadPmdMorphList(BinaryReader br)
        {
            Encoding sjis = Encoding.GetEncoding("Shift_JIS");

            var morphNum = br.ReadUInt16();

            if (morphNum - 1 <= 0) return null;

            var morphs = new List<PmdMorph>();
            PmdMorph baseMorph = null;
            for (var i = 0; i < morphNum; i++)
            {
                var m = new PmdMorph();

                var name = br.ReadBytes(20);
                m.Name = sjis.GetString(GetRange(name, 0x00));

                m.SkinVertCount = br.ReadUInt32();
                m.SkinType = br.ReadByte();

                m.Data = new PmdMorphVertex[m.SkinVertCount];
                for (var j = 0; j < m.SkinVertCount; j++)
                {
                    var v = new PmdMorphVertex();
                    v.Index = br.ReadUInt32();
                    v.Offset = ReadVector3(br);
                    m.Data[j] = v;
                }

                if (m.SkinType == 0)
                    baseMorph = m;
                else
                {
                    for (var j = 0; j < m.SkinVertCount; j++)
                    {
                        var idx = m.Data[j].Index;
                        m.Data[j].Index = baseMorph.Data[idx].Index;
                        //m.Data[j].Offset = baseMorph.Data[idx].Offset + m.Data[j].Offset;
                    }
                    morphs.Add(m);
                }
            }

            return new PmdMorphList((ushort)(morphNum - 1), morphs.ToArray());
        }

        private byte[] GetRange(byte[] src, byte end)
        {
            List<byte> buf = new List<byte>(src.Length);
            for (int i = 0; i < src.Length; i++)
            {
                if (src[i] == end) break;
                buf.Add(src[i]);
            }
            return buf.ToArray();
        }

        private Vector3 ReadVector3(BinaryReader br)
        {
            return new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        }

        private Vector2 ReadVector2(BinaryReader br)
        {
            return new Vector2(br.ReadSingle(), br.ReadSingle());
        }
    }
}
