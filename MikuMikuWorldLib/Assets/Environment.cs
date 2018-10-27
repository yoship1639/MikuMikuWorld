using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Assets
{
    public class Environment : IAsset
    {
        public Color4 Ambient = new Color4(0.2f, 0.2f, 0.2f, 0.0f);
        public bool CastShadow = true;

        public float DirLightIntensity = 4.0f;
        public Vector3 DirLightDir = new Vector3(1.0f, -1.0f, 1.0f).Normalized();
        public Color4 DirLightColor = new Color4(1.0f, 0.92f, 0.84f, 1.0f);

        public ColorCollect ColorCollect = new ColorCollect();
        public TextureCube EnvMap;

        public string Name { get; set; }
        public bool Loaded => true;
        public Result Load() => Result.Success;
        public Result Unload() => Result.Success;
    }
}
