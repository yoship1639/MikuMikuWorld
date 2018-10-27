using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Networks
{
    interface ICmd
    {
        int[] ExecutableDataTypes { get; }
        bool Execute(Server server, int dataType, byte[] data, bool isTcp);
    }
}
