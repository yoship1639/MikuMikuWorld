using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Network
{
    [DataContract]
    public class NwColorCollect
    {
        [DataMember]
        public float Contrast = 1.0f;

        [DataMember]
        public float Saturation = 1.0f;

        [DataMember]
        public float Hue = 0.0f;

        [DataMember]
        public float Brightness = 1.0f;

        [DataMember]
        public float Gamma = 2.2f;
    }
}
