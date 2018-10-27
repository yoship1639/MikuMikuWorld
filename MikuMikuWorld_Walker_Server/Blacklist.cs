using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld_Walker_Server
{
    [DataContract]
    public class Blacklist
    {
        [DataMember(Order = 0)]
        public List<string> IgnoreIPs = new List<string>();

        [DataMember(Order = 1)]
        public List<uint> IgnoreIDs = new List<uint>();

        public bool IsIgnoreIP(string ip)
        {
            return IgnoreIPs.Contains(ip);
        }

        public bool IsIgnoreID(uint id)
        {
            return IgnoreIDs.Contains(id);
        }
    }
}
