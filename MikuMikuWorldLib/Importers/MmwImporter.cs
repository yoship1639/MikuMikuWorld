using MikuMikuWorld.Assets;
using MikuMikuWorld.Properties;
using OpenTK.Graphics;
using PmxModelImporter;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MikuMikuWorld.Importers
{
    public class MmwImporter : IImporter
    {
        public bool DirectoryImporter { get { return false; } }

        public string[] Extensions
        {
            get
            {
                return new string[]
                {
                    //".mwc",
                    //".mww",
                    //".mwo",
                };
            }
        }

        public ImportedObject[] Import(string path, ImportType type)
        {
            string json = null;

            try
            {
                json = File.ReadAllText(path);
            }
            catch
            {
                return new ImportedObject[]
                {
                    new ImportedObject()
                    {
                        Result = Result.InvalidPath,
                        Path = path,
                    }
                };
            }
            /*
            var ext = Path.GetExtension(path);
            if (ext == ".mwce" || ext == ".mwse" || ext == ".mwoe")
            {
                var setting = Resources.SettingsPath;
                init(setting, 0, 0, -1);

                var key = new string('\0', 32);
                getpass(setting, key);

                try
                {
                    json = Encrypter.Decrypt(json, key);
                }
                catch
                {
                    return new ImportedObject[]
                    {
                        new ImportedObject()
                        {
                            Result = Result.InvalidData,
                            Path = path,
                        }
                    };
                }
            }*/

            ImportedObject obj = null;
            if (type == ImportType.Full)
            {
                
                obj = Util.DeserializeJson<ImportedObject>(json);

                if (obj.Materials != null)
                {
                    foreach (var m in obj.Materials)
                    {
                        foreach (var p in m.tex2DParams.Values)
                        {
                            if (p.tag == null) continue;
                            if ((int)p.tag == -1) continue;
                            p.value = obj.Textures[(int)p.tag];
                        }
                    }
                }
                
                obj.Result = Result.Success;
                obj.Path = path;
            }
            else if (type == ImportType.OverviewOnly)
            {
                var o = Util.DeserializeJson<ImportedOverviewObject>(json);
                obj = new ImportedObject()
                {
                    Author = o.Author,
                    AuthorURL = o.AuthorURL,
                    Description = o.Description,
                    Editor = o.Editor,
                    EditorURL = o.EditorURL,
                    ExportVersion = o.ExportVersion,
                    MagicNumber = o.MagicNumber,
                    Name = o.Name,
                    Path = path,
                    Result = Result.Success,
                    Type = o.Type,
                    Version = o.Version,
                };
            }

            return new ImportedObject[] { obj };
        }

        [DllImport("MMWModule.dll", CharSet = CharSet.Unicode)]
        static extern void init(string confpath, int datatype, int closed, int check);

        [DllImport("MMWModule.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        static extern void getpass(string confpath, string pass);

        [DllImport("MMWModule.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        static extern string encrypt(string text);

        [DllImport("MMWModule.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        static extern string decrypt(string text);
    }
}
