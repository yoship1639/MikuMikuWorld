using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PmxModelImporter
{
    public class PmxModelData
    {
        public PmxHeader Header;                // ヘッダ
        public PmxModelInfo ModelInfo;          // モデル情報
        public PmxVertexList VertexList;        // 頂点リスト
        public PmxFaceList FaceList;            // 面リスト
        public PmxTextureList TextureList;      // テクスチャリスト
        public PmxMaterialList MaterialList;    // 材質リスト
        public PmxBoneList BoneList;            // ボーンリスト
        public PmxMorphList MorphList;          // モーフリスト
    }

    /// <summary>
    /// Pmxヘッダ
    /// </summary>
    public class PmxHeader
    {
        public string Magic;            // "PMX" (PMX1.0は"Pmx")
        public float Version;           // バージョン：2.0 / 2.1

        public byte Encoding;           // エンコード方式：0:UTF16, 1:UTF8
        public byte AddUVCound;         // 追加UVの数：0~4

        public byte VertexIndexSize;    // 頂点Indexサイズ：1,2,4
        public byte TextureIndexSize;   // テクスチャIndexサイズ：1,2,4
        public byte MaterialIndexSize;  // 材質Indexサイズ：1,2,4
        public byte BoneIndexSize;      // ボーンIndexサイズ：1,2,4
        public byte MorphIndexSize;     // モーフIndexサイズ：1,2,4
        public byte RigidIndexSize;     // 剛体Indexサイズ：1,2,4

        public override string ToString()
        {
            return string.Format("{0}:{1}", Magic, Version);
        }
    }

    /// <summary>
    /// Pmxモデル情報
    /// </summary>
    public class PmxModelInfo
    {
        public string ModelName;        // モデル名
        public string ModelNameEng;     // モデル英名
        public string Comment;          // コメント
        public string CommentEng;       // 英語コメント

        public override string ToString()
        {
            return string.Format("{0}", ModelName);
        }
    }

    /// <summary>
    /// Pmx頂点リスト
    /// </summary>
    public class PmxVertexList
    {
        public int VertexNum;           // 頂点数
        public PmxVertex[] Vertices;    // 頂点配列

        public override string ToString()
        {
            return string.Format("VertexNum: {0}", VertexNum);
        }
    }

    /// <summary>
    /// Pmx頂点
    /// </summary>
    public class PmxVertex
    {
        public Vector3 Position;            // 位置
        public Vector3 Normal;              // 法線
        public Vector2 UV;                  // UV
        public Vector4 UV1;                 // 追加UV1
        public Vector4 UV2;                 // 追加UV2
        public Vector4 UV3;                 // 追加UV3
        public Vector4 UV4;                 // 追加UV4

        public PmxWeightType WeightType;    // ウェイト変形方式
        public int BoneIndex0 = -1;         // ボーンIndex0
        public int BoneIndex1 = -1;         // ボーンIndex1
        public int BoneIndex2 = -1;         // ボーンIndex2
        public int BoneIndex3 = -1;         // ボーンIndex3
        public float Weight0 = 1.0f;        // ボーンウェイト0
        public float Weight1 = 0.0f;        // ボーンウェイト1
        public float Weight2 = 0.0f;        // ボーンウェイト2
        public float Weight3 = 0.0f;        // ボーンウェイト3 (ウェイト計1.0の保障はない)
        public Vector3 SDEF_C;              // SDEF-C値(x,y,z)
        public Vector3 SDEF_R0;             // SDEF-R0値(x,y,z)
        public Vector3 SDEF_R1;             // SDEF-R1値(x,y,z) ※修正値を要計算

        public float Edge;                  // エッジ倍率  材質のエッジサイズに対しての倍率値

        public enum PmxWeightType
        {
            BDEF1,                      // 単一ボーン
            BDEF2,                      // Bone0のウェイト値はWeight0, Bone1のウェイト値は1.0 - Weight0
            BDEF4,
            SDEF,
        }

        public override string ToString()
        {
            return string.Format("Pos{0}, Norm{1}", Position, Normal);
        }
    }

    /// <summary>
    /// Pmx面リスト
    /// </summary>
    public class PmxFaceList
    {
        public int FaceVertNum;         // 面数(頂点数)
        public int[] Faces;             // 面配列(Index配列)

        public override string ToString()
        {
            return string.Format("FaceVertNum: {0}", FaceVertNum);
        }
    }

    /// <summary>
    /// Pmxテクスチャリスト
    /// </summary>
    public class PmxTextureList
    {
        public int TextureNum;          // テクスチャ数
        public string[] Textures;       // テクスチャ名配列

        public override string ToString()
        {
            return string.Format("TextureNum: {0}", TextureNum);
        }
    }

    /// <summary>
    /// Pmx材質リスト
    /// </summary>
    public class PmxMaterialList
    {
        public int MaterialNum;         // 材質数
        public PmxMaterial[] Materials; // 材質配列

        public override string ToString()
        {
            return string.Format("MaterialNum: {0}", MaterialNum);
        }
    }

    /// <summary>
    /// Pmx材質
    /// </summary>
    public class PmxMaterial
    {
        public string Name;             // 材質名
        public string NameEng;          // 材質英名

        public Vector4 Diffuse;         // Diffuse (R,G,B,A)
        public Vector3 Specular;        // Specular (R,G,B)
        public float SpecularPower;     // Specular係数
        public Vector3 Ambient;         // Ambient (R,G,B)
        public bool BothFace;           // 両面描画
        public bool GroundShadow;       // 地面影
        public bool DrawSelfShadowMap;  // セルフシャドウマップへの描画
        public bool SelfShadow;         // セルフシャドウの描画
        public bool DrawEdge;           // エッジ描画
        public Vector4 EdgeColor;       // エッジ色 (R,G,B,A)
        public float EdgeSize;          // エッジサイズ
        public int AlbedoMapIndex;     // 通常テクスチャ, テクスチャテーブルの参照Index
        public int SphereMapIndex;      // スフィアテクスチャ, テクスチャテーブルの参照Index  ※テクスチャ拡張子の制限なし
        public byte SphereMode;         // 0:無効 1:乗算(sph) 2:加算(spa) 3:サブテクスチャ(追加UV1のx,yをUV参照して通常テクスチャ描画を行う)
        public bool SharedToon;         // 共有Toonフラグ
        public int ToonMapIndex;        // Toonテクスチャ, ShareToon:true(テクスチャテーブルの参照Index), ShareToon:false(共有Toonテクスチャ[0～9] -> それぞれ toon01.bmp～toon10.bmp に対応)

        public string Memo;             // メモ : 自由欄／スクリプト記述／エフェクトへのパラメータ配置など
        public int FaceVertNum;         // 材質に対応する面(頂点)数 (必ず3の倍数になる)

        public override string ToString()
        {
            return string.Format("{0}", Name);
        }
    }

    /// <summary>
    /// Pmxボーンリスト
    /// </summary>
    public class PmxBoneList
    {
        public int BoneNum;         // 材質数
        public PmxBone[] Bones;     // 材質配列

        public override string ToString()
        {
            return string.Format("BoneNum: {0}", BoneNum);
        }
    }

    /// <summary>
    /// Pmxボーン
    /// </summary>
    public class PmxBone
    {
        public string BoneName;             // ボーン名
        public string BoneNameEng;          // ボーン英名

        public Vector3 Position;            // 位置
        public int ParentBoneIndex;         // 親ボーンindex
        public int TransformLayer;          // 変形階層

        public bool ConnectionVisible;      // 接続先(PMD子ボーン指定)表示方法  false:座標オフセットで指定 true:ボーンで指定
        public bool EnableRotate;           // 回転可能
        public bool EnableTranslate;        // 移動可能
        public bool Visible;                // 表示
        public bool Operatable;             // 操作可能
        public bool IsIK;                   // IK
        public bool GrantLocal;             // ローカル付与 | false:ユーザー変形値／IKリンク／多重付与 true:親のローカル変形量
        public bool GrantRotate;            // 回転付与
        public bool GrantTranslate;         // 移動付与
        public bool FixedAxis;              // 軸固定
        public bool LocalAxis;              // ローカル軸
        public bool AfterPhysics;           // 物理後変形
        public bool OuterParent;            // 外部親変形

        public Vector3 Offset;              // 座標オフセット, ボーン位置からの相対分
        public int BoneIndex;               // 接続先ボーンのボーンIndex

        public int GrantParentBoneIndex;    // 付与親ボーンのボーンIndex
        public float GrantParentBoneWeight; // 付与率

        public Vector3 Axis;                // 軸の方向ベクトル

        public Vector3 AxisX;               // X軸の方向ベクトル
        public Vector3 AxisZ;               // Z軸の方向ベクトル ※フレーム軸算出方法は後述

        public int Key;                     // Key値

        public int IKBoneIndex;             // IKターゲットボーンのボーンIndex
        public int IKLoop;                  // IKループ回数 (PMD及びMMD環境では255回が最大になるようです)
        public float IKLoopLimitAngle;      // IKループ計算時の1回あたりの制限角度 -> ラジアン角 | PMDのIK値とは4倍異なるので注意

        public int IKLinkNum;               // IKリンク数 : 後続の要素数
        public PmxIKLink[] IKLinks;         // IKリンク配列

        public override string ToString()
        {
            return string.Format("{0}:{1}", BoneName, Position);
        }
    }

    /// <summary>
    /// IKリンク
    /// </summary>
    public class PmxIKLink
    {
        public int BoneIndex;               // リンクボーンのボーンIndex
        public bool LimitAngle;             // 角度制限 false:OFF true:ON

        public Vector3 LowerLimitAngle;     // 下限 (x,y,z) -> ラジアン角
        public Vector3 UpperLimitAngle;     // 上限 (x,y,z) -> ラジアン角

        public override string ToString()
        {
            return BoneIndex.ToString();
        }
    }

    /// <summary>
    /// Pmxモーフリスト
    /// </summary>
    public class PmxMorphList
    {
        public int MorphNum;                              // モーフ数
        public PmxMorph<PmxMorphVertex>[] VertexList;     // 頂点モーフリスト
        public PmxMorph<PmxMorphUV>[] UVList;             // UVモーフリスト
        public PmxMorph<PmxMorphUV>[] UV1List;            // UV1モーフリスト
        public PmxMorph<PmxMorphUV>[] UV2List;            // UV2モーフリスト
        public PmxMorph<PmxMorphUV>[] UV3List;            // UV3モーフリスト
        public PmxMorph<PmxMorphUV>[] UV4List;            // UV4モーフリスト
        public PmxMorph<PmxMorphBone>[] BoneList;         // ボーンモーフリスト
        public PmxMorph<PmxMorphMaterial>[] MaterialList; // マテリアルモーフリスト
        public PmxMorph<PmxMorphGroup>[] GroupList;       // グループモーフリスト
    }

    /// <summary>
    /// Pmxモーフ
    /// </summary>
    public class PmxMorph<T> : APmxMorph
    {
        public T[] Data;
    }

    /// <summary>
    /// Pmxモーフ(抽象)
    /// </summary>
    public abstract class APmxMorph
    {
        public string Name;                 // モーフ名
        public string NameEng;              // モーフ名英

        public byte OperatePanel;           // 操作パネル (PMD:カテゴリ) 1:眉(左下) 2:目(左上) 3:口(右上) 4:その他(右下)  | 0:システム予約
        public byte MorphType;              // モーフ種類 - 0:グループ, 1:頂点, 2:ボーン, 3:UV, 4:追加UV1, 5:追加UV2, 6:追加UV3, 7:追加UV4, 8:材質

        public override string ToString()
        {
            return Name;
        }
    }

    public class PmxMorphVertex
    {
        public int Index;
        public Vector3 Position;
    }

    public class PmxMorphUV
    {
        public int Index;
        public Vector4 UV;
    }

    public class PmxMorphBone
    {
        public int Index;
        public Vector3 Translation;
        public Vector4 Quaternion;
    }

    public class PmxMorphMaterial
    {
        public int Index;
        public byte OperationType;      // 0:乗算, 1:加算 - 詳細は後述

        public Vector4 Diffuse;         // Diffuse (R,G,B,A) - 乗算:1.0／加算:0.0 が初期値となる(同以下)
        public Vector3 Specular;        // Specular (R,G,B)
        public float SpecularPower;     // Specular係数
        public Vector3 Ambient;         // Ambient (R,G,B)
        public Vector4 EdgeColor;       // エッジ色 (R,G,B,A)
        public float EdgeSize;          // エッジサイズ
        public Vector4 TexCoeff;        // テクスチャ係数 (R,G,B,A)
        public Vector4 SphereTexCoeff;  // スフィアテクスチャ係数 (R,G,B,A)
        public Vector4 ToonTexCoeff;    // Toonテクスチャ係数 (R,G,B,A)
    }

    public class PmxMorphGroup
    {
        public int Index;               // モーフIndex  ※仕様上グループモーフのグループ化は非対応とする
        public float Rate;              // モーフ率 : グループモーフのモーフ値 * モーフ率 = 対象モーフのモーフ値
    }
}
