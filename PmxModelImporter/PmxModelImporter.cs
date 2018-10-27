using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PmxModelImporter
{
    public class PmxImportResult
    {
        public enum Result
        {
            Success,
            FileNotFound,
            NotPmxFormat,
            Failed,
        }

        public PmxModelData pmx;
        public Result result;
    }

    public class PmxModelImporter
    {
        public string Extension { get { return ".pmx"; } }

        public PmxImportResult Import(string filename, bool full)
        {
            PmxImportResult result = new PmxImportResult()
            {
                result = PmxImportResult.Result.FileNotFound,
            };

            if (!File.Exists(filename)) return result;

            result.result = PmxImportResult.Result.Failed;

            using (BinaryReader br = new BinaryReader(new MemoryStream(File.ReadAllBytes(filename))))
            {
                // ヘッダの読み込み
                var header = ReadPmxHeader(br);
                if (header == null)
                {
                    result.result = PmxImportResult.Result.NotPmxFormat;
                    return result;
                }

                // モデル情報の読み込み
                var modelInfo = ReadPmxModelInfo(br, header);
                if (modelInfo == null) return result;

                // フルで読み込まない場合
                if (!full)
                {
                    var m = new PmxModelData();
                    m.Header = header;
                    m.ModelInfo = modelInfo;
                    result.result = PmxImportResult.Result.Success;
                    result.pmx = m;
                    return result;
                }

                // 頂点リストの読み込み
                var vertexList = ReadPmxVertexList(br, header);
                if (vertexList == null) return result;

                // 面リストの読み込み
                var faceList = ReadPmxFaceList(br, header);
                if (faceList == null) return result;

                // テクスチャリストの読み込み
                var textureList = ReadPmxTextureList(br, header);
                if (textureList == null) return result;

                // マテリアルの読み込み
                var materialList = ReadPmxMaterialList(br, header);
                if (materialList == null) return result;

                // ボーンの読み込み
                var boneList = ReadPmxBoneList(br, header);
                if (boneList == null) return result;

                // モーフの読み込み
                var morphList = ReadPmxMorphList(br, header);
                if (morphList == null) return result;

                result.result = PmxImportResult.Result.Success;
                
                var model = new PmxModelData();
                model.Header = header;
                model.ModelInfo = modelInfo;
                model.VertexList = vertexList;
                model.FaceList = faceList;
                model.TextureList = textureList;
                model.MaterialList = materialList;
                model.BoneList = boneList;
                model.MorphList = morphList;

                result.pmx = model;
            }

            return result;
        }

        PmxHeader ReadPmxHeader(BinaryReader br)
        {
            var header = new PmxHeader();

            // マジック読み込み
            header.Magic = new string(br.ReadChars(4));
            if (header.Magic != "PMX " && header.Magic != "Pmx ") return null;

            // バージョン読み込み
            header.Version = br.ReadSingle();

            // 各種バイト情報読み込み
            var size = br.ReadByte();
            for (var i = 0; i < size; i++)
            {
                var data = br.ReadByte();
                if (i == 0) header.Encoding = data;
                else if (i == 1) header.AddUVCound = data;
                else if (i == 2) header.VertexIndexSize = data;
                else if (i == 3) header.TextureIndexSize = data;
                else if (i == 4) header.MaterialIndexSize = data;
                else if (i == 5) header.BoneIndexSize = data;
                else if (i == 6) header.MorphIndexSize = data;
                else if (i == 7) header.RigidIndexSize = data;
            }

            return header;
        }

        PmxModelInfo ReadPmxModelInfo(BinaryReader br, PmxHeader header)
        {
            var encode = header.Encoding == 0 ? Encoding.Unicode : Encoding.UTF8;

            var info = new PmxModelInfo();

            // モデル名
            info.ModelName = br.ReadString(encode);

            // モデル英名
            info.ModelNameEng = br.ReadString(encode);

            // コメント
            info.Comment = br.ReadString(encode);

            // 英コメント
            info.CommentEng = br.ReadString(encode);

            return info;
        }

        PmxVertexList ReadPmxVertexList(BinaryReader br, PmxHeader header)
        {
            var vertList = new PmxVertexList();

            // 頂点数読み込み
            vertList.VertexNum = br.ReadInt32();

            // 頂点データの読み込み
            vertList.Vertices = new PmxVertex[vertList.VertexNum];
            for (int i = 0; i < vertList.VertexNum; i++)
            {
                var vertex = new PmxVertex();

                vertex.Position = br.ReadVector3();  // 位置
                vertex.Normal = br.ReadVector3();    // 法線
                vertex.UV = br.ReadVector2();        // UV

                // 追加UV
                for (var j = 0; j < header.AddUVCound; j++)
                {
                    var uv = br.ReadVector4();
                    if (j == 0) vertex.UV1 = uv;
                    if (j == 1) vertex.UV2 = uv;
                    if (j == 2) vertex.UV3 = uv;
                    if (j == 3) vertex.UV4 = uv;
                }

                // ウェイト変形方式
                vertex.WeightType = (PmxVertex.PmxWeightType)br.ReadByte();
                if (vertex.WeightType == PmxVertex.PmxWeightType.BDEF1)
                {
                    vertex.BoneIndex0 = br.ReadFromIndexSize(header.BoneIndexSize);
                }
                else if (vertex.WeightType == PmxVertex.PmxWeightType.BDEF2)
                {
                    vertex.BoneIndex0 = br.ReadFromIndexSize(header.BoneIndexSize);
                    vertex.BoneIndex1 = br.ReadFromIndexSize(header.BoneIndexSize);
                    vertex.Weight0 = br.ReadSingle();
                    vertex.Weight1 = 1.0f - vertex.Weight0;
                }
                else if (vertex.WeightType == PmxVertex.PmxWeightType.BDEF4)
                {
                    vertex.BoneIndex0 = br.ReadFromIndexSize(header.BoneIndexSize);
                    vertex.BoneIndex1 = br.ReadFromIndexSize(header.BoneIndexSize);
                    vertex.BoneIndex2 = br.ReadFromIndexSize(header.BoneIndexSize);
                    vertex.BoneIndex3 = br.ReadFromIndexSize(header.BoneIndexSize);
                    vertex.Weight0 = br.ReadSingle();
                    vertex.Weight1 = br.ReadSingle();
                    vertex.Weight2 = br.ReadSingle();
                    vertex.Weight3 = br.ReadSingle();
                }
                else if (vertex.WeightType == PmxVertex.PmxWeightType.SDEF)
                {
                    vertex.BoneIndex0 = br.ReadFromIndexSize(header.BoneIndexSize);
                    vertex.BoneIndex1 = br.ReadFromIndexSize(header.BoneIndexSize);
                    vertex.Weight0 = br.ReadSingle();
                    vertex.Weight1 = 1.0f - vertex.Weight0;
                    vertex.SDEF_C = br.ReadVector3();
                    vertex.SDEF_R0 = br.ReadVector3();
                    vertex.SDEF_R1 = br.ReadVector3();
                }

                // エッジ倍率
                vertex.Edge = br.ReadSingle();

                vertList.Vertices[i] = vertex;
            }

            return vertList;
        }

        PmxFaceList ReadPmxFaceList(BinaryReader br, PmxHeader header)
        {
            var faceList = new PmxFaceList();

            // 面数を読み込み
            faceList.FaceVertNum = br.ReadInt32();
            if (faceList.FaceVertNum % 3 != 0) return null;

            // 面の読み込み
            faceList.Faces = new int[faceList.FaceVertNum];
            for (int i = 0; i < faceList.FaceVertNum; i++)
            {
                faceList.Faces[i] = br.ReadFromVertexIndexSize(header.VertexIndexSize);
            }

            return faceList;
        }

        PmxTextureList ReadPmxTextureList(BinaryReader br, PmxHeader header)
        {
            var encode = header.Encoding == 0 ? Encoding.Unicode : Encoding.UTF8;

            var textureList = new PmxTextureList();

            // テクスチャ数を読み込み
            textureList.TextureNum = br.ReadInt32();

            // 面の読み込み
            textureList.Textures = new string[textureList.TextureNum];
            for (int i = 0; i < textureList.TextureNum; i++)
            {
                textureList.Textures[i] = br.ReadString(encode);
            }

            return textureList;
        }

        PmxMaterialList ReadPmxMaterialList(BinaryReader br, PmxHeader header)
        {
            var encode = header.Encoding == 0 ? Encoding.Unicode : Encoding.UTF8;

            var matList = new PmxMaterialList();

            // マテリアルの数を読み込み
            matList.MaterialNum = br.ReadInt32();

            matList.Materials = new PmxMaterial[matList.MaterialNum];
            for (int i = 0; i < matList.MaterialNum; i++)
            {
                PmxMaterial mat = new PmxMaterial();

                mat.Name = br.ReadString(encode);                                       // 材質名
                mat.NameEng = br.ReadString(encode);                                    // 材質英名

                mat.Diffuse = br.ReadVector4();                                         // Diffuse (R,G,B,A)
                mat.Specular = br.ReadVector3();                                        // Specular (R,G,B)
                mat.SpecularPower = br.ReadSingle();                                    // Specular係数
                mat.Ambient = br.ReadVector3();                                         // Ambient (R,G,B)

                var bitFlag = br.ReadByte();
                mat.BothFace = (bitFlag & 0x01) != 0;                                   // 両面描画
                mat.GroundShadow = (bitFlag & 0x02) != 0;                               // 地面影
                mat.DrawSelfShadowMap = (bitFlag & 0x04) != 0;                          // セルフシャドウマップへの描画
                mat.SelfShadow = (bitFlag & 0x08) != 0;                                 // セルフシャドウの描画
                mat.DrawEdge = (bitFlag & 0x10) != 0;                                   // エッジ描画

                mat.EdgeColor = br.ReadVector4();                                       // エッジ色 (R,G,B,A)
                mat.EdgeSize = br.ReadSingle();                                         // エッジサイズ

                mat.AlbedoMapIndex = br.ReadFromIndexSize(header.TextureIndexSize);    // 通常テクスチャ
                mat.SphereMapIndex = br.ReadFromIndexSize(header.TextureIndexSize);     // スフィアテクスチャ
                mat.SphereMode = br.ReadByte();                                         // スフィアモード 0:無効 1:乗算(sph) 2:加算(spa) 3:サブテクスチャ(追加UV1のx,yをUV参照して通常テクスチャ描画を行う)

                mat.SharedToon = br.ReadByte() == 1;                                    // 共有Toonフラグ
                if (mat.SharedToon)
                    mat.ToonMapIndex = br.ReadFromIndexSize(header.TextureIndexSize);
                else
                    mat.ToonMapIndex = br.ReadByte();

                mat.Memo = br.ReadString(encode);                                       // メモ : 自由欄／スクリプト記述／エフェクトへのパラメータ配置など
                mat.FaceVertNum = br.ReadInt32();                                       // 材質に対応する面(頂点)数 (必ず3の倍数になる)

                matList.Materials[i] = mat;
            }

            return matList;
        }

        PmxBoneList ReadPmxBoneList(BinaryReader br, PmxHeader header)
        {
            var encode = header.Encoding == 0 ? Encoding.Unicode : Encoding.UTF8;

            var boneList = new PmxBoneList();

            // ボーンの数を読み込み
            boneList.BoneNum = br.ReadInt32();

            boneList.Bones = new PmxBone[boneList.BoneNum];
            for (var i = 0; i < boneList.BoneNum; i++)
            {
                PmxBone bone = new PmxBone();

                bone.BoneName = br.ReadString(encode);          // ボーン名
                bone.BoneNameEng = br.ReadString(encode);       // ボーン英名

                bone.Position = br.ReadVector3();               // 位置
                bone.ParentBoneIndex = br.ReadFromIndexSize(header.BoneIndexSize); // 親ボーンのボーンIndex
                bone.TransformLayer = br.ReadInt32();           // 変形階層

                var flag = br.ReadUInt16();
                bone.ConnectionVisible = (flag & 0x0001) != 0;  // 接続先(PMD子ボーン指定)表示方法
                bone.EnableRotate = (flag & 0x0002) != 0;
                bone.EnableTranslate = (flag & 0x0004) != 0;
                bone.Visible = (flag & 0x0008) != 0;
                bone.Operatable = (flag & 0x0010) != 0;
                bone.IsIK = (flag & 0x0020) != 0;
                bone.GrantLocal = (flag & 0x0080) != 0;
                bone.GrantRotate = (flag & 0x0100) != 0;
                bone.GrantTranslate = (flag & 0x0200) != 0;
                bone.FixedAxis = (flag & 0x0400) != 0;
                bone.LocalAxis = (flag & 0x0800) != 0;
                bone.AfterPhysics = (flag & 0x1000) != 0;
                bone.OuterParent = (flag & 0x2000) != 0;

                if (!bone.ConnectionVisible) bone.Offset = br.ReadVector3();
                else bone.BoneIndex = br.ReadFromIndexSize(header.BoneIndexSize);

                if (bone.GrantRotate || bone.GrantTranslate)
                {
                    bone.GrantParentBoneIndex = br.ReadFromIndexSize(header.BoneIndexSize);
                    bone.GrantParentBoneWeight = br.ReadSingle();
                }

                if (bone.FixedAxis) bone.Axis = br.ReadVector3();

                if (bone.LocalAxis)
                {
                    bone.AxisX = br.ReadVector3();
                    bone.AxisZ = br.ReadVector3();
                }

                if (bone.OuterParent) bone.Key = br.ReadInt32();

                if (bone.IsIK)
                {
                    bone.IKBoneIndex = br.ReadFromIndexSize(header.BoneIndexSize);
                    bone.IKLoop = br.ReadInt32();
                    bone.IKLoopLimitAngle = br.ReadSingle();
                    bone.IKLinkNum = br.ReadInt32();
                    bone.IKLinks = new PmxIKLink[bone.IKLinkNum];
                    for (var ik = 0; ik < bone.IKLinkNum; ik++)
                    {
                        var link = new PmxIKLink();
                        link.BoneIndex = br.ReadFromIndexSize(header.BoneIndexSize);
                        link.LimitAngle = br.ReadByte() == 1;
                        if (link.LimitAngle)
                        {
                            link.LowerLimitAngle = br.ReadVector3();
                            link.UpperLimitAngle = br.ReadVector3();
                        }
                        bone.IKLinks[ik] = link;
                    }
                }

                boneList.Bones[i] = bone;
            }

            return boneList;
        }

        PmxMorphList ReadPmxMorphList(BinaryReader br, PmxHeader header)
        {
            var encode = header.Encoding == 0 ? Encoding.Unicode : Encoding.UTF8;

            var morphList = new PmxMorphList();
            morphList.MorphNum = br.ReadInt32();

            var vertList = new List<PmxMorph<PmxMorphVertex>>();
            var uvList = new List<PmxMorph<PmxMorphUV>>();
            var uv1List = new List<PmxMorph<PmxMorphUV>>();
            var uv2List = new List<PmxMorph<PmxMorphUV>>();
            var uv3List = new List<PmxMorph<PmxMorphUV>>();
            var uv4List = new List<PmxMorph<PmxMorphUV>>();
            var boneList = new List<PmxMorph<PmxMorphBone>>();
            var matList = new List<PmxMorph<PmxMorphMaterial>>();
            var groupList = new List<PmxMorph<PmxMorphGroup>>();

            for (var i = 0; i < morphList.MorphNum; i++)
            {
                var name = br.ReadString(encode);           // モーフ名
                var nameEng = br.ReadString(encode);        // モーフ英名

                var opePanel = br.ReadByte();
                var type = br.ReadByte();
                var num = br.ReadInt32();

                if (type == 0) // グループモーフ
                {
                    var g = new PmxMorph<PmxMorphGroup>()
                    {
                        Name = name,
                        NameEng = nameEng,
                        OperatePanel = opePanel,
                        MorphType = type,
                    };
                    g.Data = new PmxMorphGroup[num];
                    for (var j = 0; j < num; j++)
                    {
                        g.Data[j] = new PmxMorphGroup();
                        g.Data[j].Index = br.ReadFromVertexIndexSize(header.MorphIndexSize);
                        g.Data[j].Rate = br.ReadSingle();
                    }
                    groupList.Add(g);
                }
                else if (type == 1) // 頂点モーフ
                {
                    var vertex = new PmxMorph<PmxMorphVertex>()
                    {
                        Name = name,
                        NameEng = nameEng,
                        OperatePanel = opePanel,
                        MorphType = type,
                    };
                    vertex.Data = new PmxMorphVertex[num];
                    for (var j = 0; j < num; j++)
                    {
                        vertex.Data[j] = new PmxMorphVertex();
                        vertex.Data[j].Index = br.ReadFromVertexIndexSize(header.VertexIndexSize);
                        vertex.Data[j].Position = br.ReadVector3();
                    }
                    vertList.Add(vertex);
                }
                else if (type == 2) // ボーンモーフ
                {
                    var bone = new PmxMorph<PmxMorphBone>()
                    {
                        Name = name,
                        NameEng = nameEng,
                        OperatePanel = opePanel,
                        MorphType = type,
                    };
                    bone.Data = new PmxMorphBone[num];
                    for (var j = 0; j < num; j++)
                    {
                        bone.Data[j] = new PmxMorphBone();
                        bone.Data[j].Index = br.ReadFromVertexIndexSize(header.BoneIndexSize);
                        bone.Data[j].Translation = br.ReadVector3();
                        bone.Data[j].Quaternion = br.ReadVector4();
                    }
                    boneList.Add(bone);
                }
                else if (type == 3 || type == 4 || type == 5 || type == 6 || type == 7) // UVモーフ
                {
                    var uv = new PmxMorph<PmxMorphUV>()
                    {
                        Name = name,
                        NameEng = nameEng,
                        OperatePanel = opePanel,
                        MorphType = type,
                    };
                    uv.Data = new PmxMorphUV[num];
                    for (var j = 0; j < num; j++)
                    {
                        uv.Data[j] = new PmxMorphUV();
                        uv.Data[j].Index = br.ReadFromVertexIndexSize(header.VertexIndexSize);
                        uv.Data[j].UV = br.ReadVector4();
                    }
                    if (type == 3) uvList.Add(uv);
                    else if (type == 4) uv1List.Add(uv);
                    else if (type == 5) uv2List.Add(uv);
                    else if (type == 6) uv3List.Add(uv);
                    else if (type == 7) uv4List.Add(uv);
                }
                else if (type == 8) // 材質モーフ
                {
                    var mat = new PmxMorph<PmxMorphMaterial>()
                    {
                        Name = name,
                        NameEng = nameEng,
                        OperatePanel = opePanel,
                        MorphType = type,
                    };
                    mat.Data = new PmxMorphMaterial[num];
                    for (var j = 0; j < num; j++)
                    {
                        mat.Data[j] = new PmxMorphMaterial();
                        mat.Data[j].Index = br.ReadFromVertexIndexSize(header.MaterialIndexSize);
                        mat.Data[j].OperationType = br.ReadByte();
                        mat.Data[j].Diffuse = br.ReadVector4();
                        mat.Data[j].Specular = br.ReadVector3();
                        mat.Data[j].SpecularPower = br.ReadSingle();
                        mat.Data[j].Ambient = br.ReadVector3();
                        mat.Data[j].EdgeColor = br.ReadVector4();
                        mat.Data[j].EdgeSize = br.ReadSingle();
                        mat.Data[j].TexCoeff = br.ReadVector4();
                        mat.Data[j].SphereTexCoeff = br.ReadVector4();
                        mat.Data[j].ToonTexCoeff = br.ReadVector4();
                    }
                    matList.Add(mat);
                }
            }

            if (vertList.Count > 0) morphList.VertexList = vertList.ToArray();
            if (uvList.Count > 0) morphList.UVList = uvList.ToArray();
            if (uv1List.Count > 0) morphList.UV1List = uv1List.ToArray();
            if (uv2List.Count > 0) morphList.UV2List = uv2List.ToArray();
            if (uv3List.Count > 0) morphList.UV3List = uv3List.ToArray();
            if (uv4List.Count > 0) morphList.UV4List = uv4List.ToArray();
            if (boneList.Count > 0) morphList.BoneList = boneList.ToArray();
            if (matList.Count > 0) morphList.MaterialList = matList.ToArray();
            if (groupList.Count > 0) morphList.GroupList = groupList.ToArray();

            return morphList;
        }
    }

    static class PmxExtension
    {
        public static Vector2 ReadVector2(this BinaryReader br)
        {
            return new Vector2(br.ReadSingle(), br.ReadSingle());
        }

        public static Vector3 ReadVector3(this BinaryReader br)
        {
            return new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        }

        public static Vector4 ReadVector4(this BinaryReader br)
        {
            return new Vector4(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        }

        public static int ReadFromVertexIndexSize(this BinaryReader br, byte vertexIndexSize)
        {
            if (vertexIndexSize == 1) return br.ReadByte();
            else if (vertexIndexSize == 2) return br.ReadUInt16();
            else if (vertexIndexSize == 4) return br.ReadInt32();
            return -1;
        }

        public static int ReadFromIndexSize(this BinaryReader br, byte indexSize)
        {
            if (indexSize == 1) return br.ReadSByte();
            else if (indexSize == 2) return br.ReadInt16();
            else if (indexSize == 4) return br.ReadInt32();
            return -1;
        }

        public static string ReadString(this BinaryReader br, Encoding encode)
        {
            var length = br.ReadInt32();
            return encode.GetString(br.ReadBytes(length));
        }
    }
}
