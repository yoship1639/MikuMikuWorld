using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld_Walker_Server
{
    public static class Log
    {
        internal static MainForm form;

        public static void LogInfo(string mes, bool detail)
        {
            form.LogInfo(mes, detail);
        }
        public static void LogWarn(string mes, bool detail)
        {
            LogWarn(mes, detail);
        }
        public static void LogError(string mes, bool detail)
        {
            LogError(mes, detail);
        }
    }
}
