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
using MikuMikuWorld.GameComponents;
using MikuMikuWorld.GameComponents.ImageEffects;
using MikuMikuWorld.GameComponents.Lights;
using MikuMikuWorld.Importers;
using MikuMikuWorld.Physics;
using MikuMikuWorld.Properties;
using OpenTK;
using OpenTK.Audio.OpenAL;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    public class MMWConfiguration
    {
        public enum AntialiasType
        {
            DoNotAA,
            SSAAHalf,
            SSAAx2,
            SSAAx4,
            FXAA,
        }

        public enum ShadowQualityType
        {
            VeryHigh,
            High,
            Default,
            Low,
            VeryLow,
            NoShadow,
        }

        public enum ShaderType
        {
            Toon,
            FastPhysical,
            Physical,
        }

        public enum IBLQualityType
        {
            VeryHigh,
            High,
            Default,
            Low,
            DoNotIBL,
        }

        public enum AOType
        {
            SSAO,
            DoNotAO,
        }

        public enum BloomType
        {
            Bloom,
            Glare,
            DoNotBloom,
        }

        public enum MotionBlurType
        {
            MotionBlur,
            DoNotMotionBlur,
        }

        public enum DoFType
        {
            DoF,
            DoNotDoF,
        }

        public AntialiasType Antialias = AntialiasType.DoNotAA;
        public ShadowQualityType ShadowQuality = ShadowQualityType.Default;
        public ShaderType Shader = ShaderType.Toon;
        public IBLQualityType IBLQuality = IBLQualityType.Default;
        public AOType AO = AOType.DoNotAO;
        public BloomType Bloom = BloomType.DoNotBloom;
        public MotionBlurType MotionBlur = MotionBlurType.DoNotMotionBlur;
        public DoFType DoF = DoFType.DoNotDoF;
        public bool ToneMapping = false;
        public bool DrawEdge = false;
        public PixelInternalFormat DefaultPixelFormat = PixelInternalFormat.Rgba16f;

        public DisplayDevice DisplayDevice;
    }

    /// <summary>
    /// Miku Miku Worldの処理を統括するスタティッククラス
    /// </summary>
    public static class MMW
    {
        public static readonly OculusRift Rift = new OculusRift();
        public static readonly DisplayDevice RiftDisplay =
            (Enumerable
                .Range((int)DisplayIndex.First, (int)DisplayIndex.Sixth)
                .Select(i => DisplayDevice.GetDisplay(DisplayIndex.First + i))
                .Where(d => d != null && d.Width == Rift.HResolution && d.Height == Rift.VResolution)
                .FirstOrDefault()) ??
            DisplayDevice.Default;
        public static readonly AHMDCamera HMDCamera = new RiftCamera(Rift, RiftDisplay);

        internal static List<GameObject> gameObjects;
        private static GameObject[] updateObjects;
        internal static RenderTexture defaultRenderTarget;
        internal static RenderTexture renderTargetRight;
        internal static RenderTexture renderTargetLeft;
        internal static List<IAsset> assets;
        internal static List<IImporter> importers;

        public static MMWConfiguration Configuration { get; set; } = new MMWConfiguration();

        public static GameWindow Window { get; internal set; }

        public static bool Initialized { get; private set; } = false;
        public static int X { get; internal set; }
        public static int Y { get; internal set; }
        public static float Width => ClientSize.Width;
        public static float Height => ClientSize.Height;
        public static float RenderWidth => RenderResolution.Width;
        public static float RenderHeight => RenderResolution.Height;

        private static ContextHandle ALContext;
        private static IntPtr ALDevice;

        /// <summary>
        /// 画面サイズ
        /// </summary>
        public static Size WindowSize { get; internal set; }

        /// <summary>
        /// 画面描画領域サイズ
        /// </summary>
        public static Size ClientSize { get; internal set; }

        /// <summary>
        /// 描画サイズ
        /// </summary>
        public static Size RenderResolution { get; internal set; }

        /// <summary>
        /// 経過時間
        /// </summary>
        public static double TotalElapsedTime { get; internal set; } = 0.0;

        /// <summary>
        /// メインカメラ
        /// </summary>
        public static Camera MainCamera { get; set; }

        /// <summary>
        /// ディレクショナルライト
        /// </summary>
        public static DirectionalLight DirectionalLight { get; set; }

        /// <summary>
        /// グローバルの環境色
        /// </summary>
        public static Color4 GlobalAmbient { get; set; } = new Color4(0.3f, 0.3f, 0.3f, 0.0f);

        /// <summary>
        /// Image Based Lighting の強さ
        /// </summary>
        public static float IBLIntensity { get; set; } = 0.0f;

        public static float FogIntensity { get; set; } = 1.0f;

        /// <summary>
        /// コントラスト
        /// </summary>
        public static float Contrast { get; set; } = 1.0f;

        /// <summary>
        /// 彩度
        /// </summary>
        public static float Saturation { get; set; } = 1.0f;

        /// <summary>
        /// 明度
        /// </summary>
        public static float Brightness { get; set; } = 1.0f;

        /// <summary>
        /// 重力
        /// </summary>
        public static Vector3 Gravity
        {
            get { return Bullet.Gravity; }
            set { Bullet.Gravity = value; }
        }

        private static float volume = 1.0f;
        public static float SoundVolume
        {
            get { return volume; }
            set { volume = value; AL.Listener(ALListenerf.Gain, value); }
        }

        private static int fpsCount = 0;
        private static double fpsDelta = 0.0;
        private static int fps;
        public static int FPS { get { return fps; } }
        public static int FrameCount { get; private set; }
        public static double DeltaTime { get; private set; }

        public static MasterData MasterData { get; private set; } = new MasterData();

        internal static List<Tuple<Delegate, object[]>> invokes = new List<Tuple<Delegate, object[]>>();
        public static void Invoke(Action act)
        {
            foreach (var del in act.GetInvocationList()) invokes.Add(new Tuple<Delegate, object[]>(del, null));
        }
        public static void Invoke<T>(Action<T> act, T arg1)
        {
            foreach (var del in act.GetInvocationList()) invokes.Add(new Tuple<Delegate, object[]>(del, new object[] { arg1 }));
        }
        public static void Invoke<T1, T2>(Action<T1, T2> act, T1 arg1, T2 arg2)
        {
            foreach (var del in act.GetInvocationList()) invokes.Add(new Tuple<Delegate, object[]>(del, new object[] { arg1, arg2 }));
        }
        public static void Invoke<T1, T2, T3>(Action<T1, T2, T3> act, T1 arg1, T2 arg2, T3 arg3)
        {
            foreach (var del in act.GetInvocationList()) invokes.Add(new Tuple<Delegate, object[]>(del, new object[] { arg1, arg2, arg3 }));
        }


        #region Core

        internal static void Init()
        {
            //Destroy();

            FrameCount = 0;
            gameObjects = new List<GameObject>();
            assets = new List<IAsset>();
            importers = new List<IImporter>();

            // 標準アセットを追加
            {
                // texture
                RegistAsset(new Texture2D(Resources.white, "WhiteMap"));
                RegistAsset(new Texture2D(Resources.black, "BlackMap"));
                RegistAsset(new Texture2D(Resources.defaultNormal, "DefaultNormalMap"));
                RegistAsset(new Texture2D(Resources.toon, "ToonMap"));

                RegistAsset(new Texture2D(Resources.test, "TestMap"));
                RegistAsset(new Texture2D(Resources.test_normal, "TestNormalMap"));

                RegistAsset(new TextureCube(
                    Resources.negx2,
                    Resources.negy2,
                    Resources.negz2,
                    Resources.posx2,
                    Resources.posy2,
                    Resources.posz2,
                    "DefaultSkyBox", false)
                {
                    UseMipmap = true,
                    MinFilter = TextureMinFilter.LinearMipmapLinear,
                    MagFilter = TextureMagFilter.Linear,
                });

                RegistAsset(new TextureCube(
                    Resources.red,
                    Resources.green,
                    Resources.blue,
                    Resources.red,
                    Resources.green,
                    Resources.blue,
                    "TestSkyBox", false)
                {
                    UseMipmap = true,
                    MinFilter = TextureMinFilter.LinearMipmapNearest,
                    MagFilter = TextureMagFilter.Linear
                });

                // shader
                RegistAsset(new ToonShadowShader());
                RegistAsset(new DepthShader());
                RegistAsset(new ShadowShader());
                RegistAsset(new PhysicalShader());
                RegistAsset(new ColorShader());
                RegistAsset(new VelocityShader());
                RegistAsset(new ErrorShader());
                RegistAsset(new DeferredPhysicalShader());
                RegistAsset(new DeferredPhysicalSkinShader());

                // material
                RegistAsset(new Material("Default", GetAsset<Shader>("Physical")));
            }

            // 標準インポータを追加
            {
                importers.Add(new MmwImporter());
                importers.Add(new MqoImporter());
                importers.Add(new PmdImporter());
                importers.Add(new PmxImporter());
                importers.Add(new VmdImporter());
            }

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.TextureCubeMap);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            //glEnable(GL_POLYGON_SMOOTH);
            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            //GL.Enable(EnableCap.StencilTest);
            //GL.ClearStencil(0);
            //GL.StencilMask(~0);

            Drawer.Init();

            Bullet.Init();

            // OpenAL 初期化
            ALDevice = Alc.OpenDevice(null);
            ALContext = Alc.CreateContext(ALDevice, (int[])null);
            Alc.MakeContextCurrent(ALContext);
            SoundVolume = 0.2f;
            AL.DistanceModel(ALDistanceModel.InverseDistance);

            if (!HMDCamera.Connected)
            {
                defaultRenderTarget = new RenderTexture(RenderResolution);
                defaultRenderTarget.MultiBuffer = 1;
                defaultRenderTarget.MagFilter = TextureMagFilter.Linear;
                defaultRenderTarget.ColorFormat0 = Configuration.DefaultPixelFormat;
                defaultRenderTarget.Load();
            }
            else
            {
                renderTargetRight = new RenderTexture(RenderResolution);
                renderTargetRight.Load();

                renderTargetLeft = new RenderTexture(RenderResolution);
                renderTargetLeft.Load();
            }

            // メインカメラを追加
            var camObj = new GameObject("Main Camera", Matrix4.Identity, "Camera");
            var camCom = camObj.AddComponent<DeferredCamera>();
            RegistGameObject(camObj);
            MainCamera = camCom;
            MainCamera.ShadowMapping = Configuration.ShadowQuality != MMWConfiguration.ShadowQualityType.NoShadow;
            if (Configuration.DrawEdge) MainCamera.GameObject.AddComponent<SobelEdge>();
            if (Configuration.AO == MMWConfiguration.AOType.SSAO) MainCamera.GameObject.AddComponent<SSAO>();
            if (Configuration.Antialias == MMWConfiguration.AntialiasType.FXAA) MainCamera.GameObject.AddComponent<FXAA>();
            if (Configuration.DoF == MMWConfiguration.DoFType.DoF) MainCamera.GameObject.AddComponent<BokehDoF>();
            if (Configuration.MotionBlur == MMWConfiguration.MotionBlurType.MotionBlur) MainCamera.GameObject.AddComponent<MotionBlur>();
            if (Configuration.Bloom == MMWConfiguration.BloomType.Bloom) MainCamera.GameObject.AddComponent<Bloom>();
            else if (Configuration.Bloom == MMWConfiguration.BloomType.Glare) MainCamera.GameObject.AddComponent<Glare>();
            if (Configuration.ToneMapping) MainCamera.GameObject.AddComponent<ToneMapping>();
            MainCamera.GameObject.AddComponent<GameComponents.ImageEffects.ColorCollect>();

            // ディレクショナルライトを追加
            var litObj = new GameObject("Directional Light", Matrix4.Identity, "Light");
            var dirLit = litObj.AddComponent<DirectionalLight>();
            dirLit.Intensity = 4.0f;
            dirLit.Color = new Color4(1.0f, 0.94f, 0.86f, 1.0f);
            litObj.Transform.Rotate.X = MathHelper.PiOver4 * 0.5f;
            litObj.Transform.Rotate.Z = MathHelper.PiOver4 * 0.5f;
            litObj.Transform.Position = new Vector3(-2.0f, 2.0f, -2.0f);
            RegistGameObject(litObj);
            DirectionalLight = dirLit;

            Initialized = true;
        }

        internal static void Update(double deltaTime)
        {
            TotalElapsedTime += deltaTime;
            DeltaTime = deltaTime;

            Input.Update();

            updateObjects = gameObjects.ToArray();
            var objNum = updateObjects.Length;

            Task.Run(() =>
            {
                var objects = gameObjects.ToArray();
                for (var o = 0; o < objects.Length; o++)
                {
                    if (objects[o] == null) continue;
                    objects[o].Transform.OldWorldTransfom = objects[o].Transform.WorldTransform;
                }
            });
            
            for (var o = 0; o < objNum; o++) updateObjects[o].BeforeUpdate(deltaTime);
            Bullet.Update((float)deltaTime);
            for (var o = 0; o < objNum; o++) updateObjects[o].PhysicalUpdate(deltaTime);
            for (var o = 0; o < objNum; o++) updateObjects[o].Update(deltaTime);
            for (var o = 0; o < objNum; o++) updateObjects[o].AfterUpdate(deltaTime);

            var invs = invokes.ToArray();
            invokes.Clear();
            foreach (var inv in invs)
            {
                if (inv.Item2 == null) inv.Item1.DynamicInvoke();
                else inv.Item1.DynamicInvoke(inv.Item2);
            }

            FrameCount++;
        }

        internal static void Draw(double deltaTime)
        {
            fpsDelta += deltaTime;
            fpsCount++;
            if (fpsDelta >= 1.0)
            {
                fps = fpsCount;
                fpsDelta -= 1.0;
                fpsCount = 0;
            }

            // 深度の浅いカメラから描画
            
            if (HMDCamera.Connected)
            {
                HMDCamera.CameraType = HMDCameraType.Default;
                var cameras = FindGameComponents<Camera>((cam) => cam.Enabled && cam != MainCamera);
                Array.Sort(cameras, (cam1, cam2) => { return cam1.Depth - cam2.Depth; });
                for (var i = 0; i < cameras.Length; i++) cameras[i].Draw(deltaTime);

                HMDCamera.CameraType = HMDCameraType.Right;
                MainCamera.TargetTexture = renderTargetRight;
                MainCamera.Draw(deltaTime);

                HMDCamera.CameraType = HMDCameraType.Left;
                MainCamera.TargetTexture = renderTargetLeft;
                MainCamera.Draw(deltaTime);

                RenderTexture.Bind(0, ClientSize, Color4.Black);
                Drawer.DrawTexture(renderTargetRight.ColorDst0, new RectangleF(0, 0, 1, 1), new RectangleF(0.5f, 0, 0.5f, 1));
                Drawer.DrawTexture(renderTargetLeft.ColorDst0, new RectangleF(0, 0, 1, 1), new RectangleF(0, 0, 0.5f, 1));

            }
            else
            {
                var cameras = FindGameComponents<Camera>((cam) => cam.Enabled);
                Array.Sort(cameras, (cam1, cam2) => { return cam1.Depth - cam2.Depth; });
                for (var i = 0; i < cameras.Length; i++) cameras[i].Draw(deltaTime);
                RenderTexture.Bind(0, ClientSize, Color4.Black);
                Drawer.DrawTexture(defaultRenderTarget.ColorDst0);
            }
        }

        internal static void Destroy()
        {
            // 全てのゲームオブジェクトを削除する
            DestroyGameObjects((o) => true);

            // 全てのアセットを削除する
            if (assets != null)
            {
                foreach (var a in assets)
                {
                    a.Unload();
                }
                assets = null;
            }
            
            Bullet.Destroy();

            Alc.MakeContextCurrent(ContextHandle.Zero);
            Alc.DestroyContext(ALContext);
            Alc.CloseDevice(ALDevice);

            Rift.Dispose();
        }

        internal static void Resize()
        {
            Drawer.Resize();
            BroadcastMessage(MMWSettings.Default.Message_WindowResize, ClientSize);
        }

        #endregion

        #region GameObject
        /// <summary>
        /// ゲームオブジェクトをMMWに登録する
        /// </summary>
        /// <param name="obj">登録するゲームオブジェクト</param>
        /// <returns></returns>
        public static Result RegistGameObject(GameObject obj)
        {
            if (obj == null) return Result.ObjectIsNull;
            if (obj.Destroyed) return Result.AlreadyDestroyed;
            if (obj.Registered) return Result.AlreadyRegistered;

            obj.Registered = true;
            gameObjects.Add(obj);

            return Result.Success;
        }

        /// <summary>
        /// ゲームオブジェクトをMMWから破棄する
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Result DestroyGameObject(GameObject obj)
        {
            if (obj == null) return Result.ObjectIsNull;

            var res = obj.Destroy();
            if (res == Result.Success)
            {
                gameObjects.Remove(obj);
                return Result.Success;
            }

            return res;
        }

        /// <summary>
        /// ゲームオブジェクトを複数破棄する
        /// </summary>
        /// <param name="match">破棄したいゲームオブジェクトの条件</param>
        /// <returns></returns>
        public static void DestroyGameObjects(Predicate<GameObject> match)
        {
            if (gameObjects == null) return;
            var objs = gameObjects.ToArray();
            for (var o = 0; o < objs.Length; o++)
            {
                if (match(objs[o])) objs[o].Destroy();
            }
            gameObjects.RemoveAll((o) => o.Destroyed);
        }


        public static GameObject[] GetAllGameObject()
        {
            return gameObjects.ToArray();
        }

        /// <summary>
        /// ゲームオブジェクトを検索する
        /// </summary>
        /// <param name="match">検索したいゲームオブジェクトの条件</param>
        /// <returns></returns>
        public static GameObject FindGameObject(Predicate<GameObject> match)
        {
            return gameObjects.Find(match);
        }

        /// <summary>
        /// ゲームオブジェクトを複数検索する
        /// </summary>
        /// <param name="match">検索したいゲームオブジェクトの条件</param>
        /// <returns></returns>
        public static GameObject[] FindGameObjects(Predicate<GameObject> match)
        {
            return gameObjects.FindAll(match).ToArray();
        }

        public static T FindGameComponent<T>() where T : GameComponent
        {
            var objNum = gameObjects.Count;
            for (var o = 0; o < objNum; o++)
            {
                var coms = gameObjects[o].gameComponents;
                var comNum = coms.Count;
                for (var c = 0; c < comNum; c++)
                {
                    if (coms[c] is T) return (T)coms[c];
                }
            }
            return null;
        }

        /// <summary>
        /// ゲームコンポーネントを複数検索する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] FindGameComponents<T>() where T : GameComponent
        {
            List<T> list = new List<T>();
            var objNum = gameObjects.Count;
            for (var o = 0; o < objNum; o++)
            {
                var coms = gameObjects[o].gameComponents;
                var comNum = coms.Count;
                for (var c = 0; c < comNum; c++)
                {
                    if (coms[c] is T) list.Add(coms[c] as T);
                }
            }
            return list.ToArray();
        }

        /// <summary>
        /// ゲームコンポーネントを複数検索する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="match">検索したいゲームコンポーネントの条件</param>
        /// <returns></returns>
        public static T[] FindGameComponents<T>(Predicate<T> match) where T : GameComponent
        {
            List<T> list = new List<T>();
            var objNum = gameObjects.Count;
            for (var o = 0; o < objNum; o++)
            {
                var coms = gameObjects[o].gameComponents;
                var comNum = coms.Count;
                for (var c = 0; c < comNum; c++)
                {
                    if (coms[c] is T && match(coms[c] as T)) list.Add(coms[c] as T);
                }
            }
            return list.ToArray();
        }

        /// <summary>
        /// ゲームコンポーネントを複数検索する
        /// </summary>
        /// <param name="match">検索したいゲームコンポーネントの条件</param>
        /// <returns></returns>
        public static GameComponent[] FindGameComponents(Predicate<GameComponent> match)
        {
            List<GameComponent> list = new List<GameComponent>();
            var objNum = gameObjects.Count;
            for (var o = 0; o < objNum; o++)
            {
                var coms = gameObjects[o].gameComponents;
                var comNum = coms.Count;
                for (var c = 0; c < comNum; c++)
                {
                    if (match(coms[c])) list.Add(coms[c]);
                }
            }
            return list.ToArray();
        }

        /// <summary>
        /// MMWに登録されているゲームオブジェクト全てに対してメッセージを送信する
        /// </summary>
        /// <param name="message"></param>
        public static void BroadcastMessage(string message, params object[] args)
        {
            var objNum = gameObjects.Count;
            for (var i = 0; i < objNum; i++) gameObjects[i].OnReceivedMessage(message, args);
        }

        public static void SendMessage(GameObject obj, string message, params object[] args)
        {
            obj.OnReceivedMessage(message, args);
        }

        public static List<RequestResult<T>> BroadcastRequest<T>(string request, params object[] args)
        {
            var list = new List<RequestResult<T>>();
            var objNum = gameObjects.Count;
            for (var i = 0; i < objNum; i++)
            {
                var res = gameObjects[i].OnReceivedRequest<T>(request, args);
                if (res.Count > 0) list.AddRange(res);
            }
            return list;
        }

        public static T SendRequest<T>(GameObject obj, string request, params object[] args)
        {
            var res = obj.OnReceivedRequest<T>(request, args);
            if (res.Count > 0) return res[0].Value;
            return default(T);
        }

        #endregion

        #region Asset

        /// <summary>
        /// アセットを登録する
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        public static Result RegistAsset(IAsset asset)
        {
            if (assets.Exists((a) => a == asset)) return Result.AlreadyRegistered;
            if (!asset.Loaded)
            {
                var res = asset.Load();
                if (res != Result.Success) return res;
            } 
            assets.Add(asset);
            return Result.Success;
        }

        public static Result DestroyAsset(IAsset asset)
        {
            if (!assets.Exists(a => a == asset)) return Result.ObjectIsNull;
            if (asset.Loaded)
            {
                var res = asset.Unload();
            }
            assets.Remove(asset);
            return Result.Success;
        }

        /// <summary>
        /// アセットを取得する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T GetAsset<T>(string name) where T : IAsset
        {
            var t = typeof(T);
            var asset = assets.Find((a) => (a is T) && (a.Name == name));
            return (T)asset;
        }

        /// <summary>
        /// アセットを取得する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetAsset<T>() where T : IAsset
        {
            var t = typeof(T);
            var asset = assets.Find((a) => a is T);
            return (T)asset;
        }

        /// <summary>
        /// アセットを複数取得する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] GetAssets<T>() where T : IAsset
        {
            var all = assets.FindAll((a) => a is T);
            var res = new T[all.Count];
            for (var i = 0; i < res.Length; i++) res[i] = (T)all[i];
            return res;
        }

        /// <summary>
        /// アセットを複数取得する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] GetAssets<T>(Predicate<T> match) where T : IAsset
        {
            var all = assets.FindAll((a) => (a is T) && match((T)a));
            var res = new T[all.Count];
            for (var i = 0; i < res.Length; i++) res[i] = (T)all[i];
            return res;
        }

        /// <summary>
        /// 対応するインポータを取得する
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static IImporter GetSupportedImporter(string filepath)
        {
            if (!File.Exists(filepath)) return null;

            var ext = Path.GetExtension(filepath);

            return importers.Find((i) => Array.Exists(i.Extensions, (e) => e == ext));
        }

        #endregion
    }

    public class RequestResult<T>
    {
        public GameObject GameObject;
        public GameComponent Component;
        public T Value;

        public RequestResult(GameComponent com, T value)
        {
            GameObject = com.GameObject;
            Component = com;
            Value = value;
        }
    }
}
