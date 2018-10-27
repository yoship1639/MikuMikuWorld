using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqoModelImporter
{
    /// <summary>
    /// テキストからデータを読み取るクラス
    /// </summary>
    public class Scanner
    {
        /// <summary>
        /// スキャンするテキスト
        /// </summary>
        public string Text { get { return text; } }

        /// <summary>
        /// スキャン位置
        /// </summary>
        public int Seek
        {
            get { return seek; }
            set { seek = value; if (seek < 0) seek = 0; }
        }
        private int seek = 0;

        /// <summary>
        /// 前回のスキャン位置
        /// </summary>
        public int PrevSeek { get { return prevSeek; } }
        private int prevSeek = 0;

        /// <summary>
        /// スキャンした文字列
        /// </summary>
        public string ScanString { get { return scanStr; } }
        private string scanStr = null;

        /// <summary>
        /// スキャン位置が既にテキストの終わりか
        /// </summary>
        public bool IsEnd { get { return Seek >= text.Length; } }

        private string text;

        public Scanner(string text)
        {
            this.text = text;
        }

        /// <summary>
        /// 次の文字列を読み取る
        /// </summary>
        /// <returns>文字列</returns>
        public string NextString()
        {
            prevSeek = seek;
            // WhiteSpaceを飛ばす
            while (true)
            {
                if (IsEnd)
                {
                    scanStr = null;
                    return null;
                }
                if (char.IsWhiteSpace(text[seek])) seek++;
                else break;
            }

            // テキストの読み取り
            int end = seek;
            while (true)
            {
                if (end == text.Length || char.IsWhiteSpace(text[end]))
                {
                    //if (end == text.Length) end--;
                    string str = text.Substring(seek, end - seek);
                    scanStr = str;
                    seek = end;
                    return str;
                }
                else end++;
            }
        }

        /// <summary>
        /// 次の文字列を読み取る。
        /// 指定の終端文字列までWhiteSpaceに関係なく読み込む
        /// 終端文字列が見つからなかったらSeekは動かずnullを返す
        /// </summary>
        /// <param name="endStr">終端文字列</param>
        /// <returns>文字列</returns>
        public string NextString(string endStr)
        {
            prevSeek = seek;
            // WhiteSpaceを飛ばす
            while (true)
            {
                if (IsEnd)
                {
                    scanStr = null;
                    return null;
                }
                if (char.IsWhiteSpace(text[seek])) seek++;
                else break;
            }

            // 終端文字列のある位置を検索
            int index = text.IndexOf(endStr, seek);
            if (index == -1) return null;
            int end = index + endStr.Length;
            string str = text.Substring(seek, end - seek);
            scanStr = str;
            seek = end;
            return str;
        }

        /// <summary>
        /// 次の整数値を読み取る
        /// </summary>
        /// <returns>次の整数値</returns>
        public int NextInt()
        {
            string str = NextString();
            return int.Parse(str);
        }

        /// <summary>
        /// 次の符号なし整数値を読み取る
        /// </summary>
        /// <returns>次の符号なし整数値</returns>
        public uint NextUInt()
        {
            string str = NextString();
            return uint.Parse(str);
        }

        /// <summary>
        /// 次の浮動小数値を読み取る
        /// </summary>
        /// <returns>次の浮動少数値</returns>
        public float NextFloat()
        {
            return float.Parse(NextString().Replace(',', '.'), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 次の倍精度小数値を読み取る
        /// </summary>
        /// <returns>次の倍精度小数値</returns>
        public double NextDouble()
        {
            return double.Parse(NextString().Replace(',', '.'), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 10進数値を読み取る
        /// </summary>
        /// <returns>10進数値</returns>
        public decimal NextDecimal()
        {
            return decimal.Parse(NextString().Replace(',', '.'), CultureInfo.InvariantCulture);
        }
    }
}
