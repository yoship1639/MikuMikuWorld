using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Network
{
    public class NwCharacter
    {
        public string Hash;
        public string Name { get; set; }
        public string Description;
        public string Version;
        public string Editor;
        public string EditorURL;
        public string Tags;
        public Dictionary<string, string> Properties = new Dictionary<string, string>();

        public NwMaterial[] Materials = new NwMaterial[0];
        public NwTexture2D[] Texture2Ds = new NwTexture2D[0];
        public NwCubemap[] Cubemaps = new NwCubemap[0];
        public NwMesh Mesh;
        public NwBone[] Bones = new NwBone[0];
        public NwMorph[] Morphs = new NwMorph[0];
        public NwMotion[] Motions = new NwMotion[0];
        public NwSound[] Sounds = new NwSound[0];
        public NwGameObjectScript[] Scripts = new NwGameObjectScript[0];

        public NwCollisionShape CollisionShape;

        public Vector3f EyePosition;
        public float Mass;
    }
}
