using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModelImporter
{
    /// <summary>
    /// Xモデルデータ
    /// </summary>
    public class XModelData
    {
        public string Filepath;             // ファイルのフルパス
        public string Magic;
        public string Version;
        public string Type;
        public int FloatSize;
        public MqoMaterial[] Materials;     // マテリアルデータリスト
        public MqoObject[] Objects;         // オブジェクトリスト
    }

    /// <summary>
    /// MQOモデルのマテリアルデータ
    /// </summary>
    public class MqoMaterial
    {
        public string Name;                 // 名前
        public int Index = -1;              // 材質インデックス

        public int Shader = -1;             // シェーダ [0] Classic [1] Constant [2] Lambert [3] Phong [4] Blinn
        public int VCol = -1;               // 頂点カラー [0] なし [1] あり
        public int Doubles = -1;            // 両面表示 [0] 片面 [1] 両面
        public Color4 Color;                // 色（ＲＧＢ）、不透明度
        public float Diffuse = -1;          // 拡散光 (0.0f ~ 1.0f)
        public float Ambient = -1;          // 周囲光 (0.0f ~ 1.0f)
        public float Emissive = -1;         // 自己照明 (0.0f ~ 1.0f)
        public float Specular = -1;         // 反射光 (0.0f ~ 1.0f)
        public float SpecularPower = -1;    // 反射光の強さ (0.0f ~ 100.0f)
        public float Reflect = -1;          // 鏡面反射 (Ver4.0以降) (0.0f ~ 1.0f)
        public float Refract = -1;          // 屈折率 (Ver4.0以降) (1.0f ~ 5.0f)

        public string TextureFullpath;      // 模様マップのフルパス
        public string AlphaMapFullpath;     // 透明マップのフルパス
        public string BumpMapFullpath;      // バンプマップのフルパス

        public int ProjectType = -1;        // マッピング方式 [0] UV [1] 平面 [2] 円筒 [3] 球
        public Vector3 ProjectPos;          // 投影位置
        public Vector3 ProjectScale;        // 投影拡大率
        public Vector3 ProjectAngle;        // 投影角度 (-180 ~ 180)
    }

    /// <summary>
    /// Mqoモデルのオブジェクトデータ
    /// </summary>
    public class MqoObject
    {
        public string Name;         // 名前

        public int UID = -1;        // プラグインで利用されるユニークID。指定されない場合、自動的に連番が割り当てられる。
        public int Depth = -1;      // 階層の深さ　ルート直下を0として深くなるごとに+1
        public int Folding = -1;    // オブジェクトパネル上の階層の折りたたみ [0] 通常表示 [1] 子オブジェクトを折りたたんで非表示に
        public Vector3 Scale;       // ローカル座標の拡大率 XYZ
        public Vector3 Rotation;    // ローカル座標の回転角 HPB
        public Vector3 Translation; // ローカル座標の平行移動量 XYZ
        public int Patch = -1;      // 曲面の形式 [0] 平面(曲面指定をしない) [1] 曲面タイプ１ （スプライン Type1） [2] 曲面タイプ２ （スプライン Type2） [3] Catmull-Clark （Ver2.2以降） [4] OpenSubdiv （Ver4.0以降）
        public int PatchTri = -1;   // Catmull-Clark曲面の三角形面の処理 [0] 四角形に分割 [1] 三角形のまま分割
        public int Segment = -1;    // 曲面の分割数 1～16 (Catmull-Clark/OpenSubdivの場合、再帰分割数を示すため1～4となる）
        public int Visible = -1;    // 表示・非表示 [0] 非表示 [15] 表示
        public int Locking = -1;    // オブジェクトの固定 [0] 編集可能 [1] 編集禁止
        public int Shading = -1;    // シェーディング [0] フラットシェーディング [1] グローシェーディング
        public float Facet = -1;    // スムージング角度 0～180
        public Color4 Color;        // 色（ＲＧＢ）それぞれ0～1
        public int ColorType = -1;  // 辺の色タイプ [0] 環境設定での色を使用 [1] オブジェクト固有の色を使用
        public int Mirror = -1;     // 鏡面のタイプ [0] なし [1] 左右を分離 [2] 左右を接続
        public int MirrorAxis = -1; // 鏡面の適用軸 [1] X軸 [2] Y軸 [4] Z軸
        public float MirrorDis = -1;// 接続距離 0～
        public int Lathe = -1;      // 回転体のタイプ [0] なし [3] 両面
        public int LatheAxis = -1;  // 回転体の軸 [0] X軸 [1] Y軸 [2] Z軸
        public int LatheSeg = -1;   // 回転体の分割数 3～
        public Vector3[] Vertices;  // 頂点データリスト
        public MqoFace[] Faces;     // 面リスト
    }

    /// <summary>
    /// Mqoオブジェクトの面データ
    /// </summary>
    public class MqoFace
    {
        public int VertexNum = -1;      // 頂点数 2以上の値 フォーマットバージョンが1.0のときは常に4以下
        public int[] Indices;           // 頂点インデックス 0～頂点数-1 （頂点数分の数が存在）
        public int MaterialIndex = -1;  // 材質インデックス -1（未着色面）または0～材質数-1
        public Vector2[] UVs;           // UV値 (頂点数分の数が存在)
        public Color4[] VertexColors;   // 頂点カラー （頂点数分の数が存在） 通常はnull
        public float[] CRS;             // Catmull-Clark/OpenSubdiv曲面用のエッジの折れ目（頂点数分の数が存在）Catmull-Clarkの場合 [0] OFF [1] ON OpenSubdivの場合0以上の値 通常はnull
    }
}
