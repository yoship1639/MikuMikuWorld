using MikuMikuWorld;
using MikuMikuWorld.Network;
using MikuMikuWorld.Walker;
using MikuMikuWorld.Walker.Network;
using MikuMikuWorld_Walker_Server.Commands;
using MikuMikuWorldScript;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MikuMikuWorld_Walker_Server
{
    public partial class MainForm : Form
    {
        public static readonly float Version = 1.0f;
        public static readonly string VersionStr = "v1.0";
        public static readonly string AppName = "walker";

        #region Property

        #endregion

        private UdpClient noticeUdp;
        public NetworkServer Server { get; private set; }

        public int PlayerNum => listBox_player.Items.Count;
        public int MaxPlayer => (int)numericUpDown_maxplyer.Value;
        public int MemoryUsed => (int)(Environment.WorkingSet / 1024 / 1024);
        public int CPUUsed => (int)pc.NextValue();

        public static readonly string RootDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\MikuMikuWorld\";
        public static readonly string AppDir = RootDir + AppName + "\\" + VersionStr + "\\";
        public string WorldDir => AppDir + WorldDirName + "\\";
        public string WorldDirName { get; set; } = "testWorld";

        public string BlacklistPath => WorldDir + "black.ini";
        public string SettingsPath => WorldDir + "server.ini";
        public string LogPath { get; set; }

        public string DataDir => WorldDir + "data\\";
        public string DataWorldDir => DataDir + "world\\";
        public string DataCharDir => DataDir + "character\\";
        public string DataObjDir => DataDir + "object\\";
        public string DataGameObjScriptDir => DataDir + "gameobjectscript\\";

        public string SaveDir => WorldDir + "save\\";
        public string ScriptDir => WorldDir + "script\\";
        public string UserDataDir => WorldDir + "userdata\\";

        public Dictionary<string, byte[]> WorldHashmap = new Dictionary<string, byte[]>();
        public Dictionary<string, byte[]> CharHashmap = new Dictionary<string, byte[]>();
        public Dictionary<string, byte[]> ObjHashmap = new Dictionary<string, byte[]>();
        public Dictionary<string, byte[]> ScriptHashmap = new Dictionary<string, byte[]>();
        public Dictionary<string, byte[]> GameObjectScriptHashmap = new Dictionary<string, byte[]>();

        public Dictionary<string, NwWalkerGameObject> GameObjectHashmap = new Dictionary<string, NwWalkerGameObject>();

        System.Windows.Forms.Timer timer;
        PerformanceCounter pc;
        public string worldImageString;
        public string worldIconString;

        public List<Cmd> Commands = new List<Cmd>()
        {
            new CmdWorldInfo(),
            new CmdLogin(),
            new CmdWorldDataDesc(),
            new CmdWorldData(),
            new CmdChat(),
            new CmdPlayerTransform(),
            new CmdObjectPicked(),
            new CmdObjectPut(),
            new CmdGameObjects(),
            new CmdWorldStatus(),
            new CmdReady(),
            new CmdPictureChat(),
            new CmdItemUsed(),
            new CmdScriptStatus(),
        };

        public Settings Settings { get; private set; }

        public MainForm()
        {
            InitializeComponent();

            var dirs = Directory.GetDirectories(AppDir);


            CheckForIllegalCrossThreadCalls = false;
        }

        private void FormServer_Load(object sender, EventArgs e)
        {
            var date = DateTime.Now.ToString().Replace('/', '_').Replace(':', '_');
            LogPath = WorldDir + "log\\" + date + ".txt";

            var infos = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            Array.Sort(infos, (a, b) => a.EnglishName.CompareTo(b.EnglishName));
            comboBox_culture.Items.AddRange(infos);
            comboBox_culture.DisplayMember = "EnglishName";

            LoadSettings(SettingsPath);


            listBox_player.Items.Clear();
            listBox_worldObject.DisplayMember = "Name";

            Server = new NetworkServer(IPAddress.Parse("127.0.0.1"), (int)numericUpDown_tcpport.Value, (int)numericUpDown_udpport.Value);
            Server.PeerConnected += Server_PeerConnected;
            Server.PeerAccepted += Server_PeerAccepted;
            Server.PeerDataReceived += Server_PeerDataReceived;
            Server.PeerDisconnected += Server_PeerDisconnected;
            Server.PeerRejected += Server_PeerRejected;

            // blacklist
            try
            {
                Server.Blacklist = Util.DeserializeJson<Blacklist>(File.ReadAllText(BlacklistPath));
                if (Server.Blacklist == null) Server.Blacklist = new Blacklist();
            }
            catch
            {
                Server.Blacklist = new Blacklist();
            }

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 500;
            timer.Tick += Timer_Tick;
            timer.Start();

            pc = new PerformanceCounter("Processor", "% Processor Time", "_Total");

            MikuMikuWorld_Walker_Server.Log.form = this;


            // world object
            listBox_worldObject.SelectedIndexChanged += (s, ea) =>
            {
                button_worldObj_destroy.Enabled = listBox_worldObject.SelectedItem != null;
            };
            button_worldObj_destroy.Click += (s, ea) =>
            {
                var go = listBox_worldObject.SelectedItem as NwWalkerGameObject;
                if (go == null) return;
                Server.SendTcp(DataType.ResponseObjectDestroy, Buffer.Write(bw => bw.Write(go.Hash)));
                RemoveWorldObject(go.Hash);
            };
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            label_memory.Text = "Memory: " + MemoryUsed + " MB";
            label_cpu.Text = "CPU: " + CPUUsed + " %";
            label_playerNum.Text = "Player: " + PlayerNum + "/" + MaxPlayer;
            label_objCount.Text = "WorldObjectCount: " + listBox_worldObject.Items.Count;
        }

        private System.Windows.Forms.Timer updateTimer;
        private System.Windows.Forms.Timer saveTimer;
        private void button_start_Click(object sender, EventArgs e)
        {
            // アセット検証
            {
                LogInfo("validating assets...", true);

                // world
                foreach (var path in Directory.EnumerateFiles(DataWorldDir, "*.mww"))
                {
                    try
                    {
                        var data = File.ReadAllBytes(path);
                        var hash = Util.ComputeHash(data, 12);
                        if (WorldHashmap.ContainsKey(hash)) continue;

                        var decomp = Util.Decompress(data);
                        var world = Util.DeserializeJson<NwWorld>(Encoding.UTF8.GetString(decomp));
                        WorldHashmap.Add(hash, data);
                        LogInfo($"ok ({Path.GetFileName(path)})", true);
                    }
                    catch
                    {
                        LogError($"incorrect world data ({Path.GetFileName(path)})", true);
                        LogError("please check whether the data is broken", true);
                        return;
                    }
                }

                // character
                foreach (var path in Directory.EnumerateFiles(DataCharDir, "*.mwc"))
                {
                    try
                    {
                        var data = File.ReadAllBytes(path);
                        var hash = Util.ComputeHash(data, 12);
                        if (CharHashmap.ContainsKey(hash)) continue;

                        var decomp = Util.Decompress(data);
                        var ch = Util.DeserializeJson<NwCharacter>(Encoding.UTF8.GetString(decomp));
                        CharHashmap.Add(hash, data);
                        LogInfo($"ok ({Path.GetFileName(path)})", true);
                    }
                    catch
                    {
                        LogError($"incorrect character data ({Path.GetFileName(path)})", true);
                        LogError("please check whether the data is broken", true);
                        return;
                    }
                }

                // obj
                foreach (var path in Directory.EnumerateFiles(DataObjDir, "*.mwo"))
                {
                    try
                    {
                        var data = File.ReadAllBytes(path);
                        var hash = Util.ComputeHash(data, 12);
                        if (ObjHashmap.ContainsKey(hash)) continue;

                        var decomp = Util.Decompress(data);
                        var ch = Util.DeserializeJson<NwObject>(Encoding.UTF8.GetString(decomp));
                        ObjHashmap.Add(hash, data);
                        LogInfo($"ok ({Path.GetFileName(path)})", true);
                    }
                    catch
                    {
                        LogError($"incorrect object data ({Path.GetFileName(path)})", true);
                        LogError("please check whether the data is broken", true);
                        return;
                    }
                }

                // gameobject script
                foreach (var path in Directory.EnumerateFiles(DataGameObjScriptDir, "*.dll"))
                {
                    try
                    {
                        var data = File.ReadAllBytes(path);
                        var hash = Util.ComputeHash(data, 12);
                        if (GameObjectScriptHashmap.ContainsKey(hash)) continue;

                        var asm = Assembly.Load(data);
                        var inst = asm.CreateInstance(typeof(GameObjectScript).FullName) as GameObjectScript;
                        GameObjectScriptHashmap.Add(hash, data);
                        LogInfo($"ok ({Path.GetFileName(path)})", true);
                    }
                    catch
                    {
                        LogError($"incorrect gameobjectscript ({Path.GetFileName(path)})", true);
                        LogError("please check whether the data is broken", true);
                        return;
                    }
                }

                LogInfo("verification passed", true);
            }

            button_start.Enabled = false;
            button_stop.Enabled = true;

            groupBox_settingMain.Enabled = false;
            groupBox_settingWorld.Enabled = false;

            Server.TcpPort = (int)numericUpDown_tcpport.Value;
            Server.UdpStartPort = (int)numericUpDown_udpport.Value;
            Server.MaxConnection = (int)numericUpDown_maxplyer.Value;

            if (!string.IsNullOrWhiteSpace(textBox_worldImagePath.Text))
            {
                try
                {
                    var img = (Bitmap)Image.FromFile(textBox_worldImagePath.Text);
                    if (img.Width > 1280 || img.Height > 720) throw new Exception();
                    worldImageString = Util.ToBitmapString(img);
                }
                catch
                {
                    LogWarn("failed to open world image. the image is broken or the image size is too big.", true);
                }
            }
            if (!string.IsNullOrWhiteSpace(textBox_worldIconPath.Text))
            {
                try
                {
                    var img = (Bitmap)Image.FromFile(textBox_worldIconPath.Text);
                    if (img.Width > 64 || img.Height > 64) throw new Exception();
                    worldIconString = Util.ToBitmapString(img);
                }
                catch
                {
                    LogWarn("failed to open world icon. the image is broken or the image size is too big.", true);
                }
            }

            LoadData();

            Server.Start();
            label_run.Text = "Status: Running";
            LogInfo("start server", true);

            Task.Factory.StartNew(() =>
            {
                try
                {
                    while (true)
                    {
                        using (noticeUdp = new UdpClient(new IPEndPoint(IPAddress.Any, Server.TcpPort)))
                        {
                            IPEndPoint ep = null;
                            byte[] data = noticeUdp.Receive(ref ep);
                            
                            try
                            {
                                //LogInfo("received", true);
                                if (!data.Take(4).SequenceEqual(NetworkServer.Magic)) continue;
                                if (BitConverter.ToInt32(data, 4) != DataType.RequestServerDesc) continue;
                            }
                            catch { }
                            
                            Thread.Sleep(10);

                            var port = BitConverter.ToInt32(data, 8);

                            var info = CreateWorldInfo();
                            info.WorldImage = null;
                            var buf = Util.SerializeJsonBinary(info, false);

                            noticeUdp.Send(buf, buf.Length, new IPEndPoint(ep.Address, port));
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex.ToString(), true);
                }
            });

            if (updateTimer != null)
            {
                updateTimer.Stop();
                updateTimer = null;
            }
            updateTimer = new System.Windows.Forms.Timer();
            updateTimer.Interval = (int)numericUpDown_udpInterval.Value;
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();

            saveTimer = new System.Windows.Forms.Timer();
            saveTimer.Interval = 5000;
            saveTimer.Tick += (s, ev) =>
            {
                SaveData();
            };
            saveTimer.Start();
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                var buf = Buffer.Write(bw =>
                {
                    // send transform
                    bw.Write(DataType.ResponseAllTransform);
                    var peers = Array.FindAll(Server.Peers, p => !p.Pending && p.tcp.Connected);
                    bw.Write(28 * peers.Length);
                    for (var i = 0; i < peers.Length; i++)
                    {
                        if (peers[i].Player == null) continue;
                        bw.Write(peers[i].SessionID);

                        bw.Write(peers[i].Player.Position.X);
                        bw.Write(peers[i].Player.Position.Y);
                        bw.Write(peers[i].Player.Position.Z);

                        bw.Write(peers[i].Player.Rotation.X);
                        bw.Write(peers[i].Player.Rotation.Y);
                        bw.Write(peers[i].Player.Rotation.Z);
                    }
                });
                Server.SendUdp(buf);
            }
            catch { }
        }

        private void button_stop_Click(object sender, EventArgs e)
        {
            button_start.Enabled = true;
            button_stop.Enabled = false;

            groupBox_settingMain.Enabled = true;
            groupBox_settingWorld.Enabled = true;

            Server.Stop();
            label_run.Text = "Status: Stop";
            LogInfo("stop server", true);

            listBox_player.Items.Clear();

            noticeUdp.Close();
            noticeUdp = null;

            updateTimer.Stop();
            updateTimer.Tick -= UpdateTimer_Tick;
            updateTimer = null;

            saveTimer.Stop();
            saveTimer = null;
            SaveData();

            WorldHashmap.Clear();
            CharHashmap.Clear();
            ObjHashmap.Clear();
            ScriptHashmap.Clear();
            GameObjectScriptHashmap.Clear();
        }

        private void Server_PeerConnected(object sender, PeerEventArgs e)
        {
            LogInfo("connected: " + e.peer.IPAddress, true);

            foreach (var cmd in Commands) cmd.OnPeerConnected(this, e.peer);
        }
        private void Server_PeerAccepted(object sender, PeerEventArgs e)
        {
            foreach (var cmd in Commands) cmd.OnPeerAccepted(this, e.peer);
            listBox_player.Items.Add(e.peer);
            LogInfo("joined: " + e.peer, true);
        }
        private void Server_PeerDataReceived(object sender, PeerEventArgs e)
        {
            if (e.isTcp)
            {
                var dataType = BitConverter.ToInt32(e.data, 0);
                var cmd = Commands.Find(c => c.ExecDataTypes.Contains(dataType));
                if (cmd != null)
                {
                    try
                    {
                        LogInfo(cmd.GetType().Name, true);
                        var res = cmd.OnDataReceived(this, true, e.peer, dataType, e.data.Skip(4).ToArray());
                    }
                    catch (Exception ex)
                    {
                        LogError(ex.ToString(), true);
                    }
                }
            }
            else
            {
                using (var ms = new MemoryStream(e.data))
                {
                    using (var br = new BinaryReader(ms))
                    {
                        try
                        {
                            while (ms.Position < ms.Length)
                            {
                                var dataType = br.ReadInt32();
                                var l = br.ReadInt32();
                                if (l == 0) break;
                                var data = br.ReadBytes(l);

                                var cmd = Commands.Find(c => c.ExecDataTypes.Contains(dataType));
                                if (cmd != null)
                                {
                                    try
                                    {
                                        cmd.OnDataReceived(this, false, e.peer, dataType, data);
                                    }
                                    catch (Exception ex)
                                    {
                                        LogError(ex.ToString(), true);
                                    }
                                } 
                            }
                        }
                        catch { }
                    }
                }
            }
        }
        private void Server_PeerDisconnected(object sender, PeerEventArgs e)
        {
            listBox_player.Items.Remove(e.peer);
            LogInfo("left: " + e.peer, true);
        }
        private void Server_PeerRejected(object sender, IPEndPoint e)
        {
            LogInfo("rejected: " + e.Address, false);
        }

        #region Save
        private bool SaveData()
        {
            var path = SaveDir + "objects.json";
            try
            {
                var json = Util.SerializeJson(GameObjectHashmap, false);
                File.WriteAllText(path, json);
                return true;
            }
            catch { }

            return false;
        }
        private bool LoadData()
        {
            var path = SaveDir + "objects.json";
            //try
            //{
                var json = File.ReadAllText(path);
                var data = Util.DeserializeJson<Dictionary<string, NwWalkerGameObject>>(json, false);

                foreach (var obj in data)
                {
                    //try
                    //{
                        AddWorldObject(obj.Value);
                    //}
                    //catch (Exception ex)
                    //{
                    //    LogError(ex.ToString(), true);
                    //}
                }

                LogInfo("loaded objects.json", true);
                return true;
            //}
            //catch { }

            return false;
        }
        #endregion

        #region Log
        private void Log(string type, string mes, bool detail)
        {
            var str = $"[{type}]({DateTime.Now}) " + mes + Environment.NewLine;
            try
            {
                File.AppendAllText(LogPath, str);
            }
            catch { }
            if (!detail) return;
            if (checkBox_logDetail.Checked) textBox_log.AppendText(str);
            else textBox_log.AppendText(mes + Environment.NewLine);
        }
        public void LogInfo(string mes, bool detail)
        {
            Log("INFO", mes, detail);
        }
        public void LogWarn(string mes, bool detail)
        {
            Log("WARN", mes, detail);
        }
        public void LogError(string mes, bool detail)
        {
            Log("ERROR", mes, detail);
        }
        #endregion

        private void textBox_cmd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;

            var cmd = textBox_cmd.Text;
            textBox_cmd.Text = null;

            ExecCommand(cmd);
        }
        public bool ExecCommand(string cmd)
        {
            if (string.IsNullOrWhiteSpace(cmd)) return false;

            var c = Commands.Find(cm => cm.Executable(cmd));
            if (c != null) return c.Exec(this, cmd);

            return false;
        }

        private void listBox_player_SelectedIndexChanged(object sender, EventArgs e)
        {
            var flag = listBox_player.SelectedItem != null;
            button_playerlog.Enabled = flag;
            button_blacklist.Enabled = flag;
            button_ban.Enabled = flag;
        }

        #region Settings
        public void LoadSettings(string path)
        {
            try
            {
                Settings = Util.DeserializeJson<Settings>(File.ReadAllText(SettingsPath), false);
                LogInfo("loaded server.ini", true);
            }
            catch
            {
                Settings = new Settings();
                LogWarn("failed to load server.ini", true);
            }

            numericUpDown_tcpport.Value = Settings.TcpPort;
            numericUpDown_udpport.Value = Settings.UdpPort;
            numericUpDown_maxplyer.Value = Settings.MaxPlayer;
            numericUpDown_udpInterval.Value = Settings.UdpInterval;

            checkBox_useScript.Checked = Settings.UseScript;
            checkBox_userModel.Checked = Settings.UserModel;

            textBox_serverName.Text = Settings.WorldName;
            textBox_serverDesc.Text = Settings.WorldDesc;
            textBox_worldPass.Text = Settings.WorldPass;
            checkBox_worldPass.Checked = Settings.UseWorldPass;
            textBox_worldImagePath.Text = Settings.WorldImagePath;
            textBox_worldIconPath.Text = Settings.WorldIconPath;

            if (Settings.PublicKey == null)
            {
                var ds = DigitalSignature.Generate();
                Settings.PublicKey = Util.ToBase58(ds.PublicKey);
                Settings.PrivateKey = Util.ToBase58(ds.PrivateKey);
            }

            try
            {
                if (Settings.Culture == -1 || Settings.Culture == 0) comboBox_culture.SelectedItem = CultureInfo.CurrentCulture;
                else comboBox_culture.SelectedItem = CultureInfo.GetCultureInfo(Settings.Culture);
            }
            catch { }
        }
        public void SaveSettings(string path)
        {
            Settings.TcpPort = (int)numericUpDown_tcpport.Value;
            Settings.UdpPort = (int)numericUpDown_udpport.Value;
            Settings.MaxPlayer = (int)numericUpDown_maxplyer.Value;
            Settings.UdpInterval = (int)numericUpDown_udpInterval.Value;

            Settings.UseScript = checkBox_useScript.Checked;
            Settings.UserModel = checkBox_userModel.Checked;

            Settings.WorldName = textBox_serverName.Text;
            Settings.WorldDesc = textBox_serverDesc.Text;
            Settings.UseWorldPass = checkBox_worldPass.Checked;
            Settings.WorldPass = textBox_worldPass.Text;
            Settings.WorldImagePath = textBox_worldImagePath.Text;
            Settings.WorldIconPath = textBox_worldIconPath.Text;

            Settings.Culture = ((CultureInfo)comboBox_culture.SelectedItem).LCID;

            try
            {
                var json = Util.SerializeJson(Settings, false);
                File.WriteAllText(SettingsPath, json);
                LogInfo("save the settings to server.ini", true);
            }
            catch
            {
                LogWarn("failed to save the settings to server.ini", true);
            }
        }
        #endregion

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings(SettingsPath);
        }

        private void checkBox_worldPass_CheckedChanged(object sender, EventArgs e)
        {
            textBox_worldPass.Enabled = checkBox_worldPass.Checked;
            button_genPass.Enabled = checkBox_worldPass.Checked;
        }
        private void button_genPass_Click(object sender, EventArgs e)
        {
            textBox_worldPass.Text = Util.CreateBase58(8);
        }

        
        public WalkerPlayer GetPlayer(Predicate<WalkerPlayer> match)
        {
            foreach (Peer p in listBox_player.Items)
            {
                if (match(p.Player))
                {
                    return p.Player;
                }
            }
            return null;
        }

        public void AddWorldObject(NwWalkerGameObject obj)
        {
            //var player = GetPlayer(p => p.UserID == obj.UserID);
            //if (player == null) return;
            //obj.CreatorName = player.Name;
            GameObjectHashmap.Add(obj.Hash, obj);
            listBox_worldObject.Items.Add(obj);
            
        }
        public void RemoveWorldObject(string hash)
        {
            if (!GameObjectHashmap.ContainsKey(hash)) return;

            NwWalkerGameObject rmObj = GameObjectHashmap[hash];
            GameObjectHashmap.Remove(hash);
            listBox_worldObject.Items.Remove(rmObj);

            return;
        }

        public void UpdateWorldObjectStatus(string objHash, string scriptHash, byte[] status)
        {
            NwWalkerGameObject obj = null;
            if (GameObjectHashmap.TryGetValue(objHash, out obj))
            {
                var scr = Array.Find(obj.Scripts, s => s.Hash == scriptHash);
                if (scr != null) scr.Status = status;
                //if (listBox_worldObject.SelectedItem == obj && checkBox_worldObject_autoUpdate.Checked)
                //{
                //    textBox_wordObject_status.Text = status;
                //}
            }
        }

        private void listBox_worldObject_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox_worldObject.SelectedItem == null)
            {
                label_worldObject_Name.Text = "Name: ";
                label_worldObject_hash.Text = "Hash: ";
                label_worldObject_date.Text = "CreateDate: ";
                label_worldObject_pos.Text = "Position: ";
                label_worldObject_createPlayer.Text = "CreatePlayer: ";
                return;
            }
            var obj = listBox_worldObject.SelectedItem as NwWalkerGameObject;
            if (obj == null) return;

            label_worldObject_Name.Text = "Name: " + obj.Name;
            label_worldObject_hash.Text = "Hash: " + obj.Hash;
            label_worldObject_date.Text = "CreateDate: " + obj.CreateDate;
            label_worldObject_pos.Text = "Position: " + obj.Position.ToString();
            label_worldObject_createPlayer.Text = "CreatePlayer: " + obj.CreatorName;
            //textBox_wordObject_status.Text = obj.Status;
        }

        private void button_worldImagePath_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            ofd.Filter = "画像ファイル(*.jpeg;*.jpg;*.png)|*.jpeg;*.jpg;*.png|すべてのファイル(*.*)|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox_worldImagePath.Text = ofd.FileName;
            }
        }

        private void button_worldIconPath_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            ofd.Filter = "画像ファイル(*.jpeg;*.jpg;*.png)|*.jpeg;*.jpg;*.png|すべてのファイル(*.*)|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox_worldIconPath.Text = ofd.FileName;
            }
        }

        public WorldInfo CreateWorldInfo()
        {
            var info = new WorldInfo()
            {
                Culture = ((CultureInfo)comboBox_culture.SelectedItem).LCID,
                GameType = 1,
                MaxPlayer = (int)numericUpDown_maxplyer.Value,
                NowPlayer = listBox_player.Items.Count,
                Version = Version,
                WorldDesc = textBox_serverDesc.Text,
                WorldIcon = worldIconString,
                WorldImage = worldImageString,
                WorldName = textBox_serverName.Text,
                WorldPass = checkBox_worldPass.Checked,
                PublicKey = Settings.PublicKey,
            };

            return info;
        }
    }
}
