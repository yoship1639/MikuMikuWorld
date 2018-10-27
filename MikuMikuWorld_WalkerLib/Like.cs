using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Walker
{
    public class Like
    {
        public User From;
        public User To;
        public DateTime Timestamp;
    }

    public class User
    {
        public string Name;
        public string UserID;
    }
}
