using MikuMikuWorld.Walker;
using MikuMikuWorldScript;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    class WorldData : Assets.IAsset, IWorldData
    {
        public string Name => "world data";
        public bool Loaded => true;
        public Result Load() => Result.Success;
        public Result Unload() => Result.Success;

        public string WorldName { get; set; }
        public string WorldDesc { get; set; }
        public bool WorldPass { get; set; }
        public CultureInfo Culture { get; set; }
        public int MaxPlayerNum { get; set; }

        public bool AllowUserSound { get; set; }
        public bool AllowUserStamp { get; set; }
        public bool AllowUserEmote { get; set; }
        public bool AllowUserModel { get; set; }
        public bool AllowUserObject { get; set; }

        public List<WalkerPlayer> Players = new List<WalkerPlayer>();
    }
}
