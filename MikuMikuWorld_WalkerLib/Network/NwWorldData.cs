using MikuMikuWorld.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Walker.Network
{
    public class NwWorldData
    {
        public NwWorld[] Worlds;
        public NwCharacter[] Characters;
        public NwObject[] Objects;
        public NwGameObjectScript[] GameObjectScripts;
        public NwDataInfo[] Scripts;
        public NwDataInfo[] Emotes;
        public NwDataInfo[] Stamps;
        public NwDataInfo[] Sounds;

        public NwDataInfo[] UserCharacters;
        public NwDataInfo[] UserObjects;
        public NwDataInfo[] UserEmotes;
        public NwDataInfo[] UserStamps;
        public NwDataInfo[] UserSounds;

        public NwWalkerGameObject[] GameObjects;
    }
}
