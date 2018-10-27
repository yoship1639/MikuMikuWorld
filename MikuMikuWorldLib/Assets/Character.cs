using MikuMikuWorld.Network;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Assets
{
    public class Character : IAsset
    {
        public string Name { get; set; }

        public NwCharacter nwCharacter;

        public Material[] Materials;
        public Mesh Mesh;
        public Bone[] Bones;
        public Motion[] Motions;
        public Morph[] Morphs;
        public Texture2D[] Texture2Ds;
        public TextureCube[] TextureCubes;
        public Sound[] Sounds;
        public Assembly[] Scripts;
        public Dictionary<string, string> Properties;

        public Vector3 EyePosition;
        public CollisionShape CollisionShape;
        public PhysicalMaterial PhysicalMaterial;

        public float Height
        {
            get
            {
                if (CollisionShape == null) return 0.0f;
                if (CollisionShape is CollisionCapsule)
                {
                    var shape = (CollisionCapsule)CollisionShape;
                    return shape.Height;
                }
                else if (CollisionShape is CollisionCylinder)
                {
                    var shape = (CollisionCylinder)CollisionShape;
                    return shape.Height;
                }
                else if (CollisionShape is CollisionBox)
                {
                    var shape = (CollisionBox)CollisionShape;
                    return shape.HalfExtents.Y * 2.0f;
                }
                else if (CollisionShape is CollisionSphere)
                {
                    var shape = (CollisionSphere)CollisionShape;
                    return shape.Radius * 2.0f;
                }

                return 0.0f;
            }
        }

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
                foreach (var t in Texture2Ds)
                {
                    t.Unload();
                }
            }

            if (TextureCubes != null)
            {
                foreach (var t in TextureCubes)
                {
                    t.Unload();
                }
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
