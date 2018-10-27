using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorldScript
{
    public interface IParam
    {
        T Get<T>(string name);
        bool Has(string name);
        bool Set(string name, object value);
    }
}
