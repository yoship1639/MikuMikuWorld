using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Network
{
    [DataContract]
    public class NwPlayer
    {
        [DataMember]
        public int ID;

        [DataMember]
        public string Name;

        [DataMember]
        public Color4f Color;

        [DataMember]
        public string Icon;

        [DataMember]
        public int SessionID;
    }
}
