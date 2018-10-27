using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld_Walker_Server
{
    public class Settings
    {
        public string WorldName = "World";
        public string WorldDesc = "";
        public bool UseWorldPass = false;
        public string WorldPass;
        public string WorldImagePath;
        public string WorldIconPath;

        public int TcpPort = 39393;
        public int UdpPort = 39394;
        public int MaxPlayer = 20;
        public int UdpInterval = 400;
        public int Culture = -1;

        public bool UseScript = false;
        public bool UserModel = false;

        public string PublicKey;
        public string PrivateKey;
    }
}
