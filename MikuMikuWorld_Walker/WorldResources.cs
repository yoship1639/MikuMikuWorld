using MikuMikuWorld.Assets;
using MikuMikuWorld.Network;
using MikuMikuWorld.Walker.Network;
using MikuMikuWorldScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    class WorldResources : Assets.IAsset
    {
        public bool Loaded => true;
        public string Name => "Resources";
        public Result Load() => Result.Success; 
        public Result Unload() => Result.Success;

        public Dictionary<string, World> Worlds = new Dictionary<string, World>();
        public Dictionary<string, Character> Characters = new Dictionary<string, Character>();
        public Dictionary<string, WorldObject> Objects = new Dictionary<string, WorldObject>();
        public Dictionary<string, Assembly> GameObjectScripts = new Dictionary<string, Assembly>();

        public WorldResources() { }
        public WorldResources(NwWorldData data)
        {
            foreach (var w in data.Worlds)
            {
                var world = AssetConverter.FromNwWorld(w);
                Worlds.Add(w.Hash, world);
            }
            foreach (var ch in data.Characters)
            {
                var c = AssetConverter.FromNwCharacter(ch);
                Characters.Add(ch.Hash, c);
            }
            foreach (var obj in data.Objects)
            {
                var o = AssetConverter.FromNwObject(obj);
                Objects.Add(obj.Hash, o);
            }
            foreach (var o in data.Objects)
            {
                if (o.Scripts == null) continue;
                foreach (var s in o.Scripts)
                {
                    try
                    {
                        var asm = Assembly.Load(s.Assembly);
                        GameObjectScripts.Add(s.Hash, asm);
                    }
                    catch { }
                }
            }
            foreach (var scr in data.GameObjectScripts)
            {
                try
                {
                    var asm = Assembly.Load(scr.Assembly);
                    GameObjectScripts.Add(scr.Hash, asm);
                }
                catch { }
            }
        }

        public GameObjectScript CreateScript(string hash)
        {
            var asm = GameObjectScripts[hash];
            return CreateScript(asm);
        }
        public string GetHash(Assembly asm)
        {
            foreach (var o in GameObjectScripts)
            {
                if (o.Value.GetType() == asm.GetType()) return o.Key;
            }

            return null;
        }
        public static GameObjectScript CreateScript(Assembly asm)
        {
            var type = asm.GetExportedTypes().Where(t => t.IsSubclassOf(typeof(GameObjectScript))).ToArray()[0];
            return asm.CreateInstance(type.FullName) as GameObjectScript;
        }


    }
}
