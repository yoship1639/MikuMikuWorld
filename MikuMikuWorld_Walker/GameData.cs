using MikuMikuWorld.Assets;
using MikuMikuWorldScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    class GameData : Assets.IAsset
    {
        public bool Loaded { get { return true; } }
        public string Name { get; set; }
        public Result Load() { return Result.Success; }
        public Result Unload() { return Result.Success; }

        public static readonly string VersionStr = "v1.0";
        public static readonly string AppName = "walker";
        public static readonly string RootDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + @"\MikuMikuWorld\";
        public static readonly string AppDir = RootDir + AppName + "\\" + VersionStr + "\\";

        public float Version = 1.0f;
        public string SkinShader = "Deferred Physical Skin";
        public string Shader = "Deferred Physical";

        public ImportedObject Player;
        public ImportedObject Stage;
    }
}
