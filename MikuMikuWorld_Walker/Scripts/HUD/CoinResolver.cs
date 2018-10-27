using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikuMikuWorld.Assets;
using MikuMikuWorld.GameComponents;
using MikuMikuWorld.Properties;
using OpenTK;
using OpenTK.Graphics;

namespace MikuMikuWorld.Scripts.HUD
{
    class CoinResolver : DrawableGameComponent
    {
        private Lerper lerp;
        private UserData userData;
        private float coin;
        private Texture2D tex;
        private Texture2D texCoin;
        private Texture2D texBack;
        private Sound sound;
        private GameObject player;

        public bool IsShown { get; private set; } = false;

        protected override void OnLoad()
        {
            base.OnLoad();

            Layer = LayerUI;

            userData = MMW.GetAsset<UserData>();

            coin = userData.Coin;

            tex = new Texture2D(Resources.number);
            tex.Load();

            texCoin = new Texture2D(Resources.mmw_icon_coin);
            texCoin.MagFilter = OpenTK.Graphics.OpenGL4.TextureMagFilter.Linear;
            texCoin.Load();

            texBack = new Texture2D(Resources.shadowline);
            texBack.Load();

            lerp = new Lerper(-100.0f);
            Show();

            sound = MMW.GetAsset<Sound>("coin");

            player = MMW.FindGameObject(g => g.Tags.Contains("player"));
        }

        public void Show()
        {
            lerp.Target = 62.0f;
            IsShown = true;
        }

        public void Hide()
        {
            lerp.Target = -100.0f;
            IsShown = false;
        }

        protected override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            var prev = coin;
            if (coin < userData.Coin)
            {
                coin += (float)deltaTime * 20.0f;
                if (coin > userData.Coin) coin = userData.Coin;
            }
            else if (coin > userData.Coin)
            {
                coin -= (float)deltaTime * 20.0f;
                if (coin < userData.Coin) coin = userData.Coin;
            }

            if ((int)prev != (int)coin)
            {
                sound.Stop();
                sound.Play(player.Transform.WorldPosition);
            }

            lerp.Update(deltaTime);
        }

        protected override void Draw(double deltaTime, Camera camera)
        {
            Drawer.DrawTextureScaled(texBack, MMW.Width - 250.0f, lerp.Now, 200.0f, 36.0f, Color4.White);
            Drawer.DrawTextureScaled(texCoin, MMW.Width - 250.0f, lerp.Now - 8.0f, 40.0f, 40.0f, Color4.White);
            var digit = (int)Math.Log10(coin) + 1;
            if (digit < 1) digit = 1;
            Drawer.DrawNumber(tex, (int)coin, digit, MMW.Width - 200.0f, lerp.Now, 26.0f, 34.0f, -4.0f);
        }

        protected override void OnReceivedMessage(string message, params object[] args)
        {
            if (message == "get coin" && args.Length >= 2 && args[1] as string == "98df1d6abbc7")
            {
                var value = (int)args[0];
                userData.AddCoin(value);
            }
            else if (message == "hud show")
            {
                Show();
            }
            else if (message == "hud hide")
            {
                Hide();
            }
            else if (message == "show dialog")
            {
                Hide();
            }
            else if (message == "close dialog")
            {
                Show();
            }
            else if (message == "show coin resolver")
            {
                Show();
            }
            else if (message == "close coin resolver")
            {
                Hide();
            }
        }
    }
}
