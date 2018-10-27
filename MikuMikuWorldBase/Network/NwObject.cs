using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Network
{
    public class NwObject
    {
        public string Hash;
        public string Name { get; set; }
        public string Description;
        public string Version;
        public string Editor;
        public string EditorURL;
        public string Tags;
        public Dictionary<string, string> Properties = new Dictionary<string, string>();

        public long ItemPrice { get; set; }
        public string ItemType { get; set; }
        public int MaxStack { get; set; } = 1;
        public bool ItemConsume { get; set; }
        public bool Sync { get; set; }
        public bool Purchasable { get; set; }

        public bool RemovablePrincipal { get; set; }
        public bool RemovableHost { get; set; }
        public bool RemovableOthers { get; set; }
        public bool PickablePrincipal { get; set; }
        public bool PickableHost { get; set; }
        public bool PickableOthers { get; set; }

        public NwMaterial[] Materials = new NwMaterial[0];
        public NwTexture2D[] Texture2Ds = new NwTexture2D[0];
        public NwCubemap[] Cubemaps = new NwCubemap[0];
        public NwMesh Mesh;
        public NwBone[] Bones = new NwBone[0];
        public NwMorph[] Morphs = new NwMorph[0];
        public NwMotion[] Motions = new NwMotion[0];
        public NwSound[] Sounds = new NwSound[0];
        public NwGameObjectScript[] Scripts = new NwGameObjectScript[0];

        public NwPhysicalMaterial PhysicalMaterial;
        public NwCollisionShape CollisionShape;
        public NwTexture2D Thumbnail;

        public bool EnableScript;
        
    }
}
