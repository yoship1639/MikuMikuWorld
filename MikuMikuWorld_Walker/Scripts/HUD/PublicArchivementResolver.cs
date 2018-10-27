using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Scripts.HUD
{
    class PublicAchivementResolver : GameComponent
    {
        private UserData userData;
        private List<Func<bool>> checkList = new List<Func<bool>>();

        protected override void OnLoad()
        {
            userData = MMW.GetAsset<UserData>();

            if (!userData.Achivements.Exists(ach => ach.Name == "Walk 1km"))
            {
                checkList.Add(() =>
                {
                    if (userData.TotalMoveDistance >= 1000.0f)
                    {
                        MMW.BroadcastMessage("get reward", Reward.CreatePublicReward(10, 30, "Walk 1km", "1Km歩いた"));
                        return true;
                    }
                    return false;
                });
            }
            if (!userData.Achivements.Exists(ach => ach.Name == "Walk 5km"))
            {
                checkList.Add(() =>
                {
                    if (userData.TotalMoveDistance >= 5000.0f)
                    {
                        MMW.BroadcastMessage("get reward", Reward.CreatePublicReward(30, 100, "Walk 5km", "5Km歩いた"));
                        return true;
                    }
                    return false;
                });
            }
            if (!userData.Achivements.Exists(ach => ach.Name == "Walk 10km"))
            {
                checkList.Add(() =>
                {
                    if (userData.TotalMoveDistance >= 10000.0f)
                    {
                        MMW.BroadcastMessage("get reward", Reward.CreatePublicReward(100, 300, "Walk 10km", "10Km歩いた"));
                        return true;
                    }
                    return false;
                });
            }
            if (!userData.Achivements.Exists(ach => ach.Name == "Walk 100km"))
            {
                checkList.Add(() =>
                {
                    if (userData.TotalMoveDistance >= 100000.0f)
                    {
                        MMW.BroadcastMessage("get reward", Reward.CreatePublicReward(300, 1000, "Walk 100km", "100Km歩いた"));
                        return true;
                    }
                    return false;
                });
            }
            if (!userData.Achivements.Exists(ach => ach.Name == "Walk 1000km"))
            {
                checkList.Add(() =>
                {
                    if (userData.TotalMoveDistance >= 1000000.0f)
                    {
                        MMW.BroadcastMessage("get reward", Reward.CreatePublicReward(1000, 4000, "Walk 1000km", "1000Km歩いた"));
                        return true;
                    }
                    return false;
                });
            }

            if (!userData.Achivements.Exists(ach => ach.Name == "Jump 100"))
            {
                checkList.Add(() =>
                {
                    if (userData.TotalJumpCount >= 100)
                    {
                        MMW.BroadcastMessage("get reward", Reward.CreatePublicReward(10, 30, "Jump 100", "100回ジャンプした"));
                        return true;
                    }
                    return false;
                });
            }
            if (!userData.Achivements.Exists(ach => ach.Name == "Jump 1000"))
            {
                checkList.Add(() =>
                {
                    if (userData.TotalJumpCount >= 1000)
                    {
                        MMW.BroadcastMessage("get reward", Reward.CreatePublicReward(100, 300, "Jump 1000", "1000回ジャンプした"));
                        return true;
                    }
                    return false;
                });
            }
            if (!userData.Achivements.Exists(ach => ach.Name == "Jump 10000"))
            {
                checkList.Add(() =>
                {
                    if (userData.TotalJumpCount >= 10000)
                    {
                        MMW.BroadcastMessage("get reward", Reward.CreatePublicReward(300, 1000, "Jump 10000", "10000回ジャンプした"));
                        return true;
                    }
                    return false;
                });
            }
            if (!userData.Achivements.Exists(ach => ach.Name == "Jump 100000"))
            {
                checkList.Add(() =>
                {
                    if (userData.TotalJumpCount >= 100000)
                    {
                        MMW.BroadcastMessage("get reward", Reward.CreatePublicReward(1000, 4000, "Jump 100000", "100000回ジャンプした"));
                        return true;
                    }
                    return false;
                });
            }

            if (!userData.Achivements.Exists(ach => ach.Name == "Playing 1 hour"))
            {
                checkList.Add(() =>
                {
                    if (userData.TotalPlayingTime >= 3600.0)
                    {
                        MMW.BroadcastMessage("get reward", Reward.CreatePublicReward(10, 30, "Playing 1 hour", "1時間プレイした"));
                        return true;
                    }
                    return false;
                });
            }
            if (!userData.Achivements.Exists(ach => ach.Name == "Playing 10 hour"))
            {
                checkList.Add(() =>
                {
                    if (userData.TotalPlayingTime >= 36000.0)
                    {
                        MMW.BroadcastMessage("get reward", Reward.CreatePublicReward(30, 100, "Playing 10 hour", "10時間プレイした"));
                        return true;
                    }
                    return false;
                });
            }
            if (!userData.Achivements.Exists(ach => ach.Name == "Playing 100 hour"))
            {
                checkList.Add(() =>
                {
                    if (userData.TotalPlayingTime >= 360000.0)
                    {
                        MMW.BroadcastMessage("get reward", Reward.CreatePublicReward(300, 1000, "Playing 100 hour", "100時間プレイした"));
                        return true;
                    }
                    return false;
                });
            }
            if (!userData.Achivements.Exists(ach => ach.Name == "Playing 1000 hour"))
            {
                checkList.Add(() =>
                {
                    if (userData.TotalPlayingTime >= 3600000.0)
                    {
                        MMW.BroadcastMessage("get reward", Reward.CreatePublicReward(1000, 4000, "Playing 1000 hour", "1000時間プレイした"));
                        return true;
                    }
                    return false;
                });
            }

            if (!userData.Achivements.Exists(ach => ach.Name == "Get 100 coin"))
            {
                checkList.Add(() =>
                {
                    if (userData.TotalGetCoin >= 100)
                    {
                        MMW.BroadcastMessage("get reward", Reward.CreatePublicReward(10, 30, "Get 100 coin", "合計100コインゲットした"));
                        return true;
                    }
                    return false;
                });
            }
            if (!userData.Achivements.Exists(ach => ach.Name == "Get 1000 coin"))
            {
                checkList.Add(() =>
                {
                    if (userData.TotalGetCoin >= 1000)
                    {
                        MMW.BroadcastMessage("get reward", Reward.CreatePublicReward(30, 100, "Get 1000 coin", "合計1000コインゲットした"));
                        return true;
                    }
                    return false;
                });
            }
            if (!userData.Achivements.Exists(ach => ach.Name == "Get 10000 coin"))
            {
                checkList.Add(() =>
                {
                    if (userData.TotalGetCoin >= 10000)
                    {
                        MMW.BroadcastMessage("get reward", Reward.CreatePublicReward(300, 1000, "Get 10000 coin", "合計10000コインゲットした"));
                        return true;
                    }
                    return false;
                });
            }
            if (!userData.Achivements.Exists(ach => ach.Name == "Get 100000 coin"))
            {
                checkList.Add(() =>
                {
                    if (userData.TotalGetCoin >= 100000)
                    {
                        MMW.BroadcastMessage("get reward", Reward.CreatePublicReward(1000, 4000, "Get 100000 coin", "合計100000コインゲットした"));
                        return true;
                    }
                    return false;
                });
            }
        }

        protected override void Update(double deltaTime)
        {
            foreach (var ch in checkList.ToArray())
            {
                if (ch()) checkList.Remove(ch);
            }
        }

        protected override void OnReceivedMessage(string message, params object[] args)
        {
            if (message == "get reward")
            {
                var rew = (Reward)args[0];
                if (!rew.Verify()) return;

                if (rew.Coin > 0) userData.AddCoin(rew.Coin);
                if (rew.Exp > 0) userData.AddExp(rew.Exp);

                if (rew.Achivement != null && rew.Achivement.Verify() && !userData.Achivements.Exists(ach => ach.Name == rew.Achivement.Name))
                {
                    userData.Achivements.Add(rew.Achivement);
                    MMW.BroadcastMessage("log", $"Archived [{rew.Achivement.Name}]");
                }
            }
        }
    }
}
