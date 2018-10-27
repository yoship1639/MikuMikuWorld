//
// Miku Miku World License
//
// Copyright (c) 2017 Miku Miku World.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//

using MikuMikuWorld.Assets;
using MikuMikuWorld.Assets.Shaders;
using MikuMikuWorld.GameComponents.ImageEffects;
using MikuMikuWorld.GameComponents.Lights;
using MikuMikuWorldScript;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.GameComponents
{
    public enum ClearFlag
    {
        SolidColor,
        DepthOnly,
        SkyBox,
        DoNotClear,
    }

    /// <summary>
    /// 視界の情報を持つコンポーネントクラス
    /// </summary>
    public class Camera : GameComponent, ICamera
    {
        /// <summary>
        /// 描画領域サイズが変更されたとき自動的にカメラのサイズも変えるか
        /// </summary>
        public bool AutoResize { get; set; } = true;

        /// <summary>
        /// 正射投影カメラか
        /// </summary>
        public bool Orthographic { get; set; } = false;

        /// <summary>
        /// カメラの上方向
        /// </summary>
        public Vector3 Up { get; set; } = Vector3.UnitY;

        /// <summary>
        /// 正射投影の幅
        /// </summary>
        public float Width { get; set; } = MMW.RenderResolution.Width;

        /// <summary>
        /// 正射投影の高さ
        /// </summary>
        public float Height { get; set; } = MMW.RenderResolution.Height;

        /// <summary>
        /// アスペクト比
        /// </summary>
        public float Aspect { get { return Width / Height; } }

        /// <summary>
        /// 視野角
        /// </summary>
        public float FoV { get; set; } = 1.2f;

        /// <summary>
        /// ニアクリップ距離
        /// </summary>
        public float Near { get; set; } = 0.1f;

        /// <summary>
        /// ファークリップ距離
        /// </summary>
        public float Far { get; set; } = 1000f;

        /// <summary>
        /// カメラ深度、値が小さいほど描画順が早い
        /// </summary>
        public int Depth { get; set; } = 0;

        /// <summary>
        /// クリア色
        /// </summary>
        public Color4 ClearColor { get; set; } = Color4.Black;

        /// <summary>
        /// クリアの種類
        /// </summary>
        public ClearFlag ClearFlag { get; set; } = ClearFlag.SkyBox;

        /// <summary>
        /// クリアに用いるスカイボックス
        /// </summary>
        public TextureCube ClearSkyBox { get; set; } = MMW.GetAsset<TextureCube>("DefaultSkyBox");

        /// <summary>
        /// スカイボックスの色
        /// </summary>
        public Color4 SkyBoxColor { get; set; } = new Color4(1.0f, 1.0f, 1.0f, 1.0f);

        public float SkyBoxContrast { get; set; } = 1.0f;
        public float SkyBoxSaturation { get; set; } = 1.0f;
        public float SkyBoxBrightness { get; set; } = 1.3f;

        /// <summary>
        /// 環境マップ
        /// </summary>
        public TextureCube EnvironmentMap { get; set; } = MMW.GetAsset<TextureCube>("DefaultSkyBox");

        /// <summary>
        /// シャドーマッピングを行うか
        /// </summary>
        public bool ShadowMapping { get; set; } = false;

        /// <summary>
        /// デバッグ描画
        /// </summary>
        public bool DebugDraw { get; set; } = false;

        /// <summary>
        /// メッシュを描画するか
        /// </summary>
        public bool MeshDraw { get; set; } = true;

        /// <summary>
        /// コンポーネントスクリプトを描画するか
        /// </summary>
        public bool ScriptDraw { get; set; } = true;

        /// <summary>
        /// UI層のコンポーネントを描画するか
        /// </summary>
        public bool UIDraw { get; set; } = true;

        protected Vector3 prevLightDir;
        protected Vector3 prevCameraPos;
        protected RenderTexture shadowDepthRT1;
        protected RenderTexture shadowDepthRT2;
        protected RenderTexture shadowDepthRT3;
        protected Matrix4 shadowDepthBias;
        protected DepthShader depthShader;
        protected Texture2D whiteMap;

        protected RenderTexture depthRT;

        protected ColorShader colorShader;
        protected RenderTexture colorRT;
        protected Color4[] randColors;

        protected VelocityShader velocityShader;
        protected RenderTexture velocityRT;

        protected RenderTexture targetTexture;
        /// <summary>
        /// 描画対象のテクスチャ。Nullにするとバックバッファ用レンダーテクスチャを対象に描画する
        /// </summary>
        public RenderTexture TargetTexture
        {
            get { if (targetTexture == null) return MMW.defaultRenderTarget; return targetTexture; }
            set { targetTexture = value; }
        }

        protected BitArray layerMask = new BitArray(MMWSettings.Default.LayerMaskMax, true);

        /// <summary>
        /// カメラの描画レイヤをセットする。trueにするとそのレイヤ上のオブジェクトは描画される
        /// </summary>
        /// <param name="index">レイヤ番号(0 ~ 31まで)</param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Result SetLayerMask(int index, bool value)
        {
            if (index < 0 || index >= MMWSettings.Default.LayerMaskMax) return Result.OutOfIndex;
            layerMask.Set(index, value);
            return Result.Success;
        }

        /// <summary>
        /// カメラの描画レイヤを取得する
        /// </summary>
        /// <param name="index">レイヤ番号(0 ~ 31まで)</param>
        /// <returns></returns>
        public bool GetLayerMask(int index)
        {
            if (index < 0 || index >= MMWSettings.Default.LayerMaskMax) return false;
            return layerMask.Get(index);
        }

        public bool ForceTarget { get; set; }
        public Vector3 Target { get; set; }

        /// <summary>
        /// カメラのビュー上列を取得する
        /// </summary>
        public Matrix4 View
        {
            get
            {
                var worldPos = GameObject.Transform.WorldPosition;
                if (ForceTarget) return Matrix4.LookAt(worldPos, Target, Up);
                var target = worldPos + Vector3.TransformVector(Vector3.UnitZ, GameObject.Transform.WorldTransform);
                return Matrix4.LookAt(worldPos, target, Up);
            }
        }

        /// <summary>
        /// カメラの射影行列を取得する
        /// </summary>
        public Matrix4 Projection
        {
            get
            {
                if (Orthographic) return Matrix4.CreateOrthographic(Width, Height, Near, Far);
                return Matrix4.CreatePerspectiveFieldOfView(FoV, Aspect, Near, Far);
            }
        }

        /// <summary>
        /// カメラの透視投影行列を取得する
        /// </summary>
        public Matrix4 PersProjection
        {
            get
            {
                return Matrix4.CreatePerspectiveFieldOfView(FoV, Aspect, Near, Far);
            }
        }

        /// <summary>
        /// カメラの正射影行列を取得する
        /// </summary>
        public Matrix4 OrthoProjection
        {
            get
            {
                return Matrix4.CreateOrthographic(Width, Height, Near, Far);
            }
        }

        /// <summary>
        /// カメラのビューXプロジェクション行列を取得する
        /// </summary>
        public Matrix4 ViewProjection
        {
            get { return View * Projection; }
        }

        /// <summary>
        /// ローカル座標の向きを取得する
        /// </summary>
        public Vector3 LocalDirection
        {
            get
            {
                return Vector3.TransformNormal(Vector3.UnitZ, GameObject.Transform.LocalTransform);
            }
        }

        /// <summary>
        /// ワールド座標の向きを取得する
        /// </summary>
        public Vector3 WorldDirection
        {
            get
            {
                return Vector3.TransformNormal(Vector3.UnitZ, GameObject.Transform.WorldTransform);
            }
        }

        protected internal override void OnLoad()
        {
            base.OnLoad();

            CreateShadowMap();

            whiteMap = MMW.GetAsset<Texture2D>("WhiteMap");
            depthShader = (DepthShader)MMW.GetAsset<Shader>("Depth");
            colorShader = (ColorShader)MMW.GetAsset<Shader>("Color");

            depthRT = new RenderTexture(MMW.RenderResolution);
            depthRT.ColorFormat0 = PixelInternalFormat.Rgba16f;
            depthRT.Load();

            colorRT = new RenderTexture(MMW.RenderResolution);
            colorRT.ColorFormat0 = MMW.Configuration.DefaultPixelFormat;
            colorRT.Load();
            randColors = new Color4[ushort.MaxValue + 1];
            for (var i = 0; i < randColors.Length; i++) randColors[i] = new Color4(
                RandomHelper.NextFloat() * 0.5f + 0.5f,
                RandomHelper.NextFloat() * 0.5f + 0.5f,
                RandomHelper.NextFloat() * 0.5f + 0.5f,
                1.0f);

            shadowDepthBias = Matrix4.CreateScale(0.5f) * Matrix4.CreateTranslation(0.5f, 0.5f, 0.5f);

            velocityShader = (VelocityShader)MMW.GetAsset<Shader>("Velocity");
            velocityRT = new RenderTexture(MMW.RenderResolution);
            velocityRT.ColorFormat0 = PixelInternalFormat.Rgba16f;
            velocityRT.Load();

            sp = new ShaderUniqueParameter()
            {
                camera = this,
            };

            getter.Add("Orthographic", obj => Orthographic);
            getter.Add("Up", obj => Up);
            getter.Add("Width", obj => Width);
            getter.Add("Height", obj => Height);
            getter.Add("Aspect", obj => Aspect);
            getter.Add("FoV", obj => FoV);
            getter.Add("Near", obj => Near);
            getter.Add("Far", obj => Far);
            getter.Add("Depth", obj => Depth);
            getter.Add("ClearColor", obj => ClearColor);
        }

        protected ShaderUniqueParameter sp;
        protected class SubMeshData
        {
            public SubMesh SubMesh;
            public Matrix4 WorldTransform;
            public Matrix4 WorldTransformInv;
            public Matrix4 OldWorldTransform;
            public bool Visible;
            public bool CastShadow;
            public GameObject GameObject;
            public AMorpher Morpher;
            public AAnimator Animator;
            public int OcclusionQuery;
            public MeshRenderer MeshRenderer;
        }

        protected internal virtual void Draw(double deltaTime)
        {
            var objects = MMW.FindGameObjects(o => true);
            var drawMeshDic = new Dictionary<Shader, Dictionary<Material, List<SubMeshData>>>();

            var animList = new List<AAnimator>();
            var morphList = new List<AMorpher>();

            var clipDic = new Dictionary<GameObject, bool>();
            var clipTask = Task.Factory.StartNew(() =>
            {
                foreach (var obj in objects)
                {
                    var mr = obj.GetComponent<MeshRenderer>();
                    if (mr == null || !mr.Enabled)
                    {
                        clipDic.Add(obj, true);
                        continue;
                    }

                    var clip = false;
                    var b = mr.Mesh.Bounds;

                    var mvp = obj.Transform.WorldTransform * ViewProjection;
                    var min = new Vector4(b.Min, 1.0f) * mvp;
                    var max = new Vector4(b.Max, 1.0f) * mvp;
                    min /= min.W;
                    max /= max.W;

                    if (min.X < -1.2f && max.X < -1.2f) clip = true;
                    if (min.X > 1.2f && max.X > 1.2f) clip = true;
                    if (min.Y < -1.2f && max.Y < -1.2f) clip = true;
                    if (min.Y > 1.2f && max.Y > 1.2f) clip = true;
                    if (min.Z < -1.2f && max.Z < -1.2f) clip = true;
                    if (min.Z > 1.2f && max.Z > 1.2f) clip = true;

                    clipDic.Add(obj, clip);
                }
            });

            var animTasks = new List<Task>();
            var findAnimTask = Task.Factory.StartNew(() =>
            {
                clipTask.Wait();
                foreach (var obj in objects)
                {
                    if (!obj.Enabled) continue;
                    if (clipDic[obj]) continue;

                    var animator = obj.GetComponent<AAnimator>();
                    var morpher = obj.GetComponent<AMorpher>();

                    if (animator != null && animator.Enabled)
                    {
                        animTasks.Add(Task.Run(() => animator.CalcTransform()));
                        animList.Add(animator);
                    }

                    if (morpher != null && morpher.Enabled)
                    {
                        animTasks.Add(Task.Run(() => morpher.CalcMorph()));
                        morphList.Add(morpher);
                    }
                }
            });

            if (MeshDraw)
            {
                Matrix4 proj;
                if (Orthographic) proj = Matrix4.CreateOrthographic(TargetTexture.Size.Width, TargetTexture.Size.Height, Near, Far);
                proj = Matrix4.CreatePerspectiveFieldOfView(FoV, Aspect, Near, Far);

                sp.pointLights = null;
                sp.spotLights = null;
                sp.camera = this;
                sp.resolution = new Vector2(TargetTexture.Size.Width, TargetTexture.Size.Height);
                sp.deltaTime = deltaTime;
                sp.oldViewProj = sp.viewProj;
                sp.view = View;
                sp.proj = proj;
                sp.cameraPos = GameObject.Transform.WorldPosition;
                sp.cameraDir = WorldDirection;
                sp.dirLight = MMW.DirectionalLight;
                var viewproj = sp.view * sp.proj;
                sp.viewProj = viewproj;
                sp.viewInverse = sp.view.Inverted();
                sp.projInverse = sp.proj.Inverted();
                sp.viewProjInverse = sp.viewProj.Inverted();
                sp.environmentMap = EnvironmentMap;

                // 描画準備
                clipTask.Wait();
                foreach (var obj in objects)
                {
                    if (!obj.Enabled) continue;
                    if (clipDic[obj]) continue;

                    var renderers = obj.GetComponents<MeshRenderer>();
                    var animator = obj.GetComponent<AAnimator>();
                    var morpher = obj.GetComponent<AMorpher>();

                    var world = obj.Transform.WorldTransform;
                    var oldWorld = obj.Transform.OldWorldTransfom;
                    foreach (var r in renderers)
                    {
                        if (!r.Enabled) continue;
                        for (var i = 0; i < r.MaterialCount; i++)
                        {
                            var mat = r.GetMaterialAt(i);
                            if (!drawMeshDic.ContainsKey(mat.Shader)) drawMeshDic.Add(mat.Shader, new Dictionary<Material, List<SubMeshData>>());
                            if (!drawMeshDic[mat.Shader].ContainsKey(mat)) drawMeshDic[mat.Shader].Add(mat, new List<SubMeshData>());
                        }
                        foreach (var sm in r.Mesh.subMeshes)
                        {
                            var mat = r.GetMaterial(sm.materialIndex);
                            var smd = new SubMeshData()
                            {
                                SubMesh = sm,
                                WorldTransform = world,
                                OldWorldTransform = oldWorld,
                                Visible = r.Visible,
                                CastShadow = r.CastShadow,
                                GameObject = obj,
                                Morpher = morpher,
                                Animator = animator,
                            };
                            drawMeshDic[mat.Shader][mat].Add(smd);
                        }
                    }
                }

                findAnimTask.Wait();
                foreach (var task in animTasks) task.Wait();

                foreach (var a in animList) a.UpdateData();
                foreach (var m in morphList) m.UpdateData();

                // DoF,SSAO,MotionBlurを使う場合は深度マップを書き込む
                var dof = GameObject.GetComponent<BokehDoF>();
                var ssao = GameObject.GetComponent<SSAO>();
                var motionBlur = GameObject.GetComponent<MotionBlur>();
                if ((dof != null && dof.Enabled) || (ssao != null && ssao.Enabled) || (motionBlur != null && motionBlur.Enabled))
                {
                    RenderToDepthMap(depthRT, sp, drawMeshDic);
                    if (dof != null && dof.Enabled) dof.DepthMap = depthRT.ColorDst0;
                    if (ssao != null && ssao.Enabled) ssao.DepthMap = depthRT.ColorDst0;
                    if (motionBlur != null && motionBlur.Enabled) motionBlur.DepthMap = depthRT.ColorDst0;
                }

                // エッジを描画する場合はカラーマップを書き込む
                var sobel = GameObject.GetComponent<SobelEdge>();
                if (sobel != null && sobel.Enabled)
                {
                    RenderToColorMap(colorRT, sp, drawMeshDic);
                    sobel.ColorTexture = colorRT.ColorDst0;
                }

                // モーションブラーをする場合は速度マップを書き込む
                if ((motionBlur != null && motionBlur.Enabled))
                {
                    RenderToVelocityMap(velocityRT, sp, drawMeshDic);
                    motionBlur.VelocityMap = velocityRT.ColorDst0;
                }

                // 影の準備
                if (ShadowMapping && MMW.Configuration.ShadowQuality != MMWConfiguration.ShadowQualityType.NoShadow)
                {
                    Vector3 lightDir = MMW.DirectionalLight.WorldDirection;
                    Vector3 center = GameObject.Transform.WorldPosition;
                    Matrix4 view = Matrix4.Identity;
                    Matrix4 projo = Matrix4.Identity;

                    // render to far shadow depth map
                    {
                        view = Matrix4.LookAt(Vector3.Zero, lightDir, Vector3.UnitY);
                        projo = Matrix4.CreateOrthographic(240, 240, -150, 150);
                        sp.viewProj = view * projo;
                        if (prevLightDir != lightDir)
                        {
                            RenderToDepthMap(shadowDepthRT1, sp, drawMeshDic);
                        }
                        sp.shadowDepthBias1 = sp.viewProj * shadowDepthBias;
                        sp.shadowDepthMap1 = shadowDepthRT1.ColorDst0;
                    }

                    //  render to middle shadow depth map
                    {
                        view = Matrix4.LookAt(center, center + lightDir, Vector3.UnitY);
                        projo = Matrix4.CreateOrthographic(40, 40, -150, 150);
                        sp.viewProj = view * projo;
                        RenderToDepthMap(shadowDepthRT2, sp, drawMeshDic);
                        sp.shadowDepthBias2 = sp.viewProj * shadowDepthBias;
                        sp.shadowDepthMap2 = shadowDepthRT2.ColorDst0;
                    }

                    //  render to near shadow depth map
                    {
                        view = Matrix4.LookAt(center, center + lightDir, Vector3.UnitY);
                        projo = Matrix4.CreateOrthographic(10, 10, -80, 80);
                        sp.viewProj = view * projo;
                        RenderToDepthMap(shadowDepthRT3, sp, drawMeshDic);
                        sp.shadowDepthBias3 = sp.viewProj * shadowDepthBias;
                        sp.shadowDepthMap3 = shadowDepthRT3.ColorDst0;
                    }

                    sp.viewProj = viewproj;
                    prevLightDir = lightDir;
                    prevCameraPos = center;
                }
            }

            // レンダーターゲットをセット
            TargetTexture.Bind();
            GL.ClearColor(ClearColor);
            if (ClearFlag == ClearFlag.SolidColor) GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            else if (ClearFlag == ClearFlag.DepthOnly) GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            else if (ClearFlag == ClearFlag.SkyBox)
            {
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
                Drawer.DrawTextureCube(ClearSkyBox, GameObject.Transform.WorldDirectionZ, FoV, Aspect, SkyBoxColor, SkyBoxContrast, SkyBoxSaturation, SkyBoxBrightness);
            }

            // その他スクリプトの描画(メッシュ描画前)
            if (ScriptDraw)
            {
                //DrawScripts(objects, (float)deltaTime, c => c.GameObject.Layer < 16);
            }

            // メッシュ描画
            if (MeshDraw)
            {
                // ライト並び替えタスクが終わるまで待機
                //MMW.lightSortTask.Wait();

                DrawMeshes(sp, drawMeshDic);
            }

            // その他スクリプトの描画
            if (ScriptDraw)
            {
                //DrawScripts(objects, (float)deltaTime, c => c.GameObject.Layer >= 16);
            }

            // イメージエフェクトをかける
            var effects = GameObject.GetComponents<ImageEffect>();
            if (effects != null)
            {
                for (var i = 0; i < effects.Length; i++)
                {
                    if (!effects[i].Enabled) continue;
                    effects[i].Draw(deltaTime);
                }
            }

            // UI層のゲームオブジェクトを描画
            if (UIDraw && layerMask.Get(GameObject.LayerUI))
            {
                //var objs = MMW.FindGameObjects((obj) => obj.Layer == GameObject.LayerUI && layerMask.Get(obj.Layer));
                //DrawScripts(objs, (float)deltaTime, c => true);
            }
        }

        protected void RenderToColorMap(RenderTexture rt, ShaderUniqueParameter sp, Dictionary<Shader, Dictionary<Material, List<SubMeshData>>> sm)
        {
            // レンダーターゲットをセット
            rt.Bind(Color4.Black);
            
            colorShader.UseShader();
            colorShader.SetUniqueParameter(sp, true);

            ushort index = 0;
            foreach (var sh in sm)
            {
                foreach (var mat in sh.Value)
                {
                    foreach (var sub in mat.Value)
                    {
                        if (!sub.Visible) continue;
                        sp.world = sub.WorldTransform;
                        colorShader.SetUniqueParameter(sp, false);
                        colorShader.SetParameter(colorShader.loc_color, ref randColors[index++]);

                        if (sub.Animator != null) sub.Animator.BindMotion(1);
                        else Animator.BindIdentity(1);
                        Drawer.DrawSubMesh(sub.SubMesh);
                        if (sub.Animator != null) sub.Animator.UnbindMotion(1);
                        else Animator.UnbindIdentity(1);
                    }
                }
            }
        }
        protected void RenderToDepthMap(RenderTexture rt, ShaderUniqueParameter sp, Dictionary<Shader, Dictionary<Material, List<SubMeshData>>> sm)
        {
            // レンダーターゲットをセット
            rt.Bind(Color4.White);

            depthShader.UseShader();
            //depthShader.SetUniqueParameter(sp, true);

            foreach (var sh in sm)
            {
                foreach (var mat in sh.Value)
                {
                    foreach (var sub in mat.Value)
                    {
                        if (!sub.Visible || !sub.CastShadow) continue;
                        sp.world = sub.WorldTransform;
                        depthShader.SetUniqueParameter(sp, false);

                        if (sub.Animator != null)
                        {
                            sub.Animator.BindMotion(1);
                            depthShader.SetParameter(depthShader.loc_skin, 1);
                        } 
                        else  depthShader.SetParameter(depthShader.loc_skin, 0);
                        Drawer.DrawSubMesh(sub.SubMesh);
                    }
                }
            }
        }
        protected void RenderToVelocityMap(RenderTexture rt, ShaderUniqueParameter sp, Dictionary<Shader, Dictionary<Material, List<SubMeshData>>> sm)
        {
            // レンダーターゲットをセット
            rt.Bind(Color4.Black);

            velocityShader.UseShader();
            velocityShader.SetUniqueParameter(sp, true);

            foreach (var sh in sm)
            {
                foreach (var mat in sh.Value)
                {
                    foreach (var sub in mat.Value)
                    {
                        if (!sub.Visible) continue;
                        sp.world = sub.WorldTransform;
                        sp.oldWorld = sub.OldWorldTransform;
                        velocityShader.SetUniqueParameter(sp, false);

                        if (sub.Animator != null) sub.Animator.BindMotion(1, 2);
                        else Animator.BindIdentity(1, 2);
                        Drawer.DrawSubMesh(sub.SubMesh);
                        if (sub.Animator != null) sub.Animator.UnbindMotion(1, 2);
                        else Animator.UnbindIdentity(1, 2);
                    }
                }
            }
        }
        protected void DrawMeshes(ShaderUniqueParameter sp, Dictionary<Shader, Dictionary<Material, List<SubMeshData>>> sm)
        {
            foreach (var sh in sm)
            {
                sh.Key.UseShader();
                sh.Key.SetUniqueParameter(sp, true);
                foreach (var mat in sh.Value)
                {
                    mat.Key.ApplyShaderParam();
                    foreach (var sub in mat.Value)
                    {
                        if (!sub.Visible) continue;

                        sp.world = sub.WorldTransform;

                        // ls = MMW.lightSorts[sub.GameObject];
                        //sp.pointLights = ls.first;
                        //sp.spotLights = ls.second;
                        sh.Key.SetUniqueParameter(sp, false);

                        if (sub.Morpher != null) sub.Morpher.UseMorph(0);
                        if (sub.Animator != null) sub.Animator.BindMotion(1);
                        else Animator.BindIdentity(1);
                        Drawer.DrawSubMesh(sub.SubMesh);
                        if (sub.Animator != null) sub.Animator.UnbindMotion(1);
                        else Animator.UnbindIdentity(1);
                        if (sub.Morpher != null) sub.Morpher.UnuseMorph(0);
                    }
                }
                sh.Key.UnuseShader();
            }

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
        protected void DrawScripts(GameObject[] objects, float deltaTime, Predicate<DrawableGameComponent> match)
        {
            Drawer.ClearGraphics();

            for (var i = 0; i < objects.Length; i++)
            {
                var coms = objects[i].GetComponents(match);
                if (coms == null) continue;
                for (var c = 0; c < coms.Length; c++)
                {
                    if (!coms[c].Enabled || coms[c] is MeshRenderer) continue;
                    coms[c].Draw(deltaTime, this);
                }
                if (DebugDraw)
                {
                    for (var c = 0; c < coms.Length; c++)
                    {
                        if (!coms[c].Enabled || coms[c] is MeshRenderer) continue;
                        coms[c].DebugDraw(deltaTime, this);
                    }
                }
            }

            Drawer.EndGraphicsDraw();
        }
        protected virtual void CreateShadowMap()
        {
            var far = new Size(4096, 4096);
            var middle = new Size(2048, 2048);
            var near = new Size(1024, 1024);

            if (MMW.Configuration.ShadowQuality == MMWConfiguration.ShadowQualityType.VeryHigh)
            {
                near = middle;
                middle = far;
            }
            else if (MMW.Configuration.ShadowQuality == MMWConfiguration.ShadowQualityType.Default)
            {
                far = middle;
                //middle = near;
            }
            else if (MMW.Configuration.ShadowQuality == MMWConfiguration.ShadowQualityType.Low)
            {
                far = near;
                middle = near;
                near = new Size(512, 512);
            }
            else if (MMW.Configuration.ShadowQuality == MMWConfiguration.ShadowQualityType.VeryLow)
            {
                far = new Size(512, 512);
                middle = new Size(1024, 1024);
                near = new Size(256, 256);
            }
            shadowDepthRT1 = new RenderTexture(far);
            shadowDepthRT1.ColorFormat0 = PixelInternalFormat.Rgba32f;
            shadowDepthRT1.Load();

            shadowDepthRT2 = new RenderTexture(middle);
            shadowDepthRT2.ColorFormat0 = PixelInternalFormat.Rgba32f;
            shadowDepthRT2.MagFilter = TextureMagFilter.Linear;
            shadowDepthRT2.Load();

            shadowDepthRT3 = new RenderTexture(near);
            shadowDepthRT3.ColorFormat0 = PixelInternalFormat.Rgba32f;
            shadowDepthRT3.MagFilter = TextureMagFilter.Linear;
            shadowDepthRT3.Load();
        }

        protected internal override void OnReceivedMessage(string message, params object[] args)
        {
            if (AutoResize && message == MMWSettings.Default.Message_WindowResize)
            {
                Width = MMW.RenderResolution.Width;
                Height = MMW.RenderResolution.Height;

                depthRT.Size = MMW.RenderResolution;
                colorRT.Size = MMW.RenderResolution;
                velocityRT.Size = MMW.RenderResolution;
            }
        }

        public override GameComponent Clone()
        {
            return new Camera()
            {
                AutoResize = AutoResize,
                ClearColor = ClearColor,
                ClearFlag = ClearFlag,
                Depth = Depth,
                Far = Far,
                FoV = FoV,
                Height = Height,
                Near = Near,
                layerMask = (BitArray)layerMask.Clone(),
                Orthographic = Orthographic,
                targetTexture = targetTexture,
                Up = Up,
                Width = Width,
            };
        }
    }
}
