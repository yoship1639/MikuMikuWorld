using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MikuMikuWorld
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static int Main()
        {
            using (GameWindow window = new Game())
            {
                window.Run(60.0);
            }
            return 0;
        }
    }
}
