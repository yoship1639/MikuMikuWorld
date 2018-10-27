using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    public struct BoneWeight
    {
        public int boneIndex0;
        public int boneIndex1;
        public int boneIndex2;
        public int boneIndex3;
        public float weight0;
        public float weight1;
        public float weight2;
        public float weight3;

        public float[] ToFloats()
        {
            return new float[]
            {
                boneIndex0,
                boneIndex1,
                boneIndex2,
                boneIndex3,
                weight0,
                weight1,
                weight2,
                weight3,
            };
        }
    }
}
