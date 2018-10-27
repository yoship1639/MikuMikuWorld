using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Network
{
    [DataContract]
    public class NwMesh
    {
        [DataMember]
        public string Name;

        [DataMember]
        public Vector3f[] Vertices;

        [DataMember]
        public Vector3f[] Normals;

        [DataMember]
        public Color4f[] Colors;

        [DataMember]
        public Vector2f[] UVs;

        [DataMember]
        public Vector4f[] UV1s;

        [DataMember]
        public Vector4f[] UV2s;

        [DataMember]
        public Vector4f[] UV3s;

        [DataMember]
        public Vector4f[] UV4s;

        [DataMember]
        public BoneWeight4f[] BoneWeights;

        [DataMember]
        public NwSubMesh[] SubMeshes;
    }

    [DataContract]
    public class NwSubMesh
    {
        [DataMember]
        public int MatIndex;

        [DataMember]
        public int[] Indices;

        [DataMember]
        public int BeginMode;
    }
}
