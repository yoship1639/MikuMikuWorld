using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Network
{
    public class NwWorldDataDesc
    {
        public NwDataInfo[] Worlds = new NwDataInfo[0];
        public NwDataInfo[] Characters = new NwDataInfo[0];
        public NwDataInfo[] Objects = new NwDataInfo[0];
        public NwDataInfo[] GameObjectScripts = new NwDataInfo[0];
        public NwDataInfo[] Scripts = new NwDataInfo[0];
        public NwDataInfo[] Emotes = new NwDataInfo[0];
        public NwDataInfo[] Stamps = new NwDataInfo[0];
        public NwDataInfo[] Sounds = new NwDataInfo[0];

        public NwDataInfo[] UserCharacters = new NwDataInfo[0];
        public NwDataInfo[] UserObjects = new NwDataInfo[0];
        public NwDataInfo[] UserEmotes = new NwDataInfo[0];
        public NwDataInfo[] UserStamps = new NwDataInfo[0];
        public NwDataInfo[] UserSounds = new NwDataInfo[0];
    }
}
