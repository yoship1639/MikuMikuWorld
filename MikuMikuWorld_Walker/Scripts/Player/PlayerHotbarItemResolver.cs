using MikuMikuWorld.Assets;
using MikuMikuWorld.Networks;
using MikuMikuWorld.Scripts.Character;
using MikuMikuWorld.Walker;
using MikuMikuWorld.Walker.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Scripts.Player
{
    class PlayerHotbarItemResolver : GameComponent
    {
        private GameObject focusedObj;
        private UserData userData;
        private PlayerRayData data = new PlayerRayData();
        private Server server;
        private WorldResources resources;

        protected override void OnLoad()
        {
            userData = MMW.GetAsset<UserData>();
            server = MMW.GetAsset<Server>();
            resources = MMW.GetAsset<WorldResources>();
        }

        private WalkerItem prevItem;
        protected override void Update(double deltaTime)
        {
            if (Input.IsButtonPressed(OpenTK.Input.MouseButton.Right))
            {
                var idx = userData.ItemSelectIndex;
                var item = userData.HotbarItems[idx];
                if (item == null) return;

                if (!resources.Objects.ContainsKey(item.Info.Hash))
                {
                    MMW.BroadcastMessage("log", "item unavailable in this world");
                }

                var scripts = new List<NwWalkerGameObject.NwScriptInfo>();
                var objs = MMW.GetAsset<WorldResources>().Objects;

                foreach (var s in objs[item.Info.Hash].nwObject.Scripts)
                {
                    scripts.Add(new NwWalkerGameObject.NwScriptInfo()
                    {
                        Hash = s.Hash,
                        Status = null,
                    });
                }

                var obj = new NwWalkerGameObject()
                {
                    CreateDate = DateTime.Now.ToString(),
                    CreatorName = userData.Name,
                    Hash = Util.CreateHash(12),
                    Name = item.Info.Name,
                    ObjectHash = item.Info.Hash,
                    Position = data.position.ToVec3f(),
                    Rotation = data.rotate.ToVec3f(),
                    Scale = new Vector3f(1.0f, 1.0f, 1.0f),
                    SessionID = userData.SessionID,
                    UserID = userData.UserID,
                    Scripts = scripts.ToArray(),
                };

                if (focusedObj != null && item.Info.Type == "Put")
                {
                    if (item.Info.Consume)
                    {
                        item.Number--;
                        if (item.Number <= 0) userData.HotbarItems[idx] = null;
                    }
                    if (item.Info.Sync)
                    {
                        server.SendTcp(DataType.RequestObjectPut, obj);
                        MMW.BroadcastMessage("log", "sync put item");
                    }
                    else if (item.Number > 0)
                    {
                        MMW.FindGameComponent<WalkerScript>().PutGameObject(obj, false);
                        MMW.BroadcastMessage("log", "put item");
                    }
                }
                else if (item.Info.Type == "Use")
                {
                    if (item.Info.Consume)
                    {
                        item.Number--;
                        if (item.Number <= 0) userData.HotbarItems[idx] = null;
                    }
                    if (item.Info.Sync)
                    {
                        server.SendTcp(DataType.RequestItemUsed, obj);
                        MMW.BroadcastMessage("log", "sync use item");
                    }
                    else if (item.Number > 0)
                    {
                        MMW.FindGameComponent<WalkerScript>().ItemUse(obj, GameObject.GetComponent<CharacterInfo>().Player, false);
                        MMW.BroadcastMessage("log", "use item");
                    }
                }
            }
            else if (Input.IsButtonDown(OpenTK.Input.MouseButton.Right))
            {
                var idx = userData.ItemSelectIndex;
                var item = userData.HotbarItems[idx];
                if (item == null) return;

                if (!resources.Objects.ContainsKey(item.Info.Hash))
                {
                    MMW.BroadcastMessage("log", "item unavailable in this world");
                }

                if (item.Info.Type == "Throw")
                {

                }
            }
            else if (Input.IsKeyPressed(OpenTK.Input.Key.Q))
            {
                var idx = userData.ItemSelectIndex;
                var item = userData.HotbarItems[idx];
                if (item == null) return;

                userData.HotbarItems[idx] = null;

            }
        }

        protected override void OnReceivedMessage(string message, params object[] args)
        {
            if (message == "focus enter")
            {
                focusedObj = (GameObject)args[0];
                data = (PlayerRayData)args[1];
            }
            else if (message == "focus leave")
            {
                focusedObj = null;
                data = (PlayerRayData)args[1];
            }
        }
    }
}
