using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorldScript
{
    public interface ICamera : IGameComponent
    {
        /// <summary>
        /// 正射投影カメラか
        /// </summary>
        bool Orthographic { get; set; }

        /// <summary>
        /// カメラの上方向
        /// </summary>
        Vector3 Up { get; set; }

        /// <summary>
        /// 正射投影の幅
        /// </summary>
        float Width { get; set; }

        /// <summary>
        /// 正射投影の高さ
        /// </summary>
        float Height { get; set; }

        /// <summary>
        /// アスペクト比
        /// </summary>
        float Aspect { get; }

        /// <summary>
        /// 視野角
        /// </summary>
        float FoV { get; set; }

        /// <summary>
        /// ニアクリップ距離
        /// </summary>
        float Near { get; set; }

        /// <summary>
        /// ファークリップ距離
        /// </summary>
        float Far { get; set; }

        /// <summary>
        /// カメラ深度、値が小さいほど描画順が早い
        /// </summary>
        int Depth { get; set; }

        /// <summary>
        /// クリア色
        /// </summary>
        Color4 ClearColor { get; set; }

        /// <summary>
        /// スカイボックスの色
        /// </summary>
        Color4 SkyBoxColor { get; set; }

        /// <summary>
        /// シャドーマッピングを行うか
        /// </summary>
        bool ShadowMapping { get; set; }

        /// <summary>
        /// カメラのビュー上列を取得する
        /// </summary>
        Matrix4 View { get; }

        /// <summary>
        /// カメラの射影行列を取得する
        /// </summary>
        Matrix4 Projection { get; }

        /// <summary>
        /// カメラの透視投影射影行列を取得する
        /// </summary>
        Matrix4 PersProjection { get; }

        /// <summary>
        /// カメラの正射影行列を取得する
        /// </summary>
        Matrix4 OrthoProjection { get; }

        /// <summary>
        /// カメラのビューxプロジェクション行列を取得する
        /// </summary>
        Matrix4 ViewProjection { get; }
    }
}
