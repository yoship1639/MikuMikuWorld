using MikuMikuWorld.Importers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    public static class MwsExporter
    {
        public static Result Export(string filepath, ImportedObject obj)
        {
            using (var ms = new MemoryStream())
            {
                using (var sr = new StreamReader(ms))
                {
                    var serializer = new DataContractJsonSerializer(typeof(ImportedObject));
                    serializer.WriteObject(ms, obj);
                    ms.Position = 0;

                    var json = sr.ReadToEnd();

                    Console.WriteLine(json);
                }
            }
            
            return Result.Failed;
        }
    }
}
