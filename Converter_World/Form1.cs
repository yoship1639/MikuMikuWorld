using Converter_World.Properties;
using MikuMikuWorld;
using MikuMikuWorld.Assets;
using MikuMikuWorld.Importers;
using MikuMikuWorld.Network;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Converter_World
{
    public partial class Form1 : Form
    {
        List<IImporter> importers = new List<IImporter>();
        ImportedObject obj;

        public Form1()
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
            ofd.Filter = "対応フォーマット|*.mwc;*.mws;*.mwo;*.mwce;*.mwse;*.mwoe;*.pmd;*.pmx;*.mqo|すべてのファイル|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox_inputPath.Text = ofd.FileName;
            }
        }

        private void button_output_Click(object sender, EventArgs e)
        {

            var filter = "MMWキャラクタ|*.mwc|MMWワールド|*.mww|MMWオブジェクト|*.mwo";
            var filtere = "Encrypted MMWキャラクタ|*.mwce|Encrypted MMWステージ|*.mwse|Encrypted MMWオブジェクト|*.mwoe";

            var sfd = new SaveFileDialog();
            sfd.FileName = Path.GetFileNameWithoutExtension(textBox_inputPath.Text);
            sfd.Filter = checkBox_encrypt.Checked ? filtere : filter;
            sfd.AddExtension = true;

            if (sfd.ShowDialog() != DialogResult.OK) return;

            textBox_desc.AppendText("MMWフォーマットに変換中...\r\n");

            byte[] data = null;
            if (sfd.FilterIndex == 1) data = ConvertCharacter(obj);
            else if (sfd.FilterIndex == 2) data = ConvertWorld(obj);
            else if (sfd.FilterIndex == 3) data = ConvertObject(obj);

            if (data == null)
            {
                textBox_desc.AppendText("変換中にエラーが発生しました.出力を中止します.\r\n\r\n");
                return;
            }

            File.WriteAllBytes(sfd.FileName, data);

            textBox_desc.AppendText(string.Format("{0} に出力完了!\r\n\r\n", sfd.FileName));
        }

        private byte[] ConvertCharacter(ImportedObject obj)
        {
            var ch = new NwCharacter();

            {
                ch.Name = obj.Name;

                var texs = new List<NwTexture2D>();
                foreach (var t in obj.Textures)
                {
                    var bm = AssetConverter.ToNwTexture2D(t.SrcBitmap, t.Name);
                    texs.Add(bm);
                }

                var cubes = new List<NwCubemap>();

                var mats = new List<NwMaterial>();
                foreach (var m in obj.Materials)
                {
                    var mat = AssetConverter.ToNwMaterial(m, m.Name, ref texs, ref cubes);
                    mats.Add(mat);
                }
                ch.Texture2Ds = texs.ToArray();
                ch.Cubemaps = cubes.ToArray();
                ch.Materials = mats.ToArray();

                var bones = new List<NwBone>();
                foreach (var b in obj.Bones)
                {
                    var bone = AssetConverter.ToNwBone(b);
                    bones.Add(bone);
                }
                ch.Bones = bones.ToArray();

                ch.Mesh = AssetConverter.ToNwMesh(obj.Meshes[0]);

                //ch.Height = 1.6f;
                //ch.WidthRadius = 0.3f;
                ch.EyePosition = new Vector3f(0.0f, 1.42f, 0.2f);
                ch.Mass = 50.0f;
            }

            var json = Util.SerializeJson(ch);
            var comp = Util.Compress(Encoding.UTF8.GetBytes(json));

            return comp;
        }

        private byte[] ConvertWorld(ImportedObject obj)
        {
            var world = new NwWorld();

            {
                var texs = new List<NwTexture2D>();
                foreach (var t in obj.Textures)
                {
                    var bm = AssetConverter.ToNwTexture2D(t.SrcBitmap, t.Name);
                    texs.Add(bm);
                }

                var cubes = new List<NwCubemap>();
                cubes.Add(AssetConverter.ToNwTextureCube(new Bitmap[]
                {
                    Resources.posx2,
                    Resources.posy2,
                    Resources.posz2,
                    Resources.negx2,
                    Resources.negy2,
                    Resources.negz2,
                }, null));

                var mats = new List<NwMaterial>();
                foreach (var m in obj.Materials)
                {
                    var mat = AssetConverter.ToNwMaterial(m, m.Name, ref texs, ref cubes);
                    mats.Add(mat);
                }
                world.Texture2Ds = texs.ToArray();
                world.Cubemaps = cubes.ToArray();
                world.Materials = mats.ToArray();

                var meshes = new List<NwMesh>();
                foreach (var mesh in obj.Meshes) meshes.Add(AssetConverter.ToNwMesh(mesh));
                world.Meshes = meshes.ToArray();

                var colMeshes = new List<NwColliderMesh>();
                foreach (var mesh in obj.Meshes)
                {
                    colMeshes.Add(AssetConverter.ToNwColliderMesh(mesh));
                }
                world.ColliderMeshes = colMeshes.ToArray();

                world.Environments = new NwEnvironment[1];
                world.Environments[0] = new NwEnvironment()
                {
                    EnvMap = world.Cubemaps[0].Hash,
                };
            }
            var json = Util.SerializeJson(world);
            var comp = Util.Compress(Encoding.UTF8.GetBytes(json));

            return comp;
        }

        private byte[] ConvertObject(ImportedObject obj)
        {
            var o = new NwObject();

            {
                o.Name = obj.Name;

                var texs = new List<NwTexture2D>();
                foreach (var t in obj.Textures)
                {
                    var bm = AssetConverter.ToNwTexture2D(t.SrcBitmap, t.Name);
                    texs.Add(bm);
                }

                var cubes = new List<NwCubemap>();

                var mats = new List<NwMaterial>();
                foreach (var m in obj.Materials)
                {
                    var mat = AssetConverter.ToNwMaterial(m, m.Name, ref texs, ref cubes);
                    mats.Add(mat);
                }
                o.Texture2Ds = texs.ToArray();
                o.Cubemaps = cubes.ToArray();
                o.Materials = mats.ToArray();
                o.Mesh = AssetConverter.ToNwMesh(obj.Meshes[0]);

                //o.Height = 1.6f;
                //o.WidthRadius = 0.3f;

                o.PhysicalMaterial = new NwPhysicalMaterial();
            }
            var json = Util.SerializeJson(o);
            var comp = Util.Compress(Encoding.UTF8.GetBytes(json));

            return comp;
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
