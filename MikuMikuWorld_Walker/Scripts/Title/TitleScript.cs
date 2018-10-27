using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikuMikuWorld.GameComponents;
using MikuMikuWorld.Assets;
using MikuMikuWorld.Properties;
using System.Drawing;
using OpenTK.Graphics;
using OpenTK.Input;
using OpenTK;
using MikuMikuWorld.Controls;
using MikuMikuWorld.Scripts.Title;

namespace MikuMikuWorld.Scripts
{
    class TitleScript : DrawableGameComponent
    {
        public bool AcceptInput { get; set; } = true;

        private List<Control> controls = new List<Control>();
        //MenuInputResolver input;

        //Texture2D texStar;

        //enum State
        //{
        //    ModeSelect,
        //    Option,
        //}

        //enum ModeSelect
        //{
        //    PlayOffline,
        //    PlayOnline,
        //    Option,
        //    Exit,
        //}

        //int modeSelectIndex = 0;
        //State state = State.ModeSelect;

        Texture2D texTitle;
        Texture2D texSubTitle;
        //Texture2D[] texModes;

        float transition = 0.0f;
        bool trans = false;
        TransitControl transit;

        //float rate = 0.0f;

        Font font;
        Font fontB;
        Font fontL;
        private Sound soundSelect;
        private Sound soundClick;

        public TitleScript() { }
        //public TitleScript(int modeSelectIndex)
        //{
        //    this.modeSelectIndex = modeSelectIndex;
        //}

        protected override void OnLoad()
        {
            base.OnLoad();

            Layer = LayerUI;

            MMW.FindGameComponent<BackgroundScript>().Trans(new Color4(148, 212, 222, 255), 0.25);

            transit = new TransitControl();
            transit.LocalLocation = new Vector2(0.0f, 0);
            transit.Size = new Vector2(MMW.ClientSize.Width, MMW.ClientSize.Height);
            transit.Target = Vector2.Zero;

            //input = new MenuInputResolver();
            //input.Up = Key.W;
            //input.Down = Key.S;
            //input.Right = Key.D;
            //input.Left = Key.A;

            //texStar = new Texture2D(Resources.star);
            //texStar.Load();

            font = new Font("Yu Gothic UI Light", 20.0f);
            fontB = new Font("Yu Gothic UI Light", 20.0f, FontStyle.Bold);
            fontL = new Font("Yu Gothic UI Light", 60.0f);

            texTitle = Drawer.CreateStringTexture("MIKU MIKU WORLD", fontL);
            texSubTitle = Drawer.CreateStringTexture("WALKER", fontL);

            var btnExit = new Button(transit, "Exit", new Vector2(60.0f, MMW.Height - 132.0f), new Vector2(140.0f, 32.0f), "click");
            btnExit.Clicked += (s, e) =>
            {
                MMW.Window.Close();
            };
            controls.Add(btnExit);

            var btnSingle = new Button(btnExit, "Single Play", new Vector2(0.0f, -120.0f), new Vector2(140.0f, 32.0f), "click");
            btnSingle.Clicked += (s, e) => 
            {

            };
            controls.Add(btnSingle);

            var btnMulti = new Button(btnExit, "Multi Play", new Vector2(0.0f, -80.0f), new Vector2(140.0f, 32.0f), "click");
            btnMulti.Clicked += (s, e) =>
            {
                trans = true;
                transit.Target = new Vector2(-MMW.ClientSize.Width * 2.0f, 0.0f);
                GameObject.AddComponent<ServerSelectScript>();
            };
            controls.Add(btnMulti);

            var btnOption = new Button(btnExit, "Option", new Vector2(0.0f, -40.0f), new Vector2(140.0f, 32.0f), "click");
            btnOption.Clicked += (s, e) =>
            {
                trans = true;
                transit.Target = new Vector2(-MMW.ClientSize.Width * 2.0f, 0.0f);
                GameObject.AddComponent<OptionScript>();
            };
            controls.Add(btnOption);

            /*
            var ts = new string[]
            {
                "SINGLE PLAY",
                "MULTI PLAY",
                "OPTION",
                "EXIT",
            };
            texModes = new Texture2D[4];
            for (var i = 0; i < 4; i++) texModes[i] = Drawer.CreateStringTexture(ts[i], fontB);
            */

            soundSelect = MMW.GetAsset<Sound>("select");
            soundClick = MMW.GetAsset<Sound>("click");
        }

        protected override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            //rate = MMWMath.Lerp(rate, 1.0f, (float)deltaTime * 10.0f);

            transition += (trans ? -1.0f : 1.0f) * (float)deltaTime * 5.0f;
            transition = MMWMath.Saturate(transition);

            transit.Update(deltaTime);

            if (AcceptInput && !trans)
            {
                controls.ForEach(c => c.Update(null, deltaTime));
                //input.Update(deltaTime);

                //if (input.IsDown || input.IsUp) rate = 0.0f;

                /*
                if (state == State.ModeSelect)
                {
                    if (input.IsDown)
                    {
                        modeSelectIndex = MMWMath.Repeat(modeSelectIndex + 1, 0, 3);
                        soundSelect.Play();
                    }
                    else if (input.IsUp)
                    {
                        modeSelectIndex = MMWMath.Repeat(modeSelectIndex - 1, 0, 3);
                        soundSelect.Play();
                    } 
                    else if (input.IsSelect)
                    {
                        soundClick.Play();
                        if (modeSelectIndex == (int)ModeSelect.PlayOffline)
                        {
                            trans = true;
                            GameObject.AddComponent<PlayerSelectScript>();
                            
                        }
                        else if (modeSelectIndex == (int)ModeSelect.PlayOnline)
                        {
                            trans = true;
                            GameObject.AddComponent<ServerSelectScript>();
                        }
                    }
                }
                else if (state == State.Option)
                {

                }
                */
            }

            if (trans && transition < 0.01f) Destroy();

        }

        protected override void Draw(double deltaTime, Camera camera)
        {
            var g = Drawer.BindGraphicsDraw();

            // タイトルロゴ
            Drawer.DrawTexturePixeledAlignment(texTitle, ContentAlignment.TopCenter, 0.0f, 60.0f);
            Drawer.DrawTexturePixeledAlignment(texSubTitle, ContentAlignment.TopCenter, 0.0f, 160.0f);

            if (GameObject.Enabled) controls.ForEach(c => c.Draw(g, deltaTime));
            /*
            if (state == State.ModeSelect)
            {
                var pen = new Pen(Color.White, 2.0f);
                for (var i = 0; i < 4; i++)
                {
                    Drawer.DrawTexturePixeledAlignment(texModes[i], ContentAlignment.TopLeft, 120.0f, MMW.ClientSize.Height + ((font.Size + 24.0f) * i) - 240.0f);
                    if (i == modeSelectIndex)
                    {
                        var x = 120.0f;
                        var y = MMW.ClientSize.Height + ((font.Size + 24.0f) * i) - 240.0f + font.Height;
                        var w = texModes[i].Size.Width;

                        g.DrawLine(pen, x, y, MMWMath.Lerp(x, x + w, rate), y);

                        Drawer.DrawTexturePixeledAlignment(texStar, ContentAlignment.TopLeft, MMWMath.Lerp(x, x + w, rate) - texStar.Size.Width * 0.5f, y - texStar.Size.Height * 0.5f, Color4.White, -(float)MMW.TotalElapsedTime * 2.0f, 1.0f / 32.0f);
                    }
                }
            }
            else if (state == State.Option)
            {

            }*/

            Drawer.IsGraphicsUsed = true;
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            controls.ForEach(c => c.Unload());

            //texStar.Unload();
        }

        public override GameComponent Clone()
        {
            throw new NotImplementedException();
        }
    }
}
