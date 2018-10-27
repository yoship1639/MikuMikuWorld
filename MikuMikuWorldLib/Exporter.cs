using MikuMikuWorld.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    public static class Exporter
    {
        [DllImport("MMWModule.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        static extern void init(string confpath, int datatype, int closed, int check);

        [DllImport("MMWModule.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        static extern void getpass(string confpath, string pass);

        [DllImport("MMWModule.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        static extern string encrypt(string text);

        [DllImport("MMWModule.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        static extern string decrypt(string text);

        public static Result Export(string filepath, ImportedObject obj, bool encrypt)
        {
            if (string.IsNullOrWhiteSpace(filepath)) return Result.InvalidPath;

            string json = null;
            try
            {
                json = Util.SerializeJson(obj);
            }
            catch
            {
                return Result.InvalidData;
            }

            if (encrypt)
            {
                var setting = Resources.SettingsPath;
                init(setting, 0, 0, -1);

                var pass = new string('\0', 32);
                getpass(setting, pass);

                json = Encrypter.Encrypt(json, pass);
            }

            try
            {
                File.WriteAllText(filepath, json);
            }
            catch
            {
                return Result.InvalidPath;
            }

            return Result.Success;
        }
    }
}
