using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorldScript
{
    public interface IServer
    {
        void SendTcp(int dataType, byte[] data);
    }
}
