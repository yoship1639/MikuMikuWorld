using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Walker
{
    [DataContract]
    public class LoginDesc
    {
        [DataMember]
        public string Password;

        [DataMember]
        public uint UserID;

        [DataMember]
        public string Sign;

        [DataMember]
        public string UserName;

        [DataMember]
        public Color4f UserColor;

        [DataMember]
        public string UserIcon;
    }
}
