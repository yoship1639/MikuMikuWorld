using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Network
{
    public class NwWorld
    {
        public string Hash;
        public string Name { get; set; }
        public string Description;
        public string Version;
        public string Editor;
        public string EditorURL;

        public NwObject[] Objects = new NwObject[0];
        public NwMaterial[] Materials = new NwMaterial[0];
        public NwMesh[] Meshes = new NwMesh[0];
        public NwColliderMesh[] ColliderMeshes = new NwColliderMesh[0];
        public NwEnvironment[] Environments = new NwEnvironment[0];
        public NwTexture2D[] Texture2Ds = new NwTexture2D[0];
        public NwCubemap[] Cubemaps = new NwCubemap[0];
    }
}
