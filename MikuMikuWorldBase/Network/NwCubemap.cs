using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Network
{
    [DataContract]
    public class NwCubemap
    {
        [DataMember]
        public string Hash;

        [DataMember]
        public string Name;

        [DataMember]
        public byte[] ImagePX;

        [DataMember]
        public byte[] ImagePY;

        [DataMember]
        public byte[] ImagePZ;

        [DataMember]
        public byte[] ImageNX;

        [DataMember]
        public byte[] ImageNY;

        [DataMember]
        public byte[] ImageNZ;
    }
}
