﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Walker
{
    public class WorldInfo
    {
        public string HostName;
        public int Port;

        public int GameType = -1;
        public float Version;
        public string WorldName;
        public string WorldDesc;
        public bool WorldPass;
        public string WorldIcon;
        public string WorldImage;
        public int MaxPlayer;
        public int NowPlayer;
        public int Culture;

        public string PublicKey;
    }
}
