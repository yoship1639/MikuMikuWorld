using MikuMikuWorld.Assets;
using MikuMikuWorld.Walker;
using MikuMikuWorldScript;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    class UserData : Assets.IAsset
    {
        public bool Loaded { get { return true; } }
        public string Name { get; set; }
        public Result Load() { return Result.Success; }
        public Result Unload() { return Result.Success; }

        public string UserName;
        public string Comment;
        public string UserID;
        public int ArchiveIndex = 0;
        public int SessionID;
        public string CharacterHash;

        public List<WorldInfo> SignupServers = new List<WorldInfo>();
        public List<Achivement> Achivements = new List<Achivement>();
        public List<PictChatData> PictChats = new List<PictChatData>();

        public bool AddItem(WalkerItemInfo info, byte[] status)
        {
            for (var i = 0; i < MaxHotbatItemCount; i++)
            {
                if (HotbarItems[i] == null)
                {
                    HotbarItems[i] = new WalkerItem()
                    {
                        Info = info,
                        Number = 1,
                        Status = status,
                    };
                    return true;
                }
                else if (HotbarItems[i].Info.Hash == info.Hash && HotbarItems[i].Number < HotbarItems[i].Info.MaxStack)
                {
                    HotbarItems[i].Number++;
                    return true;
                }
            }

            for (var i = 0; i < MaxItemCount; i++)
            {
                if (Items[i] == null)
                {
                    Items[i] = new WalkerItem()
                    {
                        Info = info,
                        Number = 1,
                        Status = status,
                    };
                    return true;
                }
                else if (Items[i].Info.Hash == info.Hash && Items[i].Number < Items[i].Info.MaxStack)
                {
                    HotbarItems[i].Number++;
                    return true;
                }
            }

            return false;
        }
        public WalkerItem[] HotbarItems = new WalkerItem[9];
        public WalkerItem[] Items = new WalkerItem[100];
        public int MaxHotbatItemCount = 5;
        public int MaxItemCount = 50;
        public int ItemSelectIndex = 0;

        public void AddExp(long value)
        {
            var prev = Rank;
            Exp += value;
            var up = 0;
            while (Exp >= Tables.NextRankExp(Rank))
            {
                Rank++;
                SkillPoint++;
                up++;
            }

            MMW.BroadcastMessage("add experience", value);

            if (up > 0)
            {
                MMW.BroadcastMessage("rank up", prev, Rank);
            }
        }
        public long Exp = 0;
        public long Rank = 1;
        public int SkillPoint = 0;

        public void AddCoin(long value)
        {
            Coin += value;
            if (Coin > MaxCoin) Coin = MaxCoin;
            TotalGetCoin += value;

            MMW.BroadcastMessage("get coin", value);
        }
        public void SubCoin(long value)
        {
            Coin -= value;
            if (Coin < 0) Coin = 0;
            TotalUseCoin += value;
        }
        public long MaxCoin = 1000;
        public double CoinSpownTime = 120.0;
        public long Coin = 0;
        public long TotalGetCoin = 0;
        public long TotalUseCoin = 0;

        public List<Like> Likes = new List<Like>();
        public List<Like> ReceiveLikes = new List<Like>();

        public int TotalChatCount = 0;
        public int TotalJumpCount = 0;
        public float TotalMoveDistance = 0.0f;
        public double TotalPlayingTime = 0.0;

        public bool Save()
        {
            for (var i = 0; i < 3; i++)
            {
                var dd = "d0" + i;

                try
                {
                    var p = "fa3daa8aa37e1461a1b5ddf959d28f3b" + dd;
                    for (var c = 0; c < 8; c++) p = Util.ComputeHash(Encoding.UTF8.GetBytes(p), 17 + c);
                    var j = Util.SerializeJson(this, false);
                    var ec = Encrypter.Encrypt(j, p);
                    File.WriteAllText(GameData.AppDir + dd, ec);
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

    }

    class PictChatData
    {
        public string Name { get; set; }
        public Color[,] Data;
    }
}
