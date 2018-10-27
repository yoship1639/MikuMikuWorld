using MikuMikuWorld;
using MikuMikuWorld.Importers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MMWSimpleExporter
{
    public partial class MMWSimpleExporter : Form
    {
        List<IImporter> importers = new List<IImporter>();
        ImportedObject obj;

        public MMWSimpleExporter()
        {
            InitializeComponent();

            importers.Add(new MmwImporter());
            importers.Add(new PmxImporter());
            importers.Add(new PmdImporter());
            importers.Add(new MqoImporter());
        }

        private void button_ofd_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "MMWフォーマット|*.mwc;*.mws;*.mwo;*.mwce;*.mwse;*.mwoe;|対応フォーマット|*.pmd;*.pmx;*.mqo|すべてのファイル|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox_inputPath.Text = ofd.FileName;
            }
        }

        private void button_output_Click(object sender, EventArgs e)
        {

            var filter = "MMWキャラクタ|*.mwc|MMWステージ|*.mws|MMWオブジェクト|*.mwo";
            var filtere = "Encrypted MMWキャラクタ|*.mwce|Encrypted MMWステージ|*.mwse|Encrypted MMWオブジェクト|*.mwoe";

            var sfd = new SaveFileDialog();
            sfd.FileName = Path.GetFileNameWithoutExtension(textBox_inputPath.Text);
            sfd.Filter = checkBox_encrypt.Checked ? filtere : filter;
            sfd.AddExtension = true;

            if (sfd.ShowDialog() != DialogResult.OK) return;

            textBox_desc.AppendText("MMWフォーマットに変換中...\r\n");
            var res = Exporter.Export(sfd.FileName, obj, checkBox_encrypt.Checked);
            if (res != Result.Success)
            {
                textBox_desc.AppendText("変換中にエラーが発生しました.出力を中止します.\r\n\r\n");
                return;
            }
            textBox_desc.AppendText(string.Format("{0} に出力完了!\r\n\r\n", sfd.FileName));
        }

        private void textBox_inputPath_TextChanged(object sender, EventArgs e)
        {
            button_output.Enabled = false;
            Cursor = Cursors.WaitCursor;

            var ext = Path.GetExtension(textBox_inputPath.Text);
            var importer = importers.Find((i) => Array.Exists(i.Extensions, (ex) => ex == ext));
            if (importer == null)
            {
                textBox_desc.AppendText(string.Format("{0} を読み込める適切なインポータが存在しません\r\n\r\n", ext));
                Cursor = Cursors.Default;
                return;
            }

            textBox_desc.AppendText(string.Format("{0}を読み込み中...\r\n", textBox_inputPath.Text));

            try
            {
                obj = importer.Import(textBox_inputPath.Text, ImportType.Full)[0];
            }
            catch
            {
                textBox_desc.AppendText("読み込み中にエラーが発生しました.出力を中止します.\r\n\r\n");
                Cursor = Cursors.Default;
                return;
            }

            textBox_name.Text = obj.Name;
            textBox_ver.Text = obj.Version;
            textBox_author.Text = obj.Author;
            textBox_editor.Text = obj.Editor;
            textBox_description.Text = obj.Description;

            button_output.Enabled = true;
            Cursor = Cursors.Default;

            textBox_desc.AppendText(string.Format("準備完了.\r\n"));
        }

        private void textBox_name_TextChanged(object sender, EventArgs e)
        {
            if (obj != null) obj.Name = textBox_name.Text;
        }

        private void textBox_author_TextChanged(object sender, EventArgs e)
        {
            if (obj != null) obj.Author = textBox_author.Text;
        }

        private void textBox_editor_TextChanged(object sender, EventArgs e)
        {
            if (obj != null) obj.Editor = textBox_editor.Text;
        }

        private void textBox_description_TextChanged(object sender, EventArgs e)
        {
            if (obj != null) obj.Description = textBox_description.Text;
        }

        private void textBox_ver_TextChanged(object sender, EventArgs e)
        {
            if (obj != null) obj.Version = textBox_ver.Text;
        }
    }
}
