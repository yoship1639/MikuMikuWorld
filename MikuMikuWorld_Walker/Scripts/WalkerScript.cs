using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikuMikuWorld.Assets;
using MikuMikuWorld.GameComponents;
using MikuMikuWorld.GameComponents.ImageEffects;
using MikuMikuWorld.Scripts.HUD;
using MikuMikuWorld.Network;
using MikuMikuWorld.Networks;
using MikuMikuWorld.Networks.Commands;
using MikuMikuWorld.Scripts.Player;
using MikuMikuWorld.Walker;
using MikuMikuWorld.Walker.Network;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Graphics;
using MikuMikuWorldScript;
using OpenTK;
using MikuMikuWorld.Scripts.World;
using MikuMikuWorld.Scripts.Character;
using MikuMikuWorld.TestScripts;

namespace MikuMikuWorld.Scripts
{
    class WalkerScript : DrawableGameComponent
    {
        public override int[] TcpAcceptDataTypes => new int[] 
        {
            Walker.DataType.ResponseObjectDestroy,
            Walker.DataType.ResponseItemUsed,
            Walker.DataType.ResponseObjectPut,
            Walker.DataType.ResponseWorldStatus,
            Walker.DataType.ResponsePlayerJoin,
            Walker.DataType.ResponseLeaveWorld,
        };

        public bool AllowFunctionKey { get; set; } = true;

        private NwWorldData nwWorldData;
        private UserData userData;
        private WorldData worldData;
        private Server server;
        private Blur blur;

        public WorldResources Resources { get; private set; }
        public List<GameObject> Players { get; private set; } = new List<GameObject>();
        public List<GameObject> SyncWorldObjects { get; private set; } = new List<GameObject>();
        public List<GameObject> WorldObjects { get; private set; } = new List<GameObject>();

        private GameObject hudGO;
        private GameObject playerGO;
        private GameObject worldGO;
        private GameObject cameraTarget;

        public WalkerScript(NwWorldData data)
        {
            nwWorldData = data;
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            server = MMW.GetAsset<Server>();

            userData = MMW.GetAsset<UserData>();
            //userData.ArchiveIndex = 6;
            //userData.UserID = "MikuMikuWorldOrg0001";

            MMW.RegistAsset(worldData = new WorldData());

            Resources = new WorldResources(nwWorldData);
            MMW.RegistAsset(Resources);

            // アイテムの整理

            var world = Resources.Worlds.First().Value;
            world.Load();

            var ch = Resources.Characters.First().Value;
            ch.Load();

            MMW.Window.CursorVisible = false;

            worldGO = GameObjectFactory.CreateWorld(world, "world", "Deferred Physical");
            worldGO.AddComponent<CoinSpown>();
            MMW.RegistGameObject(worldGO);

            playerGO = GameObjectFactory.CreatePlayer(ch, "player", ch.Bones != null ? "Deferred Physical Skin" : "Deferred Physical");
            playerGO.Properties["sessionID"] = server.SessionID;
            playerGO.Properties["userID"] = userData.UserID;
            ch.EyePosition.Z = 0.1f;
            var rb = playerGO.GetComponent<RigidBody>();
            rb.Collide += Rb_Collide;
            rb.Collide += (s, e) =>
            {
                if (e.GameObject.Tags.Contains("coin") && e.GameObject.Tags.Contains("98df1d6abbc7"))
                {
                    e.GameObject.Tags.Remove("coin");
                    e.GameObject.Tags.Remove("98df1d6abbc7");
                    MMW.Invoke(() =>
                    {
                        MMW.DestroyGameObject(e.GameObject);
                        if (e.GameObject.Tags.Contains("cupper coin")) userData.AddCoin(1);
                        else if (e.GameObject.Tags.Contains("silver coin")) userData.AddCoin(5);
                        else if (e.GameObject.Tags.Contains("gold coin")) userData.AddCoin(20);
                    });
                }
            };
            MMW.RegistGameObject(playerGO);
            Players.Add(playerGO);

            cameraTarget = new GameObject("camera target");
            cameraTarget.Transform.Parent = playerGO.Transform;
            cameraTarget.Transform.Position = new Vector3(0.0f, ch.EyePosition.Y, ch.EyePosition.Z);
            MMW.RegistGameObject(cameraTarget);

            var wp = new WalkerPlayer()
            {
                Name = userData.UserName,
                SessionID = server.SessionID,
                ArchiveIndex = userData.ArchiveIndex,
                CharacterHash = Resources.Characters.First().Key,
                Achivements = userData.Achivements.ToList(),
                Rank = userData.Rank,
                LikesCount = userData.Likes.Count,
                LikedCount = userData.ReceiveLikes.Count,
                UserID = userData.UserID,
                Comment = userData.Comment,
                IsAdmin = false,
            };
            worldData.Players.Add(wp);

            playerGO.AddComponent<CharacterInfo>(ch, wp);
            playerGO.AddComponent<PlayerMoveController>();
            var cam = playerGO.AddComponent<PlayerCameraController>();
            cam.Target = cameraTarget.Transform;
            playerGO.AddComponent<PlayerRayResolver>();
            playerGO.AddComponent<PlayerHotbarItemResolver>();
            playerGO.AddComponent<UdpSender>();
            playerGO.AddComponent<CharacterPopupResolver>();
            playerGO.AddComponent<CharacterNameResolver>();
            playerGO.AddComponent<DoFResolver>();
            playerGO.AddComponent<WalkerGameObjectScript>(playerGO, new BigHead(), null);

            var env = nwWorldData.Worlds[0].Environments[0];
            MMW.GlobalAmbient = env.Ambient.FromColor4f();
            MMW.DirectionalLight.Intensity = env.DirLight.Intensity;
            MMW.DirectionalLight.Color = env.DirLight.Color.FromColor4f();
            MMW.DirectionalLight.Direction = env.DirLight.Direction.FromVec3f().Normalized();

            MMW.MainCamera.ShadowMapping = true;
            MMW.MainCamera.GameObject.AddComponent<AudioListener>();
            var effs = MMW.MainCamera.GameObject.GetComponents<ImageEffect>();
            foreach (var eff in effs) eff.Enabled = true;

            blur = MMW.MainCamera.GameObject.AddComponent<Blur>();
            blur.Radius = 0.0f;

            hudGO = new GameObject("hud", "system", "hud");
            //go.Layer = GameObject.LayerUI + 1;
            hudGO.AddComponent<HotBar>();
            hudGO.AddComponent<Aiming>();
            hudGO.AddComponent<CoinResolver>();
            hudGO.AddComponent<ScriptResolver>();
            hudGO.AddComponent<LogResolver>();
            hudGO.AddComponent<ChatResolver>();
            hudGO.AddComponent<PictureChatResolver>();
            hudGO.AddComponent<MenuResolver>();
            hudGO.AddComponent<PublicAchivementResolver>();
            hudGO.AddComponent<LeaveResolver>();
            hudGO.AddComponent<InventiryResolver>();
            MMW.RegistGameObject(hudGO);

            server.BeforeCmds.Add(new CmdAllTransform(this));

            /*
            foreach (var g in nwWorldData.GameObjects)
            {
                try
                {
                    var wo = Resources.Objects[g.ObjectHash];
                    if (wo.ItemType == "Put") PutGameObject(g, true);
                }
                catch { }
            }
            */

            userData.SessionID = server.SessionID;
            userData.CharacterHash = Resources.Characters.First().Key;

            server.Disconnected += Server_Disconnected;
            server.SendTcp(Walker.DataType.ResponseReady, Util.SerializeJson(wp));
        }

        

        #region Network
        #endregion

        private void Rb_Collide(object sender, Collision e)
        {
            var rb = (RigidBody)sender;
            var v = e.TotalExtrusion;
            if (v.Length == 0.0f) return;
            var n = v.Normalized();
            if (Math.Abs(n.Y) > 0.5f) return;
            v.Y = 0.0f;
            //rb.ApplyImpulse(v * rb.Mass * 0.5f);
            rb.GameObject.GetComponent<PlayerMoveController>().Velocity *= (1.0f - v.Length * 0.5f);

            if (e.GameObject.Tags.Contains("ball"))
            {
                e.GameObject.GetComponent<RigidBody>().ApplyImpulse(-v * 10.0f, Vector3.UnitY * 0.1f);
            }
        }

        private bool hudShown = true;
        private bool fps = false;
        private bool menuShown = false;
        private bool inventoryShown = false;
        private double checkTime = 3.0;
        protected override void Update(double deltaTime)
        {
            userData.TotalPlayingTime += deltaTime;

            if (AllowFunctionKey && Input.IsKeyPressed(Key.F2))
            {
                MMW.MainCamera.TargetTexture.ColorDst0.Save(GameData.AppDir + "pic_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png", System.Drawing.Imaging.ImageFormat.Png);
            }

            if (AllowFunctionKey && Input.IsKeyPressed(Key.F3))
            {
                MMW.Window.CursorVisible = !MMW.Window.CursorVisible;
            }

            if (AllowFunctionKey && Input.IsAnyKeyPressed(Key.F4))
            {
                hudShown = !hudShown;
                if (hudShown) MMW.BroadcastMessage("hud show");
                else MMW.BroadcastMessage("hud hide");
            }

            if (AllowFunctionKey && Input.IsKeyPressed(Key.F5))
            {
                fps = !fps;
                if (fps) MMW.BroadcastMessage("camera change", "first person");
                else MMW.BroadcastMessage("camera change", "third person");
            }

            if (AllowFunctionKey && Input.IsKeyPressed(Key.F6))
            {
                MMW.BroadcastMessage("change chara display");
            }

            if (!menuShown && AllowFunctionKey && Input.IsKeyPressed(Key.E))
            {
                inventoryShown = !inventoryShown;
                if (inventoryShown)
                {
                    MMW.BroadcastMessage("show dialog");
                    MMW.BroadcastMessage("show inventory");
                }
                else
                {
                    MMW.BroadcastMessage("close dialog");
                    MMW.BroadcastMessage("close inventory");
                }
            }

            if (!inventoryShown && AllowFunctionKey && Input.IsKeyPressed(Key.T))
            {
                menuShown = !menuShown;
                if (menuShown)
                {
                    MMW.BroadcastMessage("show dialog");
                    MMW.BroadcastMessage("show menu");
                }
                else
                {
                    MMW.BroadcastMessage("close dialog");
                    MMW.BroadcastMessage("close menu");
                } 
            }

            checkTime -= deltaTime;
            if (checkTime < 0)
            {
                checkTime += 10.0;
                userData.Save();
                server.SendTcp(Walker.DataType.RequestWorldStatus, new byte[0]);
            }
        }

        protected override void Draw(double deltaTime, Camera camera) { }

        public override void OnTcpReceived(int dataType, byte[] data)
        {
            if (dataType == Walker.DataType.ResponseObjectDestroy)
            {
                string hash = null;
                Buffer.Read(data, br => hash = br.ReadString());
                RemoveGameObject(SyncWorldObjects.Find(go => go.Hash == hash));
            }
            else if (dataType == Walker.DataType.ResponseItemUsed)
            {
                var obj = Util.DeserializeJsonBinary<NwWalkerGameObject>(data);
                ItemUse(obj, worldData.Players.Find(p => p.UserID == obj.UserID), true);
            }
            else if (dataType == Walker.DataType.ResponseObjectPut)
            {
                var obj = Util.DeserializeJsonBinary<NwWalkerGameObject>(data);
                PutGameObject(obj, true);
            }
            else if (dataType == Walker.DataType.ResponsePlayerJoin)
            {
                var player = Util.DeserializeJsonBinary<WalkerPlayer>(data);

                if (worldData.Players.Exists(p => p.SessionID == player.SessionID)) return;
                JoinPlayer(player);
            }
            else if (dataType == Walker.DataType.ResponseWorldStatus)
            {
                var status = Util.DeserializeJsonBinary<NwWorldStatus>(data);
                ResolveReceiveWorldStatus(status);
            }
            else if (dataType == Walker.DataType.ResponseLeaveWorld)
            {
                server.Disconnected -= Server_Disconnected;
                OnUnload();
            }
        }

        private void JoinPlayer(WalkerPlayer player)
        {
            MMW.Invoke(() =>
            {
                try
                {
                    var c = Resources.Characters[player.CharacterHash];
                    if (!c.Loaded) c.Load();

                    var ch = GameObjectFactory.CreateCharacter(c, player.Name, c.Bones != null ? "Deferred Physical Skin" : "Deferred Physical");
                    ch.Properties["sessionID"] = player.SessionID;
                    ch.Properties["userID"] =  player.UserID;
                    ch.Properties["user"] = player;
                    ch.AddComponent<CharacterInfo>(c, player);
                    ch.AddComponent<CharacterTransformResolver>(player);
                    ch.AddComponent<CharacterPopupResolver>();
                    ch.AddComponent<CharacterNameResolver>();
                    ch.Transform.Position = player.Position.FromVec3f();
                    ch.Transform.Rotate = player.Rotation.FromVec3f();
                    ch.Transform.UpdatePhysicalTransform();

                    MMW.RegistGameObject(ch);
                    Players.Add(ch);
                    worldData.Players.Add(player);

                    MMW.BroadcastMessage("log", $"player \"{player.Name}\" joined");
                }
                catch (Exception ex)
                {
                    MMW.BroadcastMessage("log", $"[ERROR] player \"{player.Name}\" join failed");
                }
            });
        }
        private void LeavePlayer(WalkerPlayer player)
        {
            MMW.Invoke(() =>
            {
                try
                {
                    Players.Remove(Players.Find(p => (int)p.Properties["sessionID"] == player.SessionID));
                    worldData.Players.Remove(player);

                    MMW.BroadcastMessage("log", $"player \"{player.Name}\" left");
                }
                catch (Exception ex)
                {
                    MMW.BroadcastMessage("log", $"[ERROR] player \"{player.Name}\" leave failed");
                }
            });
        }

        public void PutGameObject(NwWalkerGameObject go, bool sync)
        {
            MMW.Invoke(() =>
            {
                try
                {
                    var wo = Resources.Objects[go.ObjectHash];
                    if (!wo.Loaded) wo.Load();

                    var g = GameObjectFactory.CreateGameObject(go, wo, wo.Name, wo.Bones != null ? "Deferred Physical Skin" : "Deferred Physical");
                    g.Hash = go.Hash;
                    g.Transform.Position = go.Position.FromVec3f();
                    g.Transform.Rotate = go.Rotation.FromVec3f();
                    if (go.Scale != null) g.Transform.Scale = go.Scale.FromVec3f();
                    g.Transform.UpdatePhysicalTransform();

                    MMW.RegistGameObject(g);
                    if (sync) SyncWorldObjects.Add(g);
                    else WorldObjects.Add(g);
                }
                catch
                {
                    MMW.BroadcastMessage("log", $"[ERROR] object \"{go.Name}\" entry failed");
                }
            });
        }
        public void ItemUse(NwWalkerGameObject go, WalkerPlayer player, bool sync)
        {
            MMW.Invoke(() =>
            {
                try
                {
                    if (!Players.Exists(p => (string)p.Properties["userID"] == player.UserID)) return;

                    var wo = Resources.Objects[go.ObjectHash];
                    if (!wo.Loaded) wo.Load();

                    var g = GameObjectFactory.CreateItem(go, wo, player, "item", wo.Bones != null ? "Deferred Physical Skin" : "Deferred Physical");
                    
                    g.Properties["user"] = player;
                    g.Properties["userID"] = player.UserID;
                    g.Properties["sessionID"] = player.SessionID;
                    g.Hash = go.Hash;
                    g.Transform.Parent = Players.Find(p => (string)p.Properties["userID"] == player.UserID).Transform;
                    g.Transform.UpdatePhysicalTransform();

                    MMW.RegistGameObject(g);
                    if (sync) SyncWorldObjects.Add(g);
                    else WorldObjects.Add(g);
                }
                catch
                {
                    MMW.BroadcastMessage("log", $"[ERROR] failed to use item \"{go.Name}\"");
                }
            });
        }
        public void RemoveGameObject(GameObject go)
        {
            MMW.Invoke(() =>
            {
                try
                {
                    SyncWorldObjects.Remove(go);
                    WorldObjects.Remove(go);
                    if (go != null) MMW.DestroyGameObject(go);

                    MMW.BroadcastMessage("log", $"object \"{go.Name}\" removed");
                }
                catch
                {

                }
            });
        }

        private void ResolveReceiveWorldStatus(NwWorldStatus status)
        {
            for (var i = 0; i < status.Players.Length; i++)
            {
                var p = status.Players[i];
                WalkerPlayer player;
                if ((player = worldData.Players.Find(pl => pl.SessionID == p.SessionID)) != null)
                {
                    player.Update(p);
                }
                else
                {
                    JoinPlayer(p);
                }
            }
            foreach (var p in worldData.Players)
            {
                WalkerPlayer player;
                if ((player = Array.Find(status.Players, pl => pl.SessionID == p.SessionID)) == null)
                {
                    LeavePlayer(player);
                }
            }

            foreach (var o in status.WorldObjects)
            {
                if (SyncWorldObjects.Exists(obj => obj.Hash == o.Hash)) continue;

                try
                {
                    var wo = Resources.Objects[o.ObjectHash];

                    if (wo.ItemType == "Put") PutGameObject(o, true);
                    else if (wo.ItemType == "Use") ItemUse(o, Array.Find(status.Players, pl => pl.UserID == o.UserID), true);
                }
                catch { }
            }

            foreach (var obj in SyncWorldObjects)
            {
                NwWalkerGameObject wo;
                if ((wo = Array.Find(status.WorldObjects, o => o.Hash == obj.Hash)) != null) continue;

                RemoveGameObject(obj);
            }
        }

        private bool leave = false;
        private void Leave()
        {
            leave = true;
            server.SendTcp(Walker.DataType.RequestLeaveWorld, new byte[0]);
        }

        private void Server_Disconnected(object sender, EventArgs e)
        {
            if (!leave)
            {
                if (inventoryShown) MMW.BroadcastMessage("close inventory");
                if (menuShown) MMW.BroadcastMessage("close menu");
                MMW.BroadcastMessage("show dialog");
                MMW.BroadcastMessage("show leave window");
            }
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            foreach (var o in SyncWorldObjects.ToArray())
            {
                MMW.DestroyGameObject(o);
            }
            SyncWorldObjects.Clear();

            foreach (var o in WorldObjects.ToArray())
            {
                MMW.DestroyGameObject(o);
            }
            WorldObjects.Clear();

            foreach (var p in Players.ToArray())
            {
                MMW.DestroyGameObject(p);
            }
            Players.Clear();

            MMW.DestroyGameObject(worldGO);
            MMW.DestroyGameObject(hudGO);
            MMW.DestroyGameObject(cameraTarget);

            MMW.DestroyAsset(worldData);
            MMW.DestroyAsset(Resources);
            MMW.DestroyAsset(server);

            MMW.MainCamera.GameObject.RemoveComponent(blur);

            var effs = MMW.MainCamera.GameObject.GetComponents<ImageEffect>();
            foreach (var eff in effs) eff.Enabled = false;

            GC.Collect();
        }
    }
}
