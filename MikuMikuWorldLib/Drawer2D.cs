using MikuMikuWorld.Assets;
using MikuMikuWorldScript;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;
using GL = OpenTK.Graphics.OpenGL.GL;

namespace MikuMikuWorld
{
    public class Drawer2D : IDrawer
    {
        public Graphics Graphics => Drawer.GetGraphics();

        public int Width => (int)MMW.Width;
        public int Height => (int)MMW.Height;
        private Dictionary<Bitmap, Texture2D> texDic = new Dictionary<Bitmap, Texture2D>();

        private Bitmap bitmap;
        public Bitmap Texture
        {
            get { return bitmap; }
            set
            {
                if (value == null)
                {
                    bitmap = null;
                    tex = null;
                    return;
                } 
                bitmap = value;
                if (!texDic.ContainsKey(bitmap))
                {
                    var t = new Texture2D(bitmap);
                    t.Load();
                    texDic.Add(bitmap, t);
                }
                tex = texDic[bitmap];
            }
        }
        internal Texture2D tex { get; set; }

        public RectangleF SrcRect { get; set; } = new RectangleF(0, 0, 1, 1);
        public Vector2 CenterPivot { get; set; } = Vector2.One * 0.5f;
        public Vector3 Rotate { get; set; } = Vector3.Zero;
        public Vector2 Scale { get; set; } = Vector2.One;
        public Color4 Color { get; set; } = Color4.White;
        public float LineWidth { get; set; } = 1.0f;

        private void DrawBegin(RectangleF dstRect)
        {
            GL.Enable(EnableCap.AlphaTest);
            GL.Disable(EnableCap.DepthTest);
            GL.Color4(Color);
            GL.Viewport(0, 0, Width, Height);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            var w = MMW.Width;
            var h = MMW.Height;
            GL.Ortho(0, w, h, 0, -1, 1);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            var trans = new Vector3(dstRect.X + dstRect.Width * CenterPivot.X, dstRect.Y + dstRect.Height * CenterPivot.Y, 0.0f);
            GL.Translate(trans);
            GL.Scale(Scale.X, Scale.Y, 1.0f);
            GL.Rotate(Rotate.Z, 0.0f, 0.0f, 1.0f);
            GL.Rotate(Rotate.X, 1.0f, 0.0f, 0.0f);
            GL.Rotate(Rotate.Y, 0.0f, 1.0f, 0.0f);
            GL.Translate(-trans);
            GL.Translate(dstRect.X - 0.5f, dstRect.Y - 0.5f, 0.0f);
        }
        private void DrawEnd()
        {
            GL.Enable(EnableCap.DepthTest);
            GL.Disable(EnableCap.AlphaTest);
        }

        public void DrawTexture(float x, float y, bool flipY = false)
        {
            if (tex == null) return;
            DrawTexture(new RectangleF(x, y, tex.Size.Width, tex.Size.Height), flipY);
        }
        public void DrawTexture(RectangleF dstRect, bool flipY = false)
        {
            if (tex == null) return;

            Drawer.DrawTexture(tex, SrcRect, dstRect, Color, Rotate, Scale, CenterPivot, -1, flipY);
        }
        public void DrawLine(Vector2 from, Vector2 to)
        {
            GL.LineWidth(1.0f);
            var minX = Math.Min(from.X, to.X);
            var minY = Math.Min(from.Y, to.Y);
            var minV = new Vector2(minX, minY);
            var sx = Math.Abs(from.X - to.X);
            var sy = Math.Abs(from.Y - to.Y);
            DrawBegin(new RectangleF(minX, minY, sx, sy));
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex2(from - minV);
            GL.Vertex2(to - minV);
            GL.End();
            DrawEnd();

        }
        public void DrawRect(RectangleF dstRect)
        {
            GL.LineWidth(1.0f);
            DrawBegin(dstRect);
            GL.Begin(PrimitiveType.LineLoop);
            GL.Vertex2(0, 0);
            GL.Vertex2(dstRect.Width, 0);
            GL.Vertex2(dstRect.Width, dstRect.Height);
            GL.Vertex2(0, dstRect.Height);
            GL.End();
            DrawEnd();
        }
        public void FillRect(RectangleF dstRect)
        {
            DrawBegin(dstRect);
            GL.Begin(PrimitiveType.Quads);
            GL.Vertex2(0, 0);
            GL.Vertex2(dstRect.Width, 0);
            GL.Vertex2(dstRect.Width, dstRect.Height);
            GL.Vertex2(0, dstRect.Height);
            GL.End();
            DrawEnd();
        }
        public void DrawEllipse(RectangleF dstRect)
        {
            GL.LineWidth(1.0f);
            var w = dstRect.Width / 2.0;
            var h = dstRect.Height / 2.0;
            DrawBegin(dstRect);
            GL.Begin(PrimitiveType.LineLoop);
            for (var i = 0.0; i < 60.0; i++)
            {
                var rad = MathHelper.DegreesToRadians(i * 6.0);
                var s = Math.Sin(rad);
                var c = Math.Cos(rad);
                GL.Vertex2(w + c * w, h + s * h);
            }
            GL.End();
            DrawEnd();
        }
        public void FillEllipse(RectangleF dstRect)
        {
            var w = dstRect.Width / 2.0;
            var h = dstRect.Height / 2.0;
            DrawBegin(dstRect);
            GL.Begin(PrimitiveType.TriangleFan);
            GL.Vertex2(w, h);
            for (var i = 0.0; i <= 60.0; i++)
            {
                var rad = MathHelper.DegreesToRadians(i * 6.0);
                var s = Math.Sin(rad);
                var c = Math.Cos(rad);
                GL.Vertex2(w + c * w, h + s * h);
            }
            GL.End();
            DrawEnd();
        }
        public void DrawTriangle(RectangleF dstRect)
        {
            GL.LineWidth(1.0f);
            DrawBegin(dstRect);
            GL.Begin(PrimitiveType.LineLoop);
            GL.Vertex2(0.0f, dstRect.Height);
            GL.Vertex2(dstRect.Width * 0.5f, 0);
            GL.Vertex2(dstRect.Width, dstRect.Height);
            GL.End();
            DrawEnd();
        }
        public void FillTriangle(RectangleF dstRect)
        {
            DrawBegin(dstRect);
            GL.Begin(PrimitiveType.Triangles);
            GL.Vertex2(0.0f, dstRect.Height);
            GL.Vertex2(dstRect.Width * 0.5f, 0);
            GL.Vertex2(dstRect.Width, dstRect.Height);
            GL.End();
            DrawEnd();
        }

        public void SetIdentity()
        {
            SrcRect = new RectangleF(0, 0, 1, 1);
            CenterPivot = Vector2.One * 0.5f;
            Rotate = Vector3.Zero;
            Scale = Vector2.One;
            Color = Color4.White;
            LineWidth = 1.0f;
        }
    }
}
