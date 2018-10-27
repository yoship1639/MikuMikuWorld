using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    static class Buffer
    {
        public static T JsonDeserialize<T>(this byte[] data)
        {
            T res = default(T);
            Read(data, (br) =>
            {
                var json = br.ReadString();
                res = Util.DeserializeJson<T>(json);
            });
            return res;
        }

        public static void Read(byte[] buf, Action<BinaryReader> act)
        {
            using (var ms = new MemoryStream(buf))
            {
                using (var br = new BinaryReader(ms))
                {
                    act(br);
                }
            }
        }
        public static void Read(byte[] buf, Action<MemoryStream, BinaryReader> act)
        {
            using (var ms = new MemoryStream(buf))
            {
                using (var br = new BinaryReader(ms))
                {
                    act(ms, br);
                }
            }
        }

        public static byte[] Write(Action<BinaryWriter> act)
        {
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    act(bw);
                }
                return ms.ToArray();
            }
        }
    }
}
