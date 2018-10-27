using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikuMikuWorld.GameComponents;
using MikuMikuWorld.Assets;
using OpenTK.Graphics;
using MikuMikuWorld.Controls;
using OpenTK;

namespace MikuMikuWorld.Scripts
{
    class LoadingMessage
    {
        public string mes;
    }

    class LoadingScript : DrawableGameComponent
    {
        private Func<LoadingMessage, object> asyncFunc;
        private object asyncResult;
        private Func<object, object> syncFunc;

        private float rate = -1.0f;
        private bool completed = false;
        private LoadingMessage mes = new LoadingMessage();

        public event EventHandler<object> LoadCompleted = delegate { };

        Texture2D tex;

        protected override void OnLoad()
        {
            Layer = LayerUI + 1;
            tex = MMW.GetAsset<Texture2D>("BlackMap");
        }

        public void StartLoading(Func<LoadingMessage, object> asyncFunc, Func<object, object> syncFunc)
        {
            this.asyncFunc = asyncFunc;
            this.syncFunc = syncFunc;
            rate = 0.0f;
        }

        protected override void Update(double deltaTime)
        {
            if (!completed && rate == 1.0f && asyncFunc == null)
            {
                var res = syncFunc(asyncResult);
                mes.mes = "";
                LoadCompleted(this, res);
                syncFunc = null;
                completed = true;
            }
            else if (!completed && rate >= 0.0f && rate < 1.0f)
            {
                rate += (float)deltaTime;
                rate = MMWMath.Clamp(rate, 0.0f, 1.0f);
                if (rate == 1.0f && asyncFunc != null)
                {
                    Task.Factory.StartNew(() =>
                    {
                        asyncResult = asyncFunc(mes);
                        asyncFunc = null;
                    });
                }
            }
            else if (completed && rate > 0.0f)
            {
                rate -= (float)deltaTime;
                rate = MMWMath.Clamp(rate, 0.0f, 1.0f);
                if (rate == 0.0f)
                {
                    completed = false;
                    rate = -1.0f;
                }
            }
        }

        protected override void Draw(double deltaTime, Camera camera)
        {
            var g = Drawer.BindGraphicsDraw();
            var white = new Color4(1.0f, 1.0f, 1.0f, rate);
            var black = new Color4(0.0f, 0.0f, 0.0f, rate);

            if (rate > 0.0f)
            {
                g.FillRectangle(new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(black.ToArgb())), new System.Drawing.Rectangle(0, 0, MMW.ClientSize.Width, MMW.ClientSize.Height));

                ControlDrawer.DrawString(g, "Now Loading...", MMW.ClientSize.Width - 200, MMW.ClientSize.Height - 48, white);
                if (!string.IsNullOrWhiteSpace(mes.mes))
                {
                    ControlDrawer.DrawString(g, mes.mes, 20, MMW.ClientSize.Height - 48, white);
                }

                Drawer.IsGraphicsUsed = true;
            }
        }

        public override GameComponent Clone()
        {
            throw new NotImplementedException();
        }
    }
}
