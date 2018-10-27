using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Assets
{
    public class ColliderMesh : IAsset
    {
        public Vector3[] Vertices { get; set; }
        public int[] Indices { get; set; }

        public string Name { get; set; }
        public bool Loaded => true;
        public Result Load() => Result.Success;
        public Result Unload() => Result.Success;
    }
}
