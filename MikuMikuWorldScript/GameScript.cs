using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorldScript
{
    public abstract class GameScript
    {
        public static readonly string Version = "1.0";

        /// <summary>
        /// ゲーム名
        /// </summary>
        public abstract string GameName { get; }
        /// <summary>
        /// ゲームの説明
        /// </summary>
        public abstract string GameDesc { get; }
        /// <summary>
        /// ゲームのルール
        /// </summary>
        public abstract string GameRule { get; }

        //public abstract 
    }
}
