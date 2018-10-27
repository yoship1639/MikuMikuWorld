using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorldScript
{
    public interface IGameComponent : IParam, IMethod
    {
        string Name { get; }
        bool Enabled { get; set; }

        IGameObject GameObject { get; }
    }
}
