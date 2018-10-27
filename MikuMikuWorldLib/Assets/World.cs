using MikuMikuWorld.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Assets
{
    public class World : IAsset
    {
        public string Name { get; set; }

        public NwWorld nwWorld;

        public WorldObject[] Objects;
        public Material[] Materials;
        public Mesh[] Meshes;
        public ColliderMesh[] ColliderMeshes;
        public Environment[] Environments;
        public Texture2D[] Texture2Ds;
        public TextureCube[] TextureCubes;

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

            if (Meshes != null)
            {
                foreach (var m in Meshes)
                {
                    if (!m.Loaded) m.Load();
                }
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

            if (Meshes != null)
            {
                foreach (var m in Meshes)
                {
                    m.Unload();
                }
            }

            Loaded = false;
            return Result.Success;
        }
    }
}
