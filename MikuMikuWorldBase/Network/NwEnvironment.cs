using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Network
{
    [DataContract]
    public class NwEnvironment
    {
        [DataMember]
        public Color4f Ambient = new Color4f(0.2f, 0.2f, 0.2f, 0.0f);

        [DataMember]
        public bool CastShadow = true;

        [DataMember]
        public NwDirectionalLight DirLight = new NwDirectionalLight();

        [DataMember]
        public NwColorCollect ColorCollect = new NwColorCollect();

        [DataMember]
        public string EnvMap;
    }
}
