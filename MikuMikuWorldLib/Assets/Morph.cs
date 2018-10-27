using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Assets
{
    [DataContract]
    public class Morph : IAsset
    {
        public bool Loaded { get; protected set; }

        [DataMember(Name = "name", EmitDefaultValue = false, Order = 0)]
        public string Name { get; set; }

        public VertexMorph[] Vertices;
        public BoneMorph[] Bones;

        public Result Load()
        {
            return Result.Success;
        }
        public Result Unload()
        {
            return Result.Success;
        }

        [DataMember(Name = "vertices", EmitDefaultValue = false, Order = 1)]
        private string vertices
        {
            get { if (Vertices == null) return null; return Vertices.ToBase64String(); }
            set { Vertices = value.ToVertexMotphs(); }
        }

        public override string ToString()
        {
            return string.Format($"{Name}");
        }
    }

    /// <summary>
    /// 頂点モーフィング
    /// </summary>
    public class VertexMorph
    {
        public int Index;
        public Vector3 Offset;
    }
    public static class MorphExtension
    {
        public static string ToBase64String(this VertexMorph[] vertices)
        {
            var list = new List<byte>();
            foreach (var v in vertices)
            {
                list.AddRange(BitConverter.GetBytes(v.Index));
                list.AddRange(v.Offset.ToBytes());
            }
            return Convert.ToBase64String(list.ToArray());
        }
        public static VertexMorph[] ToVertexMotphs(this string s)
        {
            var buf = Convert.FromBase64String(s);
            var vms = new VertexMorph[buf.Length / (4 * 4)];
            for (var i = 0; i < vms.Length; i++)
            {
                vms[i] = new VertexMorph();
                vms[i].Index = BitConverter.ToInt32(buf, (i * 4 * 4) + 0);
                vms[i].Offset.X = BitConverter.ToSingle(buf, (i * 4 * 4) + 4);
                vms[i].Offset.Y = BitConverter.ToSingle(buf, (i * 4 * 4) + 8);
                vms[i].Offset.Z = BitConverter.ToSingle(buf, (i * 4 * 4) + 12);
            }
            return vms;
        }
    }

    /// <summary>
    /// ボーンモーフィング
    /// </summary>
    public class BoneMorph
    {
        public int Index;
        public Vector3 Location;
        public Quaternion Rotation;
    }
}
