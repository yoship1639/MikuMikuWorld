using MikuMikuWorld.Assets;
using MikuMikuWorld.Assets.Shaders;
using MikuMikuWorld.GameComponents;
using MikuMikuWorld.GameComponents.Coliders;
using MikuMikuWorld.GameComponents.ImageEffects;
using MikuMikuWorld.GameComponents.Lights;
using MikuMikuWorld.Importers;
using MikuMikuWorld.Properties;
using MikuMikuWorld.Scripts;
using MikuMikuWorld.Scripts.Title;
using MikuMikuWorld.Walker;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MikuMikuWorld
{
    class Game : MMWGameWindow
    {
        public Game() : 
            base(1280, 720, "Walker")
        {
            MMW.Configuration.Antialias = MMWConfiguration.AntialiasType.FXAA;
            MMW.Configuration.ShadowQuality = MMWConfiguration.ShadowQualityType.High;
            MMW.Configuration.AO = MMWConfiguration.AOType.SSAO;
            //MMW.Configuration.Shader = MMWConfiguration.ShaderType.Physical;
            MMW.Configuration.IBLQuality = MMWConfiguration.IBLQualityType.Default;
            MMW.Configuration.Bloom = MMWConfiguration.BloomType.Bloom;
            MMW.Configuration.MotionBlur = MMWConfiguration.MotionBlurType.MotionBlur;
            MMW.Configuration.DoF = MMWConfiguration.DoFType.DoF;
            MMW.Configuration.ToneMapping = true;
            MMW.Configuration.DrawEdge = false;

            // Oculusが接続済み
            if (MMW.HMDCamera.Connected)
            {
                WindowState = WindowState.Fullscreen;
                //ClientSize = new Size(MMW.Rift.VResolution, MMW.Rift.HResolution);
                ClientSize = new Size(MMW.HMDCamera.DisplayDevice.Height, 1280);
            }
            else
            {
                //WindowState = WindowState.Fullscreen;
                //ClientSize = new Size(1366, 768);
                ClientSize = new Size(1366, 768);
                WindowBorder = WindowBorder.Fixed;
                CursorVisible = false;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            MMW.GetAsset<UserData>().Save();
            base.OnClosing(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var gd = new GameData()
            {
                Shader = "Deferred Physical",
                SkinShader = "Deferred Physical Skin",
            };
            MMW.RegistAsset(gd);

            var ds = new List<UserData>();
            var hs = new List<string>();
            for (var i = 0; i < 3; i++)
            {
                var str = "d0" + i;
                UserData d = null;
                try
                {
                    var t = 2;
                    var x = 3;
                    var s = 0;
                    while (x > 1)
                    {
                        t *= 2;
                        x--;
                        s = t;
                    }
                    var p = "fa3daa8aa37e1461a1b5ddf959d28f3b" + str;
                    for (var c = 0; c < s; c++) p = Util.ComputeHash(Encoding.UTF8.GetBytes(p), 17 + c);
                    var j = Encrypter.Decrypt(File.ReadAllText(GameData.AppDir + str), p);
                    d = Util.DeserializeJson<UserData>(j, false);
                }
                catch { }

                ds.Add(d);

                if (ds.Last() == null) hs.Add(null);
                else
                {
                    var b = Util.SerializeBson(ds.Last());
                    hs.Add(Util.ComputeHash(b, 8));
                }
            }
            var counts = new int[3];
            for (var i = 0; i < 3; i++) counts[i] = hs.Where(h => h == hs[i]).Count();
            var idx = Array.IndexOf(counts, counts.Max());
            if (ds[idx] != null)
            {
                MMW.RegistAsset(ds[idx]);
                foreach (var ach in ds[idx].Achivements.ToArray())
                {
                    if (!ach.Verify()) ds[idx].Achivements.Remove(ach);
                }
            }
            else
            {
                var data = new UserData() { UserID = Util.CreateBase58(20) };
                var rew = Reward.CreatePublicReward(0, 0, 0, "Beginner", "Have a nice trip!");
                data.Achivements.Add(rew.Achivement);
                MMW.RegistAsset(data);
            } 

            {
                var userData = MMW.GetAsset<UserData>();
                var p = "fa3daa8aa37e1461a1b5ddf959d28f3b";
                for (var c = 0; c < 8; c++) p = Util.ComputeHash(Encoding.UTF8.GetBytes(p), 17 + c);

                foreach (var f in Directory.EnumerateFiles(GameData.AppDir, "*.mwr"))
                {
                    try
                    {
                        var data = File.ReadAllText(f);
                        var str = Encrypter.Decrypt(data, p);
                        var rew = Util.DeserializeJson<Reward>(str, false);

                        if (userData.Achivements.Exists(a => a.Name == rew.Achivement.Name)) continue;
                        if (!(rew.Verify() && rew.Achivement.Verify()) || rew.Achivement.PublicKey != Achivement.PublicPub) continue;

                        userData.Achivements.Add(rew.Achivement);
                        userData.AddCoin(rew.Coin);
                        userData.Exp += rew.Exp;
                    }
                    catch { }
                }



                /*
                var r1 = Reward.CreatePublicReward(50, 0, 10, "Little Donor", "Thank you for Donation!");
                var r2 = Reward.CreatePublicReward(100, 0, 10, "Common Donor", "Thank you for Donation!");
                var r3 = Reward.CreatePublicReward(150, 0, 10, "Big Donor", "Thank you for Donation!");
                var r4 = Reward.CreatePublicReward(200, 0, 10, "Super Donor", "Thank you for Donation!");
                var r5 = Reward.CreatePublicReward(300, 0, 10, "The Walker", "Messiah of MMW");

                File.WriteAllText(GameData.AppDir + "Little Donor.mwr", Encrypter.Encrypt(Util.SerializeJson(r1, false), p));
                File.WriteAllText(GameData.AppDir + "Common Donor.mwr", Encrypter.Encrypt(Util.SerializeJson(r2, false), p));
                File.WriteAllText(GameData.AppDir + "Big Donor.mwr", Encrypter.Encrypt(Util.SerializeJson(r3, false), p));
                File.WriteAllText(GameData.AppDir + "Super Donor.mwr", Encrypter.Encrypt(Util.SerializeJson(r4, false), p));
                File.WriteAllText(GameData.AppDir + "The Walker.mwr", Encrypter.Encrypt(Util.SerializeJson(r5, false), p));
                */
            }

            MMW.RegistAsset(new Sound(Resources.coin2, "WAV") { Name = "coin" });
            MMW.RegistAsset(new Sound(Resources.select2, "WAV") { Name = "select" });
            MMW.RegistAsset(new Sound(Resources.click7, "WAV") { Name = "click" });
            MMW.RegistAsset(new Sound(Resources.back3, "WAV") { Name = "back" });
            MMW.RegistAsset(new Sound(Resources.button, "WAV") { Name = "button" });

            var effs = MMW.MainCamera.GameObject.GetComponents<ImageEffect>();
            foreach (var eff in effs) eff.Enabled = false;

            var hw = new GameObject("Hello World");
            hw.AddComponent<HelloWorld>();
            MMW.RegistGameObject(hw);

            var load = new GameObject("Init Loading", Matrix4.Identity, "title");
            load.AddComponent<InitLoading>();
            load.Enabled = false;
            MMW.RegistGameObject(load);

            var title = new GameObject("Title", Matrix4.Identity, "title");
            MMW.RegistGameObject(title);
            title.AddComponent<BackgroundScript>();
            title.AddComponent<TitleScript>();
            title.Enabled = false;

            var loading = new GameObject("Loading");
            loading.AddComponent<LoadingScript>();
            loading.Enabled = false;
            MMW.RegistGameObject(loading);

            var probj = new GameObject("Property Renderer");
            probj.AddComponent<PropertyRenderer>();
            MMW.RegistGameObject(probj);

            var debugger = new GameObject("Debugger");
            debugger.AddComponent<Debugger>();
            MMW.RegistGameObject(debugger);

            MMW.MainCamera.GameObject.UpdateAction += (s, ev) =>
            {
                var cc = MMW.FindGameComponent<GameComponents.ImageEffects.ColorCollect>();
                cc.Contrast = MMW.Contrast;
                cc.Saturation = MMW.Saturation;
                cc.Brightness = MMW.Brightness;
            };

            for (var i = 0; i < 200; i++)
            {
                var value = Tables.NextRankExp(i+1);
                Debug.WriteLine(i + ": " + value);
            }
            

            /*
            var g = Drawer.GetGraphics();
            var font = new Font("Yu Gothic UI Light", 32.0f);
            var size = g.MeasureString("0 1 2 3 4 5 6 7 8 9", font, 1024, new StringFormat() { FormatFlags = StringFormatFlags.MeasureTrailingSpaces });
            var bm = new Bitmap((int)size.Width + 1, (int)size.Height + 1);
            g = Graphics.FromImage(bm);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            g.Clear(Color.FromArgb(0));
            g.DrawString("0 1 2 3 4 5 6 7 8 9", font, Brushes.White, 0, 0);
            bm.Save(@"C:\Users\yoshihiro\Pictures\number.png");
            */

            // TODO: Bullet拘束機能
            // TODO: IK足修正
            // TODO: サーバー保存
            // TODO: キャラクタ作成
            // TODO: ワールド作成
            // TODO: アイテム使用の仕方
        }
    }
}
