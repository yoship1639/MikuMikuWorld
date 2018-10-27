using MikuMikuWorld.Assets;
using MikuMikuWorld.Scripts.HUD;
using MikuMikuWorld.Network;
using MikuMikuWorld.Properties;
using MikuMikuWorld.Scripts.Player;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikuMikuWorld.GameComponents;
using MikuMikuWorld.Physics;
using MikuMikuWorld.Walker.Network;

namespace MikuMikuWorld.Scripts.World
{
    class CoinSpown : GameComponent
    {
        private WorldObject cupperCoin;
        private WorldObject silverCoin;
        private WorldObject goldCoin;
        private Bounds bounds;
        private UserData userData;

        protected override void OnLoad()
        {
            var decomp = Util.Decompress(Resources.coin_cupper);
            var data = Util.DeserializeJson<NwObject>(Encoding.UTF8.GetString(decomp));
            cupperCoin = AssetConverter.FromNwObject(data);
            cupperCoin.Purchasable = false;
            cupperCoin.Hash = Util.ComputeHash(Resources.coin_cupper, 12);
            cupperCoin.Load();

            decomp = Util.Decompress(Resources.coin_silver);
            data = Util.DeserializeJson<NwObject>(Encoding.UTF8.GetString(decomp));
            silverCoin = AssetConverter.FromNwObject(data);
            silverCoin.Purchasable = false;
            silverCoin.Hash = Util.ComputeHash(Resources.coin_silver, 12);
            silverCoin.Load();

            decomp = Util.Decompress(Resources.coin_gold);
            data = Util.DeserializeJson<NwObject>(Encoding.UTF8.GetString(decomp));
            goldCoin = AssetConverter.FromNwObject(data);
            goldCoin.Purchasable = false;
            goldCoin.Hash = Util.ComputeHash(Resources.coin_gold, 12);
            goldCoin.Load();

            var wr = MMW.GetAsset<WorldResources>();
            wr.Objects.Add(cupperCoin.Hash, cupperCoin);
            wr.Objects.Add(silverCoin.Hash, silverCoin);
            wr.Objects.Add(goldCoin.Hash, goldCoin);

            var mr = GameObject.GetComponent<MeshRenderer>();
            bounds = mr.Mesh.Bounds;

            var size = bounds.Size.X * bounds.Size.Z;
            if (size > 400.0f)
            {
                maxCoin = (int)Math.Log10(size);
            }

            userData = MMW.GetAsset<UserData>();
            time = userData.CoinSpownTime;
        }

        private double time = 120.0;
        private int totalCoin = 0;
        private int maxCoin = 0;
        protected override void Update(double deltaTime)
        {
            if (time > 0.0) time -= deltaTime;

            if (time < 0.0 && totalCoin < maxCoin)
            {
                time += userData.CoinSpownTime;

                var r = Util.RandomInt(0, 100);
                var coin = cupperCoin;
                if (r >= 90) coin = silverCoin;
                else if (r == 99) coin = goldCoin;

                RayTestResult res = null;
                int test = 5;
                while (test > 0)
                {
                    var x = MMWMath.Lerp(bounds.Min.X, bounds.Max.X, Util.RandomFloat());
                    var z = MMWMath.Lerp(bounds.Min.Z, bounds.Max.Z, Util.RandomFloat());
                    var pos = new Vector4(x, 0.0f, z, 1.0f) * Transform.WorldTransform;

                    var rays = Bullet.RayTest(new Vector3(pos.X, 100.0f, pos.Z), new Vector3(pos.X, -50.0f, pos.Z));
                    if (rays.Count > 0 && rays[0].GameObject.Tags.Contains("world"))
                    {
                        res = rays[0];
                        break;
                    }

                    test--;
                }

                if (res != null)
                {
                    var go = new NwWalkerGameObject()
                    {
                        CreateDate = DateTime.Now.ToString(),
                        CreatorName = MMW.GetAsset<UserData>().UserName,
                        Hash = Util.CreateHash(12),
                        Name = "coin",
                        ObjectHash = coin.Hash,
                        Position = (res.Position + Vector3.UnitY * 0.2f).ToVec3f(),
                        Rotation = new Vector3f(),
                        Scale = new Vector3f(1, 1, 1),
                        SessionID = MMW.GetAsset<UserData>().SessionID,
                        UserID = MMW.GetAsset<UserData>().UserID,
                    };

                    MMW.FindGameComponent<WalkerScript>().PutGameObject(go, false);
                    totalCoin++;
                } 
            }
        }

        protected override void OnReceivedMessage(string message, params object[] args)
        {
            if (message == "get coin")
            {
                if (totalCoin > 0) totalCoin--;
            }
        }
    }
}
