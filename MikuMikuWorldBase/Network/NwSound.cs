using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Network
{
    public class NwSound
    {
        public string Hash;
        public string Name { get; set; }
        public string Format; // WAV MP3 OGG
        public byte[] Data;
    }
}
