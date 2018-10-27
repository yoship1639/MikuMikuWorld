using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Network
{
    [DataContract]
    public class NwDirectionalLight
    {
        [DataMember]
        public Color4f Color = new Color4f(1.0f, 0.92f, 0.84f, 1.0f);

        [DataMember]
        public Vector3f Direction = new Vector3f(1.0f, -1.0f, 1.0f);

        [DataMember]
        public float Intensity = 4.0f;
    }
}
