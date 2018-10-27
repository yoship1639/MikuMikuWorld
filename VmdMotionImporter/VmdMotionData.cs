using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VmdMotionImporter
{
    public class VmdMotionData
    {
        public VmdHeader Header;
        public VmdMotionList MotionList;
        public VmdSkinList SkinList;
    }

    // ヘッダ
    public class VmdHeader
    {
        public string Magic;
        public string ModelName;
    }

    // モーションリスト
    public class VmdMotionList
    {
        public uint MotionNum;
        public VmdMotion[] Motions;
    }

    // モーション
    public class VmdMotion
    {
        public string Name;
        public uint FrameNo;
        public Vector3 Location;
        public Vector4 Rotation;

        public Vector4 BezierX1;
        public Vector4 BezierY1;
        public Vector4 BezierX2;
        public Vector4 BezierY2;
    }

    // 表情リスト
    public class VmdSkinList
    {
        public uint SkinNum;
        public VmdSkin[] Skins;
    }

    // 表情
    public class VmdSkin
    {
        public string Name;
        public uint FrameNo;
        public float Weight;
    }
}
