using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PmdModelImporter
{
    /// <summary>
    /// PMDモデルデータ
    /// </summary>
    public class PmdModelData
    {
        public PmdHeader Header;
        public PmdVertexList VertexList;
        public PmdFaceList FaceList;
        public PmdMaterialList MaterialList;
        public PmdBoneList BoneList;
        public PmdIKList IKList;
        public PmdMorphList MorphList;
    }

    // PMDヘッダ
    public class PmdHeader
    {
        public string Magic;                // "Pmd"
        public float Version;               // 00 00 80 3F == 1.00
        public string Name;                 // モデルの名前
        public string Comment;              // コメント

        public PmdHeader(string magic, float version, string name, string comment)
        {
            Magic = magic;
            Version = version;
            Name = name;
            Comment = comment;
        }
    }

    // PMD頂点リスト
    public class PmdVertexList
    {
        public int VertexNum;               // 頂点数
        public PmdVertex[] Vertices;        // 頂点配列

        public PmdVertexList(int vertNum, PmdVertex[] vertices)
        {
            VertexNum = vertNum;
            Vertices = vertices;
        }
    }

    // PMD頂点
    public class PmdVertex
    {
        public Vector3 Position;            // 座標
        public Vector3 Normal;              // 法線
        public Vector2 UV;                  // UV
        public int BoneIndex0;              // ボーン番号0
        public int BoneIndex1;              // ボーン番号1
        public float Weight;                // 影響度
        public bool HasEdge;                // エッジフラグ
    }

    // PMD面頂点
    public class PmdFaceList
    {
        public int FaceNum;         // 面数
        public ushort[] Indices;     // インデックス (3 * 面数)

        public PmdFaceList(int faceNum, ushort[] indices)
        {
            FaceNum = faceNum;
            Indices = indices;
        }
    }

    // PMDマテリアルリスト
    public class PmdMaterialList
    {
        public int MaterialNum;
        public PmdMaterial[] Materials;

        public PmdMaterialList(int matNum, PmdMaterial[] materials)
        {
            MaterialNum = matNum;
            Materials = materials;
        }
    }

    // PMDマテリアル
    public class PmdMaterial
    {
        public Vector3 DiffuseColor;
        public float Alpha;
        public float SpecularPower;
        public Vector3 SpecularColor;
        public Vector3 AmbientColor;
        public byte ToonIndex;
        public byte EdgeFlag;
        public uint FaceVertNum;
        public string TextureFileName;
    }

    // PMDボーンリスト
    public class PmdBoneList
    {
        public ushort BoneNum;
        public PmdBone[] Bones;

        public PmdBoneList(ushort boneNum, PmdBone[] bones)
        {
            BoneNum = boneNum;
            Bones = bones;
        }
    }

    // PMDボーン
    public class PmdBone
    {
        public string Name;             // 名前
        public short ParentIndex;       // 親ボーンの番号
        public short TailIndex;         // 子ボーンの番号
        public BoneType Type;           // ボーンの種類
        public short IKIndex;           // 影響IKボーンの番号
        public Vector3 HeadPos;         // ボーンのヘッドの位置

        public override string ToString()
        {
            return Name + ": " + HeadPos;
        }

        public enum BoneType
        {
            Rot,
            RotAndLoc,
            IK,
            Unknown,
            UnderIK,
            UnderRot,
            IKConnect,
            Invisible,
        }
    }

    // PMDのIKリスト
    public class PmdIKList
    {
        public ushort IKNum;
        public PmdIK[] IKs;

        public PmdIKList(ushort iKNum, PmdIK[] iKs)
        {
            IKNum = iKNum;
            IKs = iKs;
        }
    }

    // PMDのIK
    public class PmdIK
    {
        public ushort Index;                    // IKボーン番号
        public ushort TargetIndex;              // IKターゲットボーン番号
        public byte ChainLength;                // IKチェーンの長さ (子の数)
        public ushort Itarations;               // 再帰演算回数
        public float ControlWeight;             // 演算1回あたりの制限角度
        public ushort[] ChildIndices;           // IK影響下のボーン番号の配列
    }

    // PMDの表情リスト
    public class PmdMorphList
    {
        public ushort MorphNum;
        public PmdMorph[] Morphs;

        public PmdMorphList(ushort morphNum, PmdMorph[] morphs)
        {
            MorphNum = morphNum;
            Morphs = morphs;
        }
    }

    // PMDの表情
    public class PmdMorph
    {
        public string Name;                     // 名前
        public uint SkinVertCount;              // 表情用の頂点数
        public byte SkinType;                   // 表情の種類 // 0：base、1：まゆ、2：目、3：リップ、4：その他
        public PmdMorphVertex[] Data;           // 表情用の頂点のデータ(16Bytes/vert)

        public override string ToString()
        {
            return Name + ": " + SkinType;
        }
    }

    public class PmdMorphVertex
    {
        public uint Index;
        public Vector3 Offset;

        public override string ToString()
        {
            return Index.ToString();
        }
    }
}
