using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Network
{
    [DataContract]
    public class NwRequest
    {
        [DataMember]
        public string Hash;

        public NwRequest(string hash)
        {
            Hash = hash;
        }
    }
}
