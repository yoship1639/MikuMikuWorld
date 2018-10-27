using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld_Walker_Server
{
    static class Extension
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

        public static Color4 ReadColor(this BinaryReader br)
        {
            return new Color4(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        }

        public static Bitmap ReadBitmap(this BinaryReader br)
        {
            var l = br.ReadInt32();
            var buf = br.ReadBytes(l);

            using (var ms = new MemoryStream(buf))
            {
                var img = Image.FromStream(ms);
                return (Bitmap)img;
            }
        }
    }
}
