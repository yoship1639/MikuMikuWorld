using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Test_BinaryFormatter
{
    class Program
    {
        static void Main(string[] args)
        {
            var bf = new BinaryFormatter();

            bf.Serialize()
        }
    }

    [Serializable]
    class Test
    {
        
    }
}
