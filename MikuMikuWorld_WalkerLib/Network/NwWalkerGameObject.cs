using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Walker.Network
{
    public class NwWalkerGameObject
    {
        public int SessionID;
        public string UserID;
        public string CreatorName;
        public string Name { get; set; } = "game object";
        public string Hash;
        public string CreateDate;

        public Vector3f Position = new Vector3f(0, 0, 0);
        public Vector3f Rotation = new Vector3f(0, 0, 0);
        public Vector3f Scale = new Vector3f(1, 1, 1);

        public string ObjectHash;
        public NwScriptInfo[] Scripts;

        public class NwScriptInfo
        {
            public string Hash;
            public byte[] Status;
        }
    }

    
}
