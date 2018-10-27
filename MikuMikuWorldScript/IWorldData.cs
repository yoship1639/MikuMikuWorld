using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorldScript
{
    public interface IWorldData
    {
        /// <summary>
        /// ワールド名
        /// </summary>
        string WorldName { get; }

        /// <summary>
        /// ワールドの説明
        /// </summary>
        string WorldDesc { get; }

        /// <summary>
        /// ワールドの参加にパスワードが必要か
        /// </summary>
        bool WorldPass { get; }

        /// <summary>
        /// 地域
        /// </summary>
        CultureInfo Culture { get; }

        /// <summary>
        /// プレイヤ最大数
        /// </summary>
        int MaxPlayerNum { get; }


        /// <summary>
        /// ユーザのサウンドを許可するか
        /// </summary>
        bool AllowUserSound { get; }

        /// <summary>
        /// ユーザのスタンプを許可するか
        /// </summary>
        bool AllowUserStamp { get; }

        /// <summary>
        /// ユーザのエモートを許可するか
        /// </summary>
        bool AllowUserEmote { get; }

        /// <summary>
        /// ユーザのモデルを許可するか
        /// </summary>
        bool AllowUserModel { get; }

        /// <summary>
        /// ユーザのオブジェクトを許可するか
        /// </summary>
        bool AllowUserObject { get; }
    }
}
