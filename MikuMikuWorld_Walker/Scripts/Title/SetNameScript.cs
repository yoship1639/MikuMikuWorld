using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikuMikuWorld.GameComponents;
using OpenTK;
using MikuMikuWorld.Assets;
using OpenTK.Graphics;

namespace MikuMikuWorld.Scripts
{
    class SetNameScript : DrawableGameComponent
    {
        Font font;
        Brush brush;

        Texture2D tex;
        Vector2 fix;
        public float RenderHeight { get; set; } = 1.7f;

        public bool Visible { get; set; } = true;

        protected override void OnLoad()
        {
            base.OnLoad();

            font = new Font("Yu Gothic UI Light", 10.0f);
            brush = Brushes.White;

            var size = Drawer.MeasureString(GameObject.Name, font);
            fix = new Vector2(size.X, size.Y) * 0.5f;
            tex = new Texture2D((int)size.X, (int)size.Y);
            tex.Load();

            var g = Drawer.CreateGraphics(tex);
            //g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            //g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
            g.Clear(Color.FromArgb(RandomHelper.NextInt() % 128, RandomHelper.NextInt() % 128, RandomHelper.NextInt() % 128));
            g.DrawString(GameObject.Name, font, brush, 0, 0);
            Drawer.BitmapToTexture(tex);
        }

        protected override void Draw(double deltaTime, Camera camera)
        {
            if (!Visible) return;

            var d = new Vector4(Transform.WorldPosition + new Vector3(0.0f, RenderHeight, 0.0f), 1.0f) * camera.View;
            if (d.Z > 0.0f) return;

            var vps = camera.ViewProjection * MatrixHelper.CreateScreen(MMW.RenderResolution.ToVector2());
            var v = new Vector4(0.0f, RenderHeight, 0.0f, 1.0f) * Transform.WorldTransform * vps;
            var pos = v.Xy / v.W;

            var scale = MMWMath.Clamp(Math.Abs(2.0f / -d.Z), 0.01f, 1.0f);
            pos -= fix;
            var col = new Color4(1.0f, 1.0f, 1.0f, MMWMath.Clamp(1.0f - (-d.Z / 100.0f), 0.0f, 1.0f));

            var g = Drawer.GetGraphics();
            g.DrawString(GameObject.Name, font, Brushes.Black, pos.X + 2, pos.Y + 1);
            g.DrawString(GameObject.Name, font, brush, pos.X, pos.Y);
            Drawer.IsGraphicsUsed = true;
            //Drawer.DrawTexturePixeledAlignment(tex, ContentAlignment.TopLeft, pos.X, pos.Y, col, 0.0f, scale, 200 - (int)(-d.Z), true);
        }

        public override GameComponent Clone()
        {
            throw new NotImplementedException();
        }
    }
}
