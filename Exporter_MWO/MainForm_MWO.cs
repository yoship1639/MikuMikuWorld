﻿using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MikuMikuWorld;
using System.Threading;
using OpenTK.Graphics;
using MikuMikuWorld.Importers;
using System.IO;
using MikuMikuWorld.Network;
using Exporter_MMW.Properties;
using MikuMikuWorld.Assets;
using MikuMikuWorldScript;
using System.Reflection;
using NAudio.Wave;

namespace Exporter_MMW
{
    public partial class MainForm_MWO : Form
    {
        private Game game;
        private List<IImporter> importers = new List<IImporter>()
        {
            new PmxImporter(),
            new PmdImporter(),
            new MqoImporter(),
            new VmdImporter(),
        };
        private NwObject obj;
        private List<NwTexture2D> textures = new List<NwTexture2D>();

        class ScriptInfo
        {
            public byte[] Assembly;
            public GameObjectScript Script;
            public string Name => Script.ScriptName;
        }

        private string filepath;
        private string Filepath
        {
            get { return filepath; }
            set
            {
                filepath = value;
                if (filepath != null) Text = "MWO Editor - " + filepath;
                else Text = "MWO Editor";
            }
        }
        private Control collisionShapeCtrl;
        private Task task;

        public MainForm_MWO()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            ShowPreviewWindow();

            menu_exit.Click += (s, e) =>
            {
                var res = MessageBox.Show("終了しますか?", "確認", MessageBoxButtons.YesNo);
                if (res == DialogResult.Yes) Close();
            };

            // general
            textBox_name.TextChanged += (s, e) =>
            {
                if (obj == null) return;
                obj.Name = textBox_name.Text;
            };
            textBox_editor.TextChanged += (s, e) =>
            {
                if (obj == null) return;
                obj.Editor = textBox_editor.Text;
            };
            textBox_url.TextChanged += (s, e) =>
            {
                if (obj == null) return;
                obj.EditorURL = textBox_url.Text;
            };
            textBox_version.TextChanged += (s, e) =>
            {
                if (obj == null) return;
                obj.Version = textBox_version.Text;
            };
            textBox_tags.TextChanged += (s, e) =>
            {
                if (obj == null) return;
                obj.Tags = textBox_tags.Text;
            };
            textBox_desc.TextChanged += (s, e) =>
            {
                if (obj == null) return;
                obj.Description = textBox_desc.Text;
            };

            // item
            comboBox_itemType.SelectedIndexChanged += (s, e) =>
            {
                if (obj == null) return;
                obj.ItemType = (string)comboBox_itemType.SelectedItem;
            };
            numericUpDown_price.ValueChanged += (s, e) =>
            {
                if (obj == null) return;
                obj.ItemPrice = (int)numericUpDown_price.Value;
            };
            checkBox_itemConsume.CheckedChanged += (s, e) =>
            {
                if (obj == null) return;
                obj.ItemConsume = checkBox_itemConsume.Checked;
            };
            numericUpDown_maxStack.ValueChanged += (s, e) =>
            {
                if (obj == null) return;
                obj.MaxStack = (int)numericUpDown_maxStack.Value;
            };
            checkBox_itemSync.CheckedChanged += (s, e) =>
            {
                if (obj == null) return;
                obj.Sync = checkBox_itemSync.Checked;
            };
            checkBox_itemPurchasable.CheckedChanged += (s, e) =>
            {
                if (obj == null) return;
                obj.Purchasable = checkBox_itemPurchasable.Checked;
            };

            // material
            colorPanel_albedo.ColorChanged += (s, e) =>
            {
                if (game == null || game.gameObj == null) return;
                var c = new Color4(e.R / 255.0f, e.G / 255.0f, e.B / 255.0f, slider_alpha.Value);
                game.SetMaterialParam<Color4>(listBox_material.SelectedIndex, Resources.Albedo, c);
                ((NwMaterial)listBox_material.SelectedItem).Color4s[Resources.Albedo] = c.ToColor4f();
            };
            colorPanel_emis.ColorChanged += (s, e) =>
            {
                if (game == null || game.gameObj == null) return;
                game.SetMaterialParam<Color4>(listBox_material.SelectedIndex, "emissive", e);
                ((NwMaterial)listBox_material.SelectedItem).Color4s["emissive"] = e.ToColor4f();
            };
            slider_roughness.ValueChanged += (s, e) =>
            {
                if (game == null || game.gameObj == null) return;
                game.SetMaterialParam(listBox_material.SelectedIndex, "roughness", e);
                ((NwMaterial)listBox_material.SelectedItem).Floats["roughness"] = e;
            };
            slider_metallic.ValueChanged += (s, e) =>
            {
                if (game == null || game.gameObj == null) return;
                game.SetMaterialParam(listBox_material.SelectedIndex, "metallic", e);
                ((NwMaterial)listBox_material.SelectedItem).Floats["metallic"] = e;
            };
            slider_reflectance.ValueChanged += (s, e) =>
            {
                if (game == null || game.gameObj == null) return;
                game.SetMaterialParam(listBox_material.SelectedIndex, "reflectance", e);
                ((NwMaterial)listBox_material.SelectedItem).Floats["reflectance"] = e;
            };
            slider_alpha.ValueChanged += (s, e) =>
            {
                if (game == null || game.gameObj == null) return;
                var color = colorPanel_albedo.BackColor;
                var c = new Color4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, e);
                game.SetMaterialParam(listBox_material.SelectedIndex, Resources.Albedo, c);
                ((NwMaterial)listBox_material.SelectedItem).Color4s[Resources.Albedo] = c.ToColor4f();
            };

            // physics
            radioButton_capsule.CheckedChanged += (s, e) =>
            {
                if (obj == null || !radioButton_capsule.Checked) return;
                NwCollisionCapsule cc;
                if (obj.CollisionShape is NwCollisionCapsule)
                {
                    cc = obj.CollisionShape as NwCollisionCapsule;
                }
                else
                {
                    cc = new NwCollisionCapsule();
                    obj.CollisionShape = cc;
                }
                var c = new CollisionCapsule(cc) { Location = new Point(20, 45) };
                
                groupBox_collisionShape.Controls.Remove(collisionShapeCtrl);
                groupBox_collisionShape.Controls.Add(collisionShapeCtrl = c);
                game.SetPhysics(collisionShapeCtrl as ICollisionShape);
            };
            radioButton_cylinder.CheckedChanged += (s, e) =>
            {
                if (obj == null || !radioButton_cylinder.Checked) return;
                NwCollisionCylinder cc;
                if (obj.CollisionShape is NwCollisionCylinder)
                {
                    cc = obj.CollisionShape as NwCollisionCylinder;
                }
                else
                {
                    cc = new NwCollisionCylinder();
                    obj.CollisionShape = cc;
                }
                var c = new CollisionCylinder(cc) { Location = new Point(20, 45) };

                groupBox_collisionShape.Controls.Remove(collisionShapeCtrl);
                groupBox_collisionShape.Controls.Add(collisionShapeCtrl = c);
                game.SetPhysics(collisionShapeCtrl as ICollisionShape);
            };
            radioButton_box.CheckedChanged += (s, e) =>
            {
                if (obj == null || !radioButton_box.Checked) return;
                NwCollisionBox cc;
                if (obj.CollisionShape is NwCollisionBox)
                {
                    cc = obj.CollisionShape as NwCollisionBox;
                }
                else
                {
                    cc = new NwCollisionBox();
                    obj.CollisionShape = cc;
                }
                var c = new CollisionBox(cc) { Location = new Point(20, 45) };

                groupBox_collisionShape.Controls.Remove(collisionShapeCtrl);
                groupBox_collisionShape.Controls.Add(collisionShapeCtrl = c);
                game.SetPhysics(collisionShapeCtrl as ICollisionShape);
            };
            radioButton_sphere.CheckedChanged += (s, e) =>
            {
                if (obj == null || !radioButton_sphere.Checked) return;
                NwCollisionSphere cc;
                if (obj.CollisionShape is NwCollisionSphere)
                {
                    cc = obj.CollisionShape as NwCollisionSphere;
                }
                else
                {
                    cc = new NwCollisionSphere();
                    obj.CollisionShape = cc;
                }
                var c = new CollisionSphere(cc) { Location = new Point(20, 45) };

                groupBox_collisionShape.Controls.Remove(collisionShapeCtrl);
                groupBox_collisionShape.Controls.Add(collisionShapeCtrl = c);
                game.SetPhysics(collisionShapeCtrl as ICollisionShape);
            };

            checkBox_isRigidBody.CheckedChanged += (s, e) =>
            {
                groupBox_isRigidBody.Enabled = checkBox_isRigidBody.Checked;
                obj.PhysicalMaterial.IsRigidBody = checkBox_isRigidBody.Checked;
            };
            numericUpDown_mass.ValueChanged += (s, e) =>
            {
                obj.PhysicalMaterial.Mass = (float)numericUpDown_mass.Value;
            };
            checkBox_freezePosition.CheckedChanged += (s, e) =>
            {
                obj.PhysicalMaterial.FreezePosition = checkBox_freezePosition.Checked;
            };
            checkBox_freezeRotation.CheckedChanged += (s, e) =>
            {
                obj.PhysicalMaterial.FreezeRotation = checkBox_freezeRotation.Checked;
            };
            checkBox_kinematic.CheckedChanged += (s, e) =>
            {
                obj.PhysicalMaterial.Kinematic = checkBox_kinematic.Checked;
            };
            checkBox_disableDeactivation.CheckedChanged += (s, e) =>
            {
                obj.PhysicalMaterial.DisableDeactivation = checkBox_disableDeactivation.Checked;
            };
            slider_friction.ValueChanged += (s, e) =>
            {
                obj.PhysicalMaterial.Friction = e;
            };
            slider_rollingFriction.ValueChanged += (s, e) =>
            {
                obj.PhysicalMaterial.RollingFriction = e;
            };
            slider_restitution.ValueChanged += (s, e) =>
            {
                obj.PhysicalMaterial.Restitution = e;
            };
            slider_linearDamping.ValueChanged += (s, e) =>
            {
                obj.PhysicalMaterial.LinearDamping = e;
            };
            slider_angulerDamping.ValueChanged += (s, e) =>
            {
                obj.PhysicalMaterial.AngulerDamping = e;
            };
            tabControl.SelectedIndexChanged += (s, e) =>
            {
                if (tabControl.SelectedTab == tabPage_physics)
                {
                    game.SetPhysics(collisionShapeCtrl as ICollisionShape);

                    if (collisionShapeCtrl is CollisionCapsule) radioButton_capsule.Checked = true;
                    else if (collisionShapeCtrl is CollisionCylinder) radioButton_cylinder.Checked = true;
                    else if (collisionShapeCtrl is CollisionBox) radioButton_box.Checked = true;
                    else if (collisionShapeCtrl is CollisionSphere) radioButton_sphere.Checked = true;
                    else if (collisionShapeCtrl is CollisionMesh) radioButton_mesh.Checked = true;
                } 
                else game.SetPhysics(null);

                if (tabControl.SelectedTab == tabPage_material) listBox_material_SelectedIndexChanged(this, EventArgs.Empty);
            };

            // motion
            menu_motion.Click += (s, e) =>
            {
                if (obj == null)
                {
                    MessageBox.Show("モデルがまだ読み込まれていません。", "Error", MessageBoxButtons.OK);
                    return;
                }

                var ofd = new OpenFileDialog();
                ofd.Filter = "モーションファイル|*.vmd|すべてのファイル|*.*";

                if (ofd.ShowDialog() != DialogResult.OK) return;

                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    var ext = Path.GetExtension(ofd.FileName);
                    var importer = importers.Find((i) => Array.Exists(i.Extensions, (ex) => ex == ext));

                    var motion = importer.Import(ofd.FileName, ImportType.Full)[0];

                    var mos = ConvertMotion(motion);

                    foreach (var mo in mos)
                    {
                        var idx = dataGridView_motion.Rows.Add("", mo.Name);
                        dataGridView_motion.Rows[idx].Tag = mo;
                    }
                }
                catch
                {
                    MessageBox.Show("ファイルの読み込みに失敗しました。", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                Cursor.Current = Cursors.Default;
            };

            // sound
            menu_sound.Click += (s, e) =>
            {
                if (obj == null)
                {
                    MessageBox.Show("モデルがまだ読み込まれていません。", "Error", MessageBoxButtons.OK);
                    return;
                }

                var ofd = new OpenFileDialog();
                ofd.Filter = "音楽ファイル|*.wav;*.mp3|すべてのファイル|*.*";
                if (ofd.ShowDialog() != DialogResult.OK) return;

                var r = dataGridView_sound.Rows.Add();
                var row = dataGridView_sound.Rows[r];

                try
                {
                    AudioFileReader afd = new AudioFileReader(ofd.FileName);
                    var bytes = File.ReadAllBytes(ofd.FileName);
                    var format = "WAV";
                    if (ofd.FileName.Contains(".mp3")) format = "MP3";
                    var waveOut = new WaveOut();
                    waveOut.Init(afd);

                    var name = Path.GetFileNameWithoutExtension(ofd.FileName);
                    row.Cells[0].Value = name;
                    row.Tag = new Tuple<AudioFileReader, WaveOut, byte[], string>(afd, waveOut, bytes, format);
                }
                catch
                {
                    MessageBox.Show("ファイルの読み込みに失敗しました。", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    dataGridView_sound.Rows.Remove(row);
                    return;
                }
            };
            dataGridView_sound.CellContentClick += (s, e) =>
            {
                var dgv = (DataGridView)s;
                if (dgv.Columns[e.ColumnIndex].Name == "ColumnPlay")
                {
                    var o = dgv.Rows[e.RowIndex].Tag as Tuple<AudioFileReader, WaveOut, byte[], string>;
                    foreach (DataGridViewRow r in dgv.Rows)
                    {
                        var t = r.Tag as Tuple<AudioFileReader, WaveOut, byte[], string>;
                        if (t != null)
                        {
                            t.Item2.Stop();
                            t.Item1.Position = 0;
                        }
                    }
                    o.Item2.Volume = 0.4f;
                    o.Item2.Play();
                }
            };

            // script
            listBox_scripts.DisplayMember = "Name";
            listBox_scripts.SelectedIndexChanged += (s, e) =>
            {
                var info = listBox_scripts.SelectedItem as ScriptInfo;
                if (info != null)
                {
                    textBox_scriptName.Text = info.Script.ScriptName;
                    textBox_scriptDesc.Text = info.Script.ScriptDesc;
                    button_scriptRemove.Enabled = listBox_scripts.Items.Count > 0;
                    button_scriptUp.Enabled = listBox_scripts.SelectedIndex > 0;
                    button_scriptDown.Enabled = listBox_scripts.SelectedIndex < listBox_scripts.Items.Count - 1;
                }
                else
                {
                    textBox_scriptName.Text = "";
                    textBox_scriptDesc.Text = "";
                    button_scriptRemove.Enabled = false;
                    button_scriptUp.Enabled = false;
                    button_scriptDown.Enabled = false;
                }
            };
            button_scriptDown.Click += (s, e) =>
            {
                var idx = listBox_scripts.SelectedIndex;
                var item = listBox_scripts.SelectedItem;
                listBox_scripts.Items.RemoveAt(idx);
                listBox_scripts.Items.Insert(idx + 1, item);
            };
            button_scriptDown.Click += (s, e) =>
            {
                var idx = listBox_scripts.SelectedIndex;
                var item = listBox_scripts.SelectedItem;
                listBox_scripts.Items.RemoveAt(idx);
                listBox_scripts.Items.Insert(idx - 1, item);
            };
            button_scriptRemove.Click += (s, e) =>
            {
                listBox_scripts.Items.RemoveAt(listBox_scripts.SelectedIndex);
            };
            button_scriptImport.Click += (s, e) =>
            {
                var ofd = new OpenFileDialog();
                ofd.Filter = "DLLファイル|*.dll|すべてのファイル|*.*";

                if (ofd.ShowDialog() != DialogResult.OK) return;

                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    var data = File.ReadAllBytes(ofd.FileName);
                    var asm = Assembly.Load(data);
                    var type = asm.GetExportedTypes().Where(t => t.IsSubclassOf(typeof(GameObjectScript))).ToArray()[0];
                    var inst = asm.CreateInstance(type.FullName) as GameObjectScript;

                    listBox_scripts.Items.Add(new ScriptInfo()
                    {
                        Assembly = data,
                        Script = inst,
                    });
                }
                catch
                {
                    MessageBox.Show("ファイルの読み込みに失敗しました。", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                Cursor.Current = Cursors.Default;
            };
        }
        //class Test { public byte[] Data; }

        public void ShowPreviewWindow()
        {
            if (game != null) return;

            task = Task.Factory.StartNew(() =>
            {
                using (Game g = new Game())
                {
                    game = g;
                    g.Run(60.0);
                }
                game = null;
            });
            Thread.Sleep(10);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (game != null)
            {
                game.Close();
                task.Wait();
            }

            var path = System.Environment.CurrentDirectory;
            var files = Directory.EnumerateFiles(path);
            var rems = files.Where(f => f.Contains("tempsound_")).ToList();
            rems.ForEach(r => File.Delete(r));
        }

        private void SetControlParameters(NwObject obj)
        {
            // textures
            textures.AddRange(obj.Texture2Ds);

            // general
            textBox_name.Text = obj.Name;
            textBox_version.Text = obj.Version;
            textBox_desc.Text = obj.Description;
            textBox_editor.Text = obj.Editor;
            textBox_url.Text = obj.EditorURL;
            textBox_tags.Text = obj.Tags;

            if (obj.Thumbnail != null) pictureBox_thumbnail.Image = Util.ToBitmap(obj.Thumbnail.Image);
            else pictureBox_thumbnail.Image = null;

            // item
            comboBox_itemType.SelectedItem = obj.ItemType;
            numericUpDown_price.Value = MMWMath.Clamp((int)obj.ItemPrice, 0, 1000);
            checkBox_itemConsume.Checked = obj.ItemConsume;
            numericUpDown_maxStack.Value = MMWMath.Clamp(obj.MaxStack, 1, 99);
            checkBox_itemSync.Checked = obj.Sync;
            checkBox_itemPurchasable.Checked = obj.Purchasable;

            // materials
            listBox_material.DisplayMember = "Name";
            foreach (var mat in obj.Materials) listBox_material.Items.Add(mat);
            listBox_material.SelectedIndex = 0;

            // physics
            if (obj.CollisionShape is NwCollisionCapsule) radioButton_capsule.Checked = true;
            else if (obj.CollisionShape is NwCollisionCylinder) radioButton_cylinder.Checked = true;
            else if (obj.CollisionShape is NwCollisionBox) radioButton_box.Checked = true;
            else if (obj.CollisionShape is NwCollisionSphere) radioButton_sphere.Checked = true;
            else if (obj.CollisionShape is NwCollisionMesh) radioButton_mesh.Checked = true;
            checkBox_isRigidBody.Checked = obj.PhysicalMaterial.IsRigidBody;
            if (obj.PhysicalMaterial.IsRigidBody)
            {
                numericUpDown_mass.Value = (decimal)obj.PhysicalMaterial.Mass;
                checkBox_freezePosition.Checked = obj.PhysicalMaterial.FreezePosition;
                checkBox_freezeRotation.Checked = obj.PhysicalMaterial.FreezeRotation;
                checkBox_kinematic.Checked = obj.PhysicalMaterial.Kinematic;
                checkBox_disableDeactivation.Checked = obj.PhysicalMaterial.DisableDeactivation;

                slider_friction.Value = obj.PhysicalMaterial.Friction;
                slider_rollingFriction.Value = obj.PhysicalMaterial.RollingFriction;
                slider_restitution.Value = obj.PhysicalMaterial.Restitution;
                slider_linearDamping.Value = obj.PhysicalMaterial.LinearDamping;
                slider_angulerDamping.Value = obj.PhysicalMaterial.AngulerDamping;
            }

            // motions
            foreach (var m in obj.Motions)
            {
                var i = dataGridView_motion.Rows.Add(m.Key, m.Name);
                dataGridView_motion.Rows[i].Tag = m;
            }
  
            // morphs

            // sounds
            foreach (var s in obj.Sounds)
            {
                var i = dataGridView_sound.Rows.Add();
                var row = dataGridView_sound.Rows[i];
                row.Cells[0].Value = s.Name;

                var path = System.Environment.CurrentDirectory + "/tempsound_" + Util.CreateHash();
                File.WriteAllBytes(path, s.Data);
                var afd = new AudioFileReader(path);
                var waveOut = new WaveOut();
                waveOut.Init(afd);

                row.Tag = new Tuple<AudioFileReader, WaveOut, byte[], string>(afd, waveOut, s.Data, s.Format);
            }

            // scripts
            foreach (var s in obj.Scripts)
            {
                var asm = Assembly.Load(s.Assembly);
                var type = asm.GetExportedTypes().Where(t => t.IsSubclassOf(typeof(GameObjectScript))).ToArray()[0];
                var inst = asm.CreateInstance(type.FullName) as GameObjectScript;

                listBox_scripts.Items.Add(new ScriptInfo()
                {
                    Assembly = s.Assembly,
                    Script = inst,
                });
            }

            // property
            var idx = 0;
            foreach (var p in obj.Properties)
            {
                idx = dataGridView_property.Rows.Add(1);
                dataGridView_property[0, idx].Value = p.Key;
                dataGridView_property[1, idx].Value = p.Value;
            }
        }

        private void listBox_material_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox_material.SelectedItem == null) return;
            var mat = listBox_material.SelectedItem as NwMaterial;

            colorPanel_albedo.BackColor = mat.Color4s[Resources.Albedo].ToColor(255);
            colorPanel_emis.BackColor = mat.Color4s["emissive"].ToColor(255);

            slider_alpha.Value = mat.Color4s[Resources.Albedo].A;
            slider_roughness.Value = mat.Floats["roughness"];
            slider_metallic.Value = mat.Floats["metallic"];
            slider_reflectance.Value = mat.Floats["reflectance"];

            if (mat.Texture2Ds["albedoMap"] != null) pictureBox_albedoMap.Image = Util.ToBitmap(Array.Find(obj.Texture2Ds, t => t.Hash == mat.Texture2Ds["albedoMap"]).Image);
            else pictureBox_albedoMap.Image = null;
            if (mat.Texture2Ds["normalMap"] != null) pictureBox_normalMap.Image = Util.ToBitmap(Array.Find(obj.Texture2Ds, t => t.Hash == mat.Texture2Ds["normalMap"]).Image);
            else pictureBox_normalMap.Image = null;
            if (mat.Texture2Ds["physicalMap"] != null) pictureBox_physicalMap.Image = Util.ToBitmap(Array.Find(obj.Texture2Ds, t => t.Hash == mat.Texture2Ds["physicalMap"]).Image);
            else pictureBox_physicalMap.Image = null;
        }

        private void pictureBox_Click(object sender, EventArgs e)
        {
            var pic = (PictureBox)sender;

            var ofd = new OpenFileDialog();
            ofd.Filter = "画像ファイル|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tif;*.tiff;*.tga|すべてのファイル|*.*";
            if (ofd.ShowDialog() != DialogResult.OK) return;

            var bitmap = Util.FromImageFile(ofd.FileName);
            if (bitmap == null)
            {
                MessageBox.Show("ファイルの読み込みに失敗しました。", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            pic.Image = bitmap;
            game.SetTexture(listBox_material.SelectedIndex, (string)pic.Tag, bitmap);

            var buf = Util.FromBitmap(bitmap);
            var tex = new NwTexture2D()
            {
                Image = buf,
                Hash = Util.ComputeHash(buf, 12),
                Name = Path.GetFileNameWithoutExtension(ofd.FileName),
            };

            var find = textures.Find(t => t.Hash == tex.Hash);
            if (find == null) textures.Add(tex);

            var mat = listBox_material.SelectedItem as NwMaterial;
            string hash = null;
            mat.Texture2Ds.TryGetValue((string)pic.Tag, out hash);
            if (find == null && hash == null)
                mat.Texture2Ds[(string)pic.Tag] = tex.Hash;
            else if (find != null && hash == null)
                mat.Texture2Ds[(string)pic.Tag] = find.Hash;
            else if (find != null && hash != null)
            {
                mat.Texture2Ds.Remove((string)pic.Tag);
                mat.Texture2Ds[(string)pic.Tag] = find.Hash;
            }
            else if (find == null && hash != null)
            {
                mat.Texture2Ds.Remove((string)pic.Tag);
                mat.Texture2Ds[(string)pic.Tag] = tex.Hash;
            }
        }
        private void pictureBox_thumbnail_Click(object sender, EventArgs e)
        {
            var pic = (PictureBox)sender;

            var ofd = new OpenFileDialog();
            ofd.Filter = "画像ファイル|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tif;*.tiff;*.tga|すべてのファイル|*.*";
            if (ofd.ShowDialog() != DialogResult.OK) return;

            var bitmap = Util.FromImageFile(ofd.FileName);
            if (bitmap == null)
            {
                MessageBox.Show("ファイルの読み込みに失敗しました。", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            pic.Image = bitmap;

            var buf = Util.FromBitmap(bitmap);
            var tex = new NwTexture2D()
            {
                Image = buf,
                Hash = Util.ComputeHash(buf, 12),
                Name = Path.GetFileNameWithoutExtension(ofd.FileName),
            };

            obj.Thumbnail = tex;

            if (!textures.Exists(t => t.Hash == tex.Hash)) textures.Add(tex);
        }

        private void Save()
        {
            Cursor.Current = Cursors.WaitCursor;

            // material
            foreach (var t in textures.ToArray())
            {
                var flag = false;
                foreach (var m in obj.Materials)
                {
                    foreach (var v in m.Texture2Ds.Values)
                    {
                        if (t.Hash == v) flag = true;
                    }
                }

                if (!flag) textures.Remove(t);
            }
            obj.Texture2Ds = textures.ToArray();

            // motion
            obj.Motions = new NwMotion[dataGridView_motion.Rows.Count];
            for (var i = 0; i < obj.Motions.Length; i++)
            {
                var m = dataGridView_motion.Rows[i].Tag as NwMotion;
                m.Key = dataGridView_motion.Rows[i].Cells[0].Value as string;
                m.Name = dataGridView_motion.Rows[i].Cells[1].Value as string;

                obj.Motions[i] = m;
            }

            // sound
            obj.Sounds = new NwSound[dataGridView_sound.Rows.Count];
            for (var i = 0; i < obj.Sounds.Length; i++)
            {
                var d = dataGridView_sound.Rows[i].Tag as Tuple<AudioFileReader, WaveOut, byte[], string>;
                obj.Sounds[i] = new NwSound()
                {
                    Name = dataGridView_sound.Rows[i].Cells[0].Value as string,
                    Data = d.Item3,
                    Format = d.Item4,
                    Hash = Util.ComputeHash(d.Item3, 12),
                };
            }

            // script
            obj.Scripts = new NwGameObjectScript[listBox_scripts.Items.Count];
            for (var i = 0; i < obj.Scripts.Length; i++)
            {
                var info = listBox_scripts.Items[i] as ScriptInfo;
                obj.Scripts[i] = new NwGameObjectScript()
                {
                    Assembly = info.Assembly,
                    Hash = Util.ComputeHash(info.Assembly, 12),
                };
            }

            // property
            obj.Properties.Clear();
            var row = dataGridView_property.RowCount;
            for (var i = 0; i < row; i++)
            {
                if (dataGridView_property[0, i].Value == null) continue;
                obj.Properties.Add(dataGridView_property[0, i].Value as string, dataGridView_property[1, i].Value as string);
            }

            var json = Util.SerializeJson(obj);
            var data = Util.Compress(Encoding.UTF8.GetBytes(json));
            File.WriteAllBytes(filepath, data);

            Cursor.Current = Cursors.Default;
        }
        private void menu_save_Click(object sender, EventArgs e)
        {
            if (Filepath == null)
            {
                var sfd = new SaveFileDialog();
                sfd.FileName = ".mwo";
                sfd.Filter = "MWOファイル(*.mwo)|*.mwo|すべてのファイル(*.*)|*.*";

                var res = sfd.ShowDialog();
                if (res != DialogResult.OK) return;

                Filepath = sfd.FileName;
            }

            Save();
        }
        private void menu_saveas_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.FileName = ".mwo";
            sfd.Filter = "MWOファイル(*.mwo)|*.mwo|すべてのファイル(*.*)|*.*";

            var res = sfd.ShowDialog();
            if (res != DialogResult.OK) return;

            Filepath = sfd.FileName;

            Save();
        }
        private void menu_open_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "MWOファイル|*.mwo|すべてのファイル|*.*";

            if (ofd.ShowDialog() != DialogResult.OK) return;

            if (Filepath != null)
            {
                var res = MessageBox.Show("すでにデータが読み込まれています。\r\n変更を破棄して新しく読み込みますか?", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (res == DialogResult.No) return;
            }

            Clear();

            Cursor.Current = Cursors.WaitCursor;
            //try
            //{
                var data = File.ReadAllBytes(ofd.FileName);
                var decomp = Util.Decompress(data);

                obj = Util.DeserializeJson<NwObject>(Encoding.UTF8.GetString(decomp));

                game.OnObjectLoaded(obj);
                while (game.gameObj == null) ;
                SetControlParameters(obj);
                
                Filepath = ofd.FileName;
                tabControl.Enabled = true;
            //}
            //catch
            //{
            //    MessageBox.Show("ファイルの読み込みに失敗しました。", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    Clear();
            //}
            Cursor.Current = Cursors.Default;
        }
        private void menu_model_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "対応ファイル|*.pmd;*.pmx;*.mqo|すべてのファイル|*.*";

            if (ofd.ShowDialog() != DialogResult.OK) return;

            if (Filepath != null)
            {
                var res = MessageBox.Show("すでにデータが読み込まれています。\r\n変更を破棄して新しく読み込みますか?", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (res == DialogResult.No) return;
            }

            Clear();

            Cursor.Current = Cursors.WaitCursor;
            //try
            //{
                var ext = Path.GetExtension(ofd.FileName);
                var importer = importers.Find((i) => Array.Exists(i.Extensions, (ex) => ex == ext));

                var o = importer.Import(ofd.FileName, ImportType.Full)[0];

                obj = ConvertObject(o);
                SetControlParameters(obj);
                game.OnObjectLoaded(obj);
                tabControl.Enabled = true;
            //}
            //catch
            //{
            //    MessageBox.Show("ファイルの読み込みに失敗しました。", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
            Cursor.Current = Cursors.Default;
        }
        
        private NwObject ConvertObject(ImportedObject obj)
        {
            var o = new NwObject();

            {
                o.Name = obj.Name;
                o.Description = obj.Description;
                o.Version = obj.Version;
                o.Editor = obj.Editor;
                o.EditorURL = obj.EditorURL;

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

                if (obj.Bones != null)
                {
                    var bones = new List<NwBone>();

                    foreach (var b in obj.Bones)
                    {
                        var bone = AssetConverter.ToNwBone(b);
                        bones.Add(bone);
                    }
                    o.Bones = bones.ToArray();
                }
                    

                o.Mesh = AssetConverter.ToNwMesh(obj.Meshes[0]);

                o.CollisionShape = new NwCollisionCapsule()
                {
                    Height = 1.6f,
                    Radius = 0.3f,
                };

                o.PhysicalMaterial = new NwPhysicalMaterial();
            }
            return o;
        }
        private NwMotion[] ConvertMotion(ImportedObject obj)
        {
            var mos = new List<NwMotion>();
            if (obj.Motions == null) obj.Motions = new Motion[0];

            foreach (var mo in obj.Motions)
            {
                var m = new NwMotion();
                m.Name = mo.Name;

                m.BoneMotion = new Dictionary<string, NwBoneMotion>();
                if (mo.BoneMotions != null)
                {
                    foreach (var bm in mo.BoneMotions)
                    {
                        var bmv = new NwBoneMotion();
                        bmv.BoneName = bm.Value.BoneName;
                        bmv.Keys = new List<NwBoneMotionValue>();
                        foreach (var k in bm.Value.Keys)
                        {
                            var v = new NwBoneMotionValue();
                            v.FrameNo = k.FrameNo;
                            v.Location = k.Value.location.ToVec3f();
                            v.Rotation = new Vector4f(k.Value.rotation.X, k.Value.rotation.Y, k.Value.rotation.Z, k.Value.rotation.W);
                            v.Scale = k.Value.scale.ToVec3f();

                            v.Interpolate = ConvertInterpolate(k.Interpolate);

                            bmv.Keys.Add(v);
                        }

                        m.BoneMotion.Add(bmv.BoneName, bmv);
                    }
                }

                m.MorphMotion = new Dictionary<string, NwMorphMotion>();
                if (mo.SkinMotions != null)
                {
                    foreach (var sm in mo.SkinMotions)
                    {
                        var mmv = new NwMorphMotion();
                        mmv.MorphName = sm.Value.MorphName;
                        mmv.Keys = new List<NwMorphMotionValue>();
                        foreach (var k in sm.Value.Keys)
                        {
                            var v = new NwMorphMotionValue();
                            v.FrameNo = k.FrameNo;
                            v.Rate = k.Value;
                            v.Interpolate = ConvertInterpolate(k.Interpolate);

                            mmv.Keys.Add(v);
                        }

                        m.MorphMotion.Add(mmv.MorphName, mmv);
                    }
                }

                mos.Add(m);
            }

            return mos.ToArray();
        }
        private NwInterpolate ConvertInterpolate(IInterpolate ip)
        {
            if (ip is BezierInterpolate)
            {
                var i = ip as BezierInterpolate;
                return new NwInterpolate() { P1 = i.p1.ToVec2f(), P2 = i.p2.ToVec2f() };
            }
            else if (ip is LinearInterpolate) return new NwInterpolate() { P1 = new Vector2f(0.25f, 0.25f), P2 = new Vector2f(0.75f, 0.75f) };
            else if (ip is SmoothstepInterpolate) return new NwInterpolate() { P1 = new Vector2f(0.25f, 0.0f), P2 = new Vector2f(0.75f, 1.0f) };

            return null;
        }

        private void Clear()
        {
            Filepath = null;
            game.OnClear();
            textures.Clear();

            // general
            textBox_name.Clear();
            textBox_editor.Clear();
            textBox_url.Clear();
            textBox_tags.Clear();
            textBox_version.Clear();
            textBox_desc.Clear();
            pictureBox_thumbnail.Image = null;

            // item
            comboBox_itemType.SelectedIndex = 0;
            numericUpDown_price.Value = 5;
            checkBox_itemConsume.Checked = true;
            numericUpDown_maxStack.Value = 1;
            checkBox_itemSync.Checked = false;
            checkBox_itemPurchasable.Checked = true;

            // material
            listBox_material.Items.Clear();
            colorPanel_albedo.BackColor = Color.Black;
            colorPanel_emis.BackColor = Color.Black;

            // physics
            radioButton_capsule.Checked = false;
            radioButton_cylinder.Checked = false;
            radioButton_box.Checked = false;
            radioButton_sphere.Checked = false;
            radioButton_mesh.Checked = false;
            if (collisionShapeCtrl != null)
            {
                groupBox_collisionShape.Controls.Remove(collisionShapeCtrl);
                collisionShapeCtrl = null;
            }

            // motions
            dataGridView_motion.Rows.Clear();

            // morphs

            // sounds
            dataGridView_sound.Rows.Clear();

            // scripts
            listBox_scripts.Items.Clear();
            textBox_scriptName.Clear();
            textBox_scriptDesc.Clear();

            // proerties
            dataGridView_property.Rows.Clear();

            tabControl.Enabled = false;
        }
    }
}
