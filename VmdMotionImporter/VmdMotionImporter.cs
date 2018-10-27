using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VmdMotionImporter
{
    public class VmdImportResult
    {
        public enum Result
        {
            Success,
            FileNotFound,
            NotPmxFormat,
            Failed,
        }

        public VmdMotionData vmd;
        public Result result;
    }

    public class VmdMotionImporter
    {
        public string Extension { get { return ".vmd"; } }

        public VmdImportResult Import(string filename, bool full = true)
        {
            var result = new VmdImportResult()
            {
                result = VmdImportResult.Result.FileNotFound,
            };

            if (!File.Exists(filename)) return result;

            result.result = VmdImportResult.Result.Failed;

            using (BinaryReader br = new BinaryReader(new MemoryStream(File.ReadAllBytes(filename))))
            {
                var vmd = new VmdMotionData();

                // ヘッダの読み込み
                var header = ReadVmdHeader(br);
                if (header == null)
                {
                    result.result = VmdImportResult.Result.NotPmxFormat;
                    return result;
                }

                if (!full)
                {
                    result.result = VmdImportResult.Result.Success;
                    vmd.Header = header;
                    result.vmd = vmd;
                    return result;
                }

                // モーションリストの読み込み
                var motionList = ReadVmdMotions(br);
                if (motionList == null) return result;

                // 表情リストの読み込み
                var skinList = ReadVmdSkins(br);
                if (skinList == null) return result;

                result.result = VmdImportResult.Result.Success;

                vmd.Header = header;
                vmd.MotionList = motionList;
                vmd.SkinList = skinList;

                result.vmd = vmd;
            }

            return result;
        }

        private VmdHeader ReadVmdHeader(BinaryReader br)
        {
            var header = new VmdHeader();

            // マジック
            header.Magic = br.ReadString(30);
            if (!header.Magic.Contains("Vocaloid Motion Data 0002")) return null;

            // モデル名
            header.ModelName = br.ReadString(20);

            return header;
        }
        private VmdMotionList ReadVmdMotions(BinaryReader br)
        {
            var list = new VmdMotionList();

            list.MotionNum = br.ReadUInt32();
            list.Motions = new VmdMotion[list.MotionNum];
            for (var i = 0; i < list.MotionNum; i++)
            {
                list.Motions[i] = new VmdMotion();

                list.Motions[i].Name = br.ReadString(15);
                list.Motions[i].FrameNo = br.ReadUInt32();
                list.Motions[i].Location = br.ReadVector3();
                list.Motions[i].Rotation = br.ReadVector4();

                var tmp = br.ReadBytes(64);
                var b = new float[64];
                for (var j = 0; j < 64; j++) b[j] = tmp[j] / 127.0f;

                list.Motions[i].BezierX1 = new Vector4(b[0], b[1], b[2], b[3]);
                list.Motions[i].BezierY1 = new Vector4(b[4], b[5], b[6], b[7]);
                list.Motions[i].BezierX2 = new Vector4(b[8], b[9], b[10], b[11]);
                list.Motions[i].BezierY2 = new Vector4(b[12], b[13], b[14], b[15]);
            }

            return list;
        }
        private VmdSkinList ReadVmdSkins(BinaryReader br)
        {
            var list = new VmdSkinList();

            list.SkinNum = br.ReadUInt32();
            list.Skins = new VmdSkin[list.SkinNum];
            for (var i = 0; i < list.SkinNum; i++)
            {
                list.Skins[i] = new VmdSkin();

                list.Skins[i].Name = br.ReadString(15);
                list.Skins[i].FrameNo = br.ReadUInt32();
                list.Skins[i].Weight = br.ReadSingle();
            }

            return list;
        }
    }

    static class VmdExtension
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

        static Encoding enc = Encoding.GetEncoding("Shift_JIS");

        public static string ReadString(this BinaryReader br, int count)
        {
            var str = enc.GetString(br.ReadBytes(count));
            var idx = str.IndexOf('\0');
            return str.Substring(0, idx);
        }
    }
}
