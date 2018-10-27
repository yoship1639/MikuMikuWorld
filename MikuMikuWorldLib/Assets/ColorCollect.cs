using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Assets
{
    public class ColorCollect : IAsset
    {
        public string Name { get; set; }
        public bool Loaded => true;
        public Result Load() => Result.Success;
        public Result Unload() => Result.Success;

        public float Contrast = 1.0f;
        public float Saturation = 1.0f;
        public float Hue = 0.0f;
        public float Brightness = 1.0f;
        public float Gamma = 2.2f;
    }
}
