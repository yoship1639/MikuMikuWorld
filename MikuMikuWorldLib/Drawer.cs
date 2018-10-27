using MikuMikuWorld.Assets;
using MikuMikuWorld.Assets.Shaders;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    public static class Drawer
    {
        internal static void Init()
        {
            InitGraphics();
            InitTexture();

            lineMesh = new Mesh();
            lineMesh.Vertices = new Vector3[]
            {
                Vector3.Zero,
                Vector3.UnitZ,
            };
            lineMesh.SetIndices(0, new int[] { 0, 1 }, BeginMode.Lines);
            lineMesh.Load();

            lineShader = new LineShader();
            lineShader.Load();

            InitWireframe();
        }

        internal static void Resize()
        {
            InitGraphics();
        }

        #region Texture
        private static Mesh texMesh;
        private static Mesh texMesh2;
        private static TextureShader texShader;
        private static Texture2Shader tex2Shader;
        private static Matrix4 orthoMatrix;

        private static TextureCubeShader texCubeShader;
        private static Mesh cubeMesh;

        private static void InitTexture()
        {
            texMesh = new Mesh("Texture Mesh");
            texMesh.Vertices = new Vector3[]
            {
                new Vector3(-1.0f,  1.0f, 1.0f),
                new Vector3( 1.0f,  1.0f, 1.0f),
                new Vector3( 1.0f, -1.0f, 1.0f),
                new Vector3(-1.0f, -1.0f, 1.0f),
            };
            texMesh.SetIndices(0, new int[]
            {
                0, 1, 2, 3
            }, BeginMode.Quads);
            texMesh.UVs = new Vector2[]
            {
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 0.0f),
            };
            texMesh.Load();

            texMesh2 = new Mesh("Texture Mesh");
            texMesh2.Vertices = new Vector3[]
            {
                new Vector3( 0.0f,  1.0f, 0.0f),
                new Vector3( 1.0f,  1.0f, 0.0f),
                new Vector3( 1.0f,  0.0f, 0.0f),
                new Vector3( 0.0f,  0.0f, 0.0f),
            };
            texMesh2.SetIndices(0, new int[]
            {
                0, 1, 2, 3
            }, BeginMode.Quads);
            texMesh2.UVs = new Vector2[]
            {
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 0.0f),
            };
            texMesh2.Load();

            texShader = new TextureShader();
            texShader.Load();

            tex2Shader = new Texture2Shader();
            tex2Shader.Load();

            cubeMesh = Mesh.CreateSimpleBoxMesh(Vector3.One * 1000.0f);
            for (var i = 0; i < cubeMesh.Vertices.Length; i++)
            {
                cubeMesh.Vertices[i].X *= -1.0f;
            }
            cubeMesh.Load();

            texCubeShader = new TextureCubeShader();
            texCubeShader.Load();

            orthoMatrix = Matrix4.CreateOrthographicOffCenter(-1, 1, -1, 1, 0, 1);
        }

        public static bool PixelCeiling = false;
        public static void DrawTexture(Texture2D tex, RectangleF srcRect, RectangleF dstRect, Color4 color, int layer = -1, bool flipY = false)
        {
            
            if (tex == null) return;

            if (layer < 0) GL.Disable(EnableCap.DepthTest);
            texShader.UseShader();
            texShader.SetParameter(texShader.loc_srcRect, srcRect.ToVector4());
            texShader.SetParameter(texShader.loc_dstRect, dstRect.ToVector4());
            texShader.SetParameter(texShader.loc_layer, layer < 0 ? 0f : (float)layer);
            texShader.SetParameter(texShader.loc_flipY, flipY ? 1.0f : 0.0f);
            texShader.SetParameter(texShader.loc_color, ref color);
            texShader.SetParameter(TextureUnit.Texture0, tex);
            texShader.SetParameter(texShader.loc_mvp, ref orthoMatrix, false);
            DrawSubMesh(texMesh.subMeshes[0]);
            texShader.UnuseShader();
            GL.BindTexture(TextureTarget.Texture2D, 0);
            if (layer < 0) GL.Enable(EnableCap.DepthTest);
        }
        public static void DrawTexture(Texture2D tex, RectangleF srcRect, RectangleF dstRect, int layer = -1, bool flipY = false)
        {
            DrawTexture(tex, srcRect, dstRect, Color4.White, layer, flipY);
        }
        public static void DrawTexture(Texture2D tex, Color4 color, int layer = -1, bool flipY = false)
        {
            DrawTexture(tex, new RectangleF(0.0f, 0.0f, 1.0f, 1.0f), new RectangleF(0.0f, 0.0f, 1.0f, 1.0f), color, layer, flipY);
        }
        public static void DrawTexture(Texture2D tex, int layer = -1, bool flipY = false)
        {
            DrawTexture(tex, new RectangleF(0.0f, 0.0f, 1.0f, 1.0f), new RectangleF(0.0f, 0.0f, 1.0f, 1.0f), Color4.White, layer, flipY);
        }
        public static void DrawTexturePixeledAlignment(Texture2D tex, ContentAlignment alignment, float x, float y, int layer = -1, bool flipY = false)
        {
            DrawTexturePixeledAlignment(tex, alignment, x, y, Color4.White, 0.0f, 1.0f, layer, flipY);
        }
        public static void DrawTextureScaled(Texture2D tex, float x, float y, float width, float height, Color4 color, int layer = -1, bool flipY = false)
        {
            var dstRect = new RectangleF(x / MMW.RenderResolution.Width, y / MMW.RenderResolution.Height, width / MMW.RenderResolution.Width, height / MMW.RenderResolution.Height);
            DrawTexture(tex, new RectangleF(0.0f, 0.0f, 1.0f, 1.0f), dstRect, color, layer, flipY);
        }
        public static void DrawTexturePixeledAlignment(Texture2D tex, ContentAlignment alignment, float x, float y, Color4 color, float angle = 0.0f, float scale = 1.0f, int layer = -1, bool flipY = false)
        {
            var px = x;
            var py = y;
            if (alignment == ContentAlignment.TopLeft || alignment == ContentAlignment.MiddleLeft || alignment == ContentAlignment.BottomLeft)
            {
                px -= (MMW.ClientSize.Width - tex.Size.Width) * 0.5f;
                if (alignment == ContentAlignment.TopLeft) py -= (MMW.ClientSize.Height - tex.Size.Height) * 0.5f;
                else if (alignment == ContentAlignment.BottomLeft) py += (MMW.ClientSize.Height - tex.Size.Height) * 0.5f;
            }
            else if (alignment == ContentAlignment.TopCenter || alignment == ContentAlignment.MiddleCenter || alignment == ContentAlignment.BottomCenter)
            {
                if (alignment == ContentAlignment.TopCenter) py -= (MMW.ClientSize.Height - tex.Size.Height) * 0.5f;
                else if (alignment == ContentAlignment.BottomCenter) py += (MMW.ClientSize.Height - tex.Size.Height) * 0.5f;
            }
            else if (alignment == ContentAlignment.TopRight || alignment == ContentAlignment.MiddleRight || alignment == ContentAlignment.BottomRight)
            {
                px += (MMW.RenderResolution.Width - tex.Size.Width) * 0.5f;
                if (alignment == ContentAlignment.TopRight) py -= (MMW.ClientSize.Height - tex.Size.Height) * 0.5f;
                else if (alignment == ContentAlignment.BottomRight) py += (MMW.ClientSize.Height - tex.Size.Height) * 0.5f;
            }

            DrawTexturePixeled(tex, px, -py, color, angle, scale, layer, flipY);
        }
        public static void DrawTexturePixeled(Texture2D tex, float x, float y, Color4 color, float angle = 0.0f, float scale = 1.0f, int layer = -1, bool flipY = false)
        {
            if (tex == null) return;
            if (PixelCeiling)
            {
                x = (int)x;
                y = (int)y;
            }
            if (layer < 0) GL.Disable(EnableCap.DepthTest);
            tex2Shader.UseShader();
            tex2Shader.SetParameter(tex2Shader.loc_layer, layer < 0 ? 0f : (float)layer);
            tex2Shader.SetParameter(tex2Shader.loc_flipY, flipY ? 1.0f : 0.0f);
            tex2Shader.SetParameter(tex2Shader.loc_color, ref color);
            tex2Shader.SetParameter(TextureUnit.Texture0, tex);
            //var ortho = Matrix4.CreateOrthographicOffCenter(0, MMW.ClientSize.Width, MMW.ClientSize.Height, 0, 0, 1.0f);
            var ortho = Matrix4.CreateOrthographic(MMW.ClientSize.Width, MMW.ClientSize.Height, 0, 1.0f);
            var mat = MatrixHelper.CreateTransform(new Vector3(x, y, 0), new Vector3(0, 0, angle), new Vector3(tex.Size.Width * 0.5f * scale, tex.Size.Height * 0.5f * scale, 1.0f));
            tex2Shader.SetParameter(tex2Shader.loc_mvp, mat * ortho, false);
            DrawSubMesh(texMesh.subMeshes[0]);
            tex2Shader.UnuseShader();
            GL.BindTexture(TextureTarget.Texture2D, 0);
            if (layer < 0) GL.Enable(EnableCap.DepthTest);
            
        }

        public static void DrawTexture(Texture2D tex, RectangleF srcRect, RectangleF dstRect, Color4 color, Vector3 rot, Vector2 scale, Vector2 pivot, int layer = -1, bool flipY = false)
        {
            if (tex == null) return;
            if (layer < 0) GL.Disable(EnableCap.DepthTest);

            var ortho = Matrix4.CreateOrthographicOffCenter(0, MMW.ClientSize.Width, 0, MMW.ClientSize.Height, 0.0f, 1.0f);

            var trans = new Vector3(pivot.X, pivot.Y, 0.0f);
            var mat = Matrix4.CreateTranslation(-trans);
            mat *= MatrixHelper.CreateRotate(new Vector3(rot.X, rot.Y, -rot.Z));
            mat *= Matrix4.CreateScale(scale.X * dstRect.Width, scale.Y * dstRect.Height, 1.0f);
            mat.Row3 += new Vector4(trans + new Vector3(dstRect.X, MMW.Height - dstRect.Y, 0.0f));
            mat *= ortho;

            tex2Shader.UseShader();
            tex2Shader.SetParameter(tex2Shader.loc_layer, layer < 0 ? 0f : (float)layer);
            tex2Shader.SetParameter(tex2Shader.loc_flipY, flipY ? 1.0f : 0.0f);
            tex2Shader.SetParameter(tex2Shader.loc_color, ref color);
            tex2Shader.SetParameter(TextureUnit.Texture0, tex);
            tex2Shader.SetParameter(tex2Shader.loc_mvp, ref mat, false);
            DrawSubMesh(texMesh2.subMeshes[0]);
            tex2Shader.UnuseShader();

            GL.BindTexture(TextureTarget.Texture2D, 0);
            if (layer < 0) GL.Enable(EnableCap.DepthTest);
        }

        public static void DrawTextureCube(TextureCube cube, Vector3 dir, float fov, float aspect, Color4 color, float contrast = 1.0f, float saturation = 1.0f, float brightness = 1.0f)
        {
            if (cube == null) return;

            var mat = Matrix4.LookAt(Vector3.Zero, dir, Vector3.UnitY);
            mat *= Matrix4.CreatePerspectiveFieldOfView(fov, aspect, 1f, 10000.0f);

            GL.Disable(EnableCap.DepthTest);
            texCubeShader.UseShader();
            texCubeShader.SetParameter(texCubeShader.loc_color, color);
            texCubeShader.SetParameter(TextureUnit.Texture0, cube);
            texCubeShader.SetParameter(texCubeShader.loc_mvp, ref mat, false);
            texCubeShader.SetParameter(texCubeShader.loc_con, contrast);
            texCubeShader.SetParameter(texCubeShader.loc_sat, saturation);
            texCubeShader.SetParameter(texCubeShader.loc_brt, brightness);
            DrawSubMesh(cubeMesh.subMeshes[0]);
            texCubeShader.UnuseShader();
            GL.BindTexture(TextureTarget.TextureCubeMap, 0);
            GL.Enable(EnableCap.DepthTest);
        }
        internal static void DrawTextureMesh() { DrawSubMesh(texMesh.subMeshes[0]); }

        #endregion

        public static void DrawSubMesh(SubMesh sub)
        {
            GL.BindVertexArray(sub.vao);
            GL.DrawElements(sub.mode, sub.indices.Length, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }
        public static void DrawSubMesh(SubMesh sub, BeginMode mode)
        {
            GL.BindVertexArray(sub.vao);
            GL.DrawElements(mode, sub.indices.Length, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }

        public static RectangleF PixelToRect(float x, float y, float w, float h)
        {
            return new RectangleF()
            {
                X = x / MMW.RenderResolution.Width,
                Y = y / MMW.RenderResolution.Height,
                Width = w / MMW.RenderResolution.Width,
                Height = h / MMW.RenderResolution.Height,
            };
        }

        public static void DrawNumber(Texture2D tex, int number, int digit, float x, float y, float width, float height, float space = 0.0f, bool zeroPadding = false)
        {
            var seek = 0;
            var flag = false;
            while (digit-- > 0)
            {
                var n = number / (int)Math.Pow(10, digit);
                n = n % 10;

                if (n == 0 && !zeroPadding && !flag && digit > 0)
                {
                    seek++;
                    continue;  
                }
                flag = true;
                var dst = PixelToRect(x + seek * (width + space), y, width, height);
                var src = new RectangleF(0.1f * n, 0.0f, 0.1f, 1.0f);

                DrawTexture(tex, src, dst);
                seek++;
            }
        }

        #region Wireframe

        private static WireframeShader wireframeShader;
        private static Mesh wireframeBoxMesh;
        private static Mesh wireframeSphereMesh;

        private static void InitWireframe()
        {
            wireframeShader = new WireframeShader();
            wireframeShader.Load();

            wireframeBoxMesh = Mesh.CreateSimpleBoxMesh(Vector3.One, true);
            wireframeSphereMesh = Mesh.CreateSimpleSphereMesh(1.0f, 8, 6, true);
        }

        public static void DrawWireframeMesh(Mesh mesh, Matrix4 mvp, Color4 color)
        {
            wireframeShader.UseShader();
            wireframeShader.SetParameter(wireframeShader.loc_color, ref color);
            wireframeShader.SetParameter(wireframeShader.loc_mvp, ref mvp, false);
            for (var i = 0; i < mesh.SubMeshCount; i++) DrawSubMesh(mesh.subMeshes[i], BeginMode.Lines);
            wireframeShader.UnuseShader();
        }

        public static void DrawWireframeBox(Vector3 halfExtents, Matrix4 mvp, Color4 color, float width = 1.0f)
        {
            GL.LineWidth(width);
            mvp = Matrix4.CreateScale(halfExtents) * mvp;
            wireframeShader.UseShader();
            wireframeShader.SetParameter(wireframeShader.loc_color, ref color);
            wireframeShader.SetParameter(wireframeShader.loc_mvp, ref mvp, false);
            DrawSubMesh(wireframeBoxMesh.subMeshes[0]);
            wireframeShader.UnuseShader();
        }

        public static void DrawWireframeSphere(float radius, Matrix4 mvp, Color4 color, float width = 1.0f)
        {
            GL.LineWidth(width);
            mvp = Matrix4.CreateScale(radius) * mvp;
            wireframeShader.UseShader();
            wireframeShader.SetParameter(wireframeShader.loc_color, ref color);
            wireframeShader.SetParameter(wireframeShader.loc_mvp, ref mvp, false);
            DrawSubMesh(wireframeSphereMesh.subMeshes[0], BeginMode.LineStrip);
            wireframeShader.UnuseShader();
        }
        
        #endregion

        #region Line

        private static Mesh lineMesh;
        private static LineShader lineShader;

        public static void DrawLines(Line<Vector3>[] lines, Matrix4 mvp, Color4 color, float width = 1.0f)
        {
            DrawLines(lines, ref mvp, ref color, width);
        }
        public static void DrawLines(Line<Vector3>[] lines, ref Matrix4 mvp, ref Color4 color, float width = 1.0f)
        {
            GL.LineWidth(width);

            lineShader.UseShader();
            lineShader.SetParameter(lineShader.loc_color, ref color);
            foreach (var l in lines)
            {
                var v = l.to - l.from;
                var len = v.Length;
                if (len == 0.0f) continue;
                var angle = Vector3.CalculateAngle(Vector3.UnitZ, l.to - l.from);
                var axis = Vector3.Cross(Vector3.UnitZ, v).Normalized();
                var rot = Matrix4.CreateFromAxisAngle(axis, angle);
                var scale = Matrix4.CreateScale(len);
                var trans = Matrix4.CreateTranslation(l.from);
                var m = scale * rot * trans * mvp;
                lineShader.SetParameter(lineShader.loc_mvp, ref m, false);
                DrawSubMesh(lineMesh.subMeshes[0]);
            }
            lineShader.UnuseShader();
        }
        public static void DrawLine(Line<Vector3> line, Matrix4 mvp, Color4 color, float width = 1.0f)
        {
            DrawLines(new Line<Vector3>[] { line }, ref mvp, ref color, width);
        }
        public static void DrawLine(Vector3 from, Vector3 to, Matrix4 mvp, Color4 color, float width = 1.0f)
        {
            DrawLines(new Line<Vector3>[] { new Line<Vector3>(from, to) }, ref mvp, ref color, width);
        }
        public static void DrawLine(ref Vector3 from, ref Vector3 to, ref Matrix4 mvp, ref Color4 color, float width = 1.0f)
        {
            DrawLines(new Line<Vector3>[] { new Line<Vector3>(from, to) }, ref mvp, ref color, width);
        }

        #endregion

        public static void DrawLine2D(Vector2 from, Vector2 to, Color4 color)
        {
            ScriptDrawer.Color = color;
            ScriptDrawer.DrawLine(from, to);
        }
        public static void DrawRect(Vector2 pos, Vector2 size, Color4 color)
        {
            ScriptDrawer.Color = color;
            ScriptDrawer.DrawRect(new RectangleF(pos.X, pos.Y, size.X, size.Y));
        }
        public static void FillRect(Vector2 pos, Vector2 size, Color4 color)
        {
            ScriptDrawer.Color = color;
            ScriptDrawer.FillRect(new RectangleF(pos.X, pos.Y, size.X, size.Y));
        }

        private static RectangleF clip;
        public static void SetClip(Graphics g, float x, float y, float width, float height)
        {
            g.SetClip(new RectangleF(x, y, width, height));
            GL.Enable(EnableCap.ScissorTest);
            GL.Scissor((int)x, (int)MMW.Height - (int)(y + height), (int)width, (int)height);
            clip = new RectangleF(x, y, width, height);
        }
        public static void ResetClip(Graphics g)
        {
            GL.Scissor(0, 0, MMW.RenderResolution.Width, MMW.RenderResolution.Height);
            GL.Disable(EnableCap.ScissorTest);
            g.ResetClip();
            clip = new RectangleF(0, 0, MMW.RenderResolution.Width, MMW.RenderResolution.Height);
        }
        public static RectangleF GetClip() => clip;

        #region Graphics
        private static Bitmap graphicsBitmap;
        private static Graphics graphics;
        private static Texture2D graphicsTexture;
        private static byte[] graphicsData;

        private static void InitGraphics()
        {
            
            if (graphicsTexture != null) graphicsTexture.Unload();
            if (graphicsBitmap != null) graphicsBitmap.Dispose();

            graphicsBitmap = new Bitmap(MMW.ClientSize.Width, MMW.ClientSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            graphicsTexture = new Texture2D(graphicsBitmap.Width, graphicsBitmap.Height, PixelInternalFormat.Rgba)
            {
                SrcBitmap = graphicsBitmap,
                texture = GL.GenTexture()
            };
            graphicsData = new byte[MMW.ClientSize.Width * MMW.ClientSize.Height * 4];
            for (var i = 0; i < MMW.ClientSize.Width * MMW.ClientSize.Height; i++)
            {
                graphicsData[i * 4 + 0] = 255;
                graphicsData[i * 4 + 1] = 255;
                graphicsData[i * 4 + 2] = 255;
                graphicsData[i * 4 + 3] = 0;
            } 

            graphics = Graphics.FromImage(graphicsBitmap);
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            //graphics.SmoothingMode = SmoothingMode.Default;
            //graphics.CompositingQuality = CompositingQuality.Default;
            //graphics.CompositingMode = CompositingMode.SourceOver;
        }

        public static bool IsGraphicsUsed = false;
        internal static void ClearGraphics()
        {
            var data = graphicsBitmap.LockBits(new Rectangle(0, 0, graphicsBitmap.Width, graphicsBitmap.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            System.Runtime.InteropServices.Marshal.Copy(graphicsData, 0, data.Scan0, graphicsData.Length);
            graphicsBitmap.UnlockBits(data);
            //graphics.Clear(Color.FromArgb(0));
            IsGraphicsUsed = false;
        }

        public static Graphics GetGraphics()
        {
            return graphics;
        }
        
        public static Graphics BindGraphicsDraw()
        {
            GL.BindTexture(TextureTarget.Texture2D, graphicsTexture.texture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            //graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
            //graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            return graphics;
        }

        internal static void EndGraphicsDraw()
        {
            if (!IsGraphicsUsed) return;

            var data = graphicsBitmap.LockBits(new Rectangle(0, 0, graphicsBitmap.Width, graphicsBitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.BindTexture(TextureTarget.Texture2D, graphicsTexture.texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, graphicsBitmap.Width, graphicsBitmap.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            graphicsBitmap.UnlockBits(data);

            DrawTexture(graphicsTexture, -1, true);
        }

        public static Graphics CreateGraphics(Texture2D tex)
        {
            var g = Graphics.FromImage(tex.SrcBitmap);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;

            return g;
        }

        public static void BitmapToTexture(Texture2D tex)
        {
            var data = tex.SrcBitmap.LockBits(new Rectangle(0, 0, tex.SrcBitmap.Width, tex.SrcBitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            GL.BindTexture(TextureTarget.Texture2D, tex.texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, tex.SrcBitmap.Width, tex.SrcBitmap.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            tex.SrcBitmap.UnlockBits(data);
        }

        public static Vector2 GetLocation(float width, float height, ContentAlignment alignment)
        {
            var v = new Vector2();
            if (alignment == ContentAlignment.TopLeft || alignment == ContentAlignment.MiddleLeft || alignment == ContentAlignment.BottomLeft)
            {
                if (alignment == ContentAlignment.MiddleLeft) v.Y = (MMW.ClientSize.Height - height) / 2.0f;
                if (alignment == ContentAlignment.BottomLeft) v.Y = MMW.ClientSize.Height - height;
            }
            else if (alignment == ContentAlignment.TopCenter || alignment == ContentAlignment.MiddleCenter || alignment == ContentAlignment.BottomCenter)
            {
                v.X = (MMW.ClientSize.Width - width) / 2.0f;
                if (alignment == ContentAlignment.MiddleCenter) v.Y = (MMW.ClientSize.Height - height) / 2.0f;
                if (alignment == ContentAlignment.BottomCenter) v.Y = MMW.ClientSize.Height - height;
            }
            else if (alignment == ContentAlignment.TopRight || alignment == ContentAlignment.MiddleRight || alignment == ContentAlignment.BottomRight)
            {
                v.X = MMW.ClientSize.Width - width;
                if (alignment == ContentAlignment.MiddleRight) v.Y = (MMW.ClientSize.Height - height) / 2.0f;
                if (alignment == ContentAlignment.BottomRight) v.Y = MMW.ClientSize.Height - height;
            }
            return v;
        }

        #endregion

        #region IDrawer

        public static Drawer2D ScriptDrawer { get; private set; } = new Drawer2D();
        public static Drawer3D ScriptMeshDrawer { get; private set; } = new Drawer3D();

        #endregion

        private static StringFormat sf = new StringFormat(StringFormatFlags.MeasureTrailingSpaces);
        public static Texture2D CreateStringTexture(string text, Font font, int maxWidth = -1, bool antialias = true)
        {
            if (maxWidth < 0) maxWidth = int.MaxValue;
            var size = graphics.MeasureString(text, font, maxWidth, sf);

            var bitmap = new Bitmap((int)size.Width, (int)size.Height);
            var g = Graphics.FromImage(bitmap);
            g.Clear(Color.FromArgb(0, 255, 255, 255));
            if (antialias) g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            g.DrawString(text, font, Brushes.White, new RectangleF(PointF.Empty, size), sf);
            g.Dispose();
            var tex = new Texture2D(bitmap);
            tex.Load();

            return tex;
        }

        public static Vector2 MeasureString(string text, Font font)
        {
            var s = graphics.MeasureString(text, font);
            return new Vector2(s.Width, s.Height);
        }
    }
}
