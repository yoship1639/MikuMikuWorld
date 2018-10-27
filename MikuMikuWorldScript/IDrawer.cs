using OpenTK;
using OpenTK.Graphics;
using System.Drawing;

namespace MikuMikuWorldScript
{
    public interface IDrawer
    {
        /// <summary>
        /// グラフィクス
        /// </summary>
        Graphics Graphics { get; }

        int Width { get; }
        int Height { get; }

        void SetIdentity();

        Bitmap Texture { get; set; }
        RectangleF SrcRect { get; set; }
        Vector2 CenterPivot { get; set; }
        Vector3 Rotate { get; set; }
        Vector2 Scale { get; set; }
        Color4 Color { get; set; }
        float LineWidth { get; set; }

        /// <summary>
        /// テクスチャを描画する
        /// </summary>
        void DrawTexture(float x, float y, bool flipY = false);
        /// <summary>
        /// テクスチャを描画する
        /// </summary>
        /// <param name="dstRect">描画する画面領域(ピクセル)</param>
        void DrawTexture(RectangleF dstRect, bool flipY = false);

        /// <summary>
        /// 線を描画する
        /// </summary>
        /// <param name="from">線の視点</param>
        /// <param name="to">線の終点</param>
        void DrawLine(Vector2 from, Vector2 to);

        /// <summary>
        /// 四角線を描画する
        /// </summary>
        /// <param name="dstRect"></param>
        void DrawRect(RectangleF dstRect);
        /// <summary>
        /// 四角形を描画する
        /// </summary>
        /// <param name="dstRect"></param>
        void FillRect(RectangleF dstRect);

        /// <summary>
        /// 楕円線を描画する
        /// </summary>
        /// <param name="dstRect"></param>
        void DrawEllipse(RectangleF dstRect);
        /// <summary>
        /// 楕円を描画する
        /// </summary>
        /// <param name="dstRect"></param>
        void FillEllipse(RectangleF dstRect);

        /// <summary>
        /// 三角線を描画する
        /// </summary>
        /// <param name="dstRect"></param>
        void DrawTriangle(RectangleF dstRect);
        /// <summary>
        /// 三角形を描画する
        /// </summary>
        /// <param name="dstRect"></param>
        void FillTriangle(RectangleF dstRect);
    }
}