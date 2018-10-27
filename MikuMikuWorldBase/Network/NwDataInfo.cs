using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Network
{
    [DataContract]
    public class NwDataInfo
    {
        [DataMember]
        public int Size;

        [DataMember]
        public string Hash;

        public NwDataInfo() { }
        public NwDataInfo(string hash, int size)
        {
            Hash = hash;
            Size = size;
        }
    }
}
