using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorldScript
{
    public interface IMeshDrawer
    {
        // TODO: IMeshDrawer

        

        /// <summary>
        /// ビルボードを描画する
        /// </summary>
        /// <param name="texture">描画するテクスチャ</param>
        /// <param name="position">ワールド位置座標</param>
        /// <param name="size">ワールド座標での大きさ</param>
        /// <param name="color">描画色</param>
        /// <param name="freezeYAxis">Y軸を固定するか</param>
        /// <param name=""></param>
        void DrawBillboard(Bitmap texture, Vector3 position, Vector2 size, Color4 color, bool freezeYAxis = true);
        /// <summary>
        /// ビルボードを描画する
        /// </summary>
        /// <param name="texture">描画するテクスチャ</param>
        /// <param name="srcRect">テクスチャの描画範囲(UV座標)</param>
        /// <param name="position">ワールド位置座標</param>
        /// <param name="size">ワールド座標での大きさ</param>
        /// <param name="color">描画色</param>
        /// <param name="freezeYAxis">Y軸を固定するか</param>
        /// <param name=""></param>
        void DrawBillboard(Bitmap texture, RectangleF srcRect, Vector3 position, Vector2 size, Color4 color, bool freezeYAxis = true);

        /// <summary>
        /// 線を描画する
        /// </summary>
        /// <param name="from">線の始点</param>
        /// <param name="to">線の終点</param>
        /// <param name="viewProj">カメラのビュープロジェクション行列</param>
        /// <param name="color">線の色</param>
        /// <param name="width">線の太さ</param>
        void DrawLine(Vector3 from, Vector3 to, Matrix4 viewProj, Color4 color, float width = 1.0f);

        /// <summary>
        /// ボックスを描画する
        /// </summary>
        /// <param name="halfExtents">ボックスの大きさ</param>
        /// <param name="mvp">モデルビュープロジェクション行列</param>
        /// <param name="color">描画色</param>
        void DrawBox(Vector3 halfExtents, Matrix4 mvp, Color4 color);
    }
}
