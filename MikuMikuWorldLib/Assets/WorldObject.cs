using MikuMikuWorld.GameComponents;
using MikuMikuWorld.Network;
using MikuMikuWorldScript;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Assets
{
    public class WorldObject : IAsset
    {
        public string Name { get; set; }
        public string Desc { get; set; }
        public string[] Tags { get; set; }

        public NwObject nwObject;
        public string Hash { get; set; }
        public Bitmap Icon { get; set; }

        public string ItemType { get; set; }
        public int MaxStack { get; set; }
        public int Price { get; set; }
        public bool Consume { get; set; }
        public bool Purchasable { get; set; }
        public bool Sync { get; set; }



        public Material[] Materials;
        public Mesh Mesh;
        public Bone[] Bones;
        public Motion[] Motions;
        public Morph[] Morphs;
        public Texture2D[] Texture2Ds;
        public TextureCube[] TextureCubes;
        public Sound[] Sounds;
        public CollisionShape CollisionShape;
        public PhysicalMaterial PhysicalMaterial;
        public Texture2D Thumbnail;
        public Assembly[] Scripts;
        public Dictionary<string, string> Properties;

        public bool Loaded { get; private set; }
        public Result Load()
        {
            if (Texture2Ds != null)
            {
                foreach (var t in Texture2Ds)
                {
                    if (!t.Loaded) t.Load();
                }
            }

            if (TextureCubes != null)
            {
                foreach (var t in TextureCubes)
                {
                    if (!t.Loaded) t.Load();
                }
            }

            if (Mesh != null)
            {
                Mesh.Load();
            }

            if (Thumbnail != null) Thumbnail.Load();

            if (Sounds != null)
            {
                foreach (var s in Sounds) s.Load();
            }

            Loaded = true;
            return Result.Success;
        }
        public Result Unload()
        {
            if (Texture2Ds != null)
            {
                foreach (var t in Texture2Ds) t.Unload();
            }

            if (TextureCubes != null)
            {
                foreach (var t in TextureCubes) t.Unload();
            }

            if (Mesh != null)
            {
                Mesh.Unload();
            }

            if (Sounds != null)
            {
                foreach (var s in Sounds) s.Unload();
            }

            Loaded = false;
            return Result.Success;
        }
    }
}
