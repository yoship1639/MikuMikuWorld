using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Network
{
    [DataContract]
    public class NwColliderMesh
    {
        [DataMember]
        public Vector3f[] Vertices;

        [DataMember]
        public int[] Indices;
    }
}
