using MikuMikuWorld.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Importers
{
    public interface IImporter
    {
        bool DirectoryImporter { get; }
        string[] Extensions { get; }

        ImportedObject[] Import(string path, ImportType type);
    }

    public enum ImportType
    {
        OverviewOnly,
        Full,
    }
}
