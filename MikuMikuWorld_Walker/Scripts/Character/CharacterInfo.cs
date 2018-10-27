using MikuMikuWorld.Assets;
using MikuMikuWorld.Walker;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Scripts.Character
{
    class CharacterInfo : GameComponent
    {
        public Assets.Character Character;
        public WalkerPlayer Player;

        public CharacterInfo() { }
        public CharacterInfo(Assets.Character ch, WalkerPlayer pl)
        {
            Character = ch;
            Player = pl;
        }
    }
}
