using MqoModelImporter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MqoThumbnailExtract
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            //コントロール内にドロップされたとき実行される
            //ドロップされたすべてのファイル名を取得する
            string filename = ((string[])e.Data.GetData(DataFormats.FileDrop, false))[0];

            // テキストをすべて読み取る
            string text = File.ReadAllText(filename, Encoding.GetEncoding(932)); // 932:Shift-JIS

            // スキャナの作成
            Scanner scanner = new Scanner(text);
            while (scanner.NextString() != "Thumbnail" && !scanner.IsEnd) ;

            var sx = scanner.NextInt();
            var sy = scanner.NextInt();
            var size = scanner.NextInt();
            var format = scanner.NextString();
            var format2 = scanner.NextString();
            scanner.NextString();

            StringBuilder str = new StringBuilder();
            for (var i = 0; i < (sx * sy) / 32; i++)
            {
                str.Append(scanner.NextString());
            }
            var data = str.ToString();

            var buf = new byte[sx * sy * 4];
            for (var y = 0; y < sy; y++)
            {
                for (var x = 0; x < sx; x++)
                {
                    var sr = data.Substring((y * sx + x) * 6, 2);
                    var sg = data.Substring((y * sx + x) * 6 + 2, 2);
                    var sb = data.Substring((y * sx + x) * 6 + 4, 2);
                    bool alpha = sr == "1E" && sg == "41" && sb == "5A";
                    var r = Convert.ToInt64(sr, 16);
                    var g = Convert.ToInt64(sg, 16);
                    var b = Convert.ToInt64(sb, 16);
                    buf[(y * sx + x) * 4 + 0] = (byte)r;
                    buf[(y * sx + x) * 4 + 1] = (byte)g;
                    buf[(y * sx + x) * 4 + 2] = (byte)b;
                    buf[(y * sx + x) * 4 + 3] = alpha ? (byte)0 : (byte)255;
                }
            }

            var bitmap = new Bitmap(sx, sy);
            var bd = bitmap.LockBits(new Rectangle(0, 0, sx, sy), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Marshal.Copy(buf, 0, bd.Scan0, sx * sy * 4);
            bitmap.UnlockBits(bd);

            var path = Path.GetDirectoryName(filename) + "/" + Path.GetFileNameWithoutExtension(filename) + ".png";
            bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Png);

            return;
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            //コントロール内にドラッグされたとき実行される
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                //ドラッグされたデータ形式を調べ、ファイルのときはコピーとする
                e.Effect = DragDropEffects.Copy;
            else
                //ファイル以外は受け付けない
                e.Effect = DragDropEffects.None;
        }
    }
}
