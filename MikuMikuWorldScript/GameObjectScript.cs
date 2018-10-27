using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorldScript
{
    public abstract class GameObjectScript
    {
        public static readonly string Version = "1.0";

        /// <summary>
        /// スクリプト名
        /// </summary>
        public abstract string ScriptName { get; }
        /// <summary>
        /// スクリプトの説明
        /// </summary>
        public abstract string ScriptDesc { get; }

        /// <summary>
        /// スクリプトのハッシュ。書き換え禁止
        /// </summary>
        public string ScriptHash { get; set; }

        /// <summary>
        /// 有効状態
        /// </summary>
        public bool Enabled { get; set; } = true;
        /// <summary>
        /// 可視状態
        /// </summary>
        public bool Visible { get; set; } = true;
        /// <summary>
        /// 注目されているか
        /// </summary>
        public bool Focused { get; set; } = false;

        /// <summary>
        /// ゲームオブジェクトを作成した人のユーザID
        /// </summary>
        public string HostUserID { get; set; }
        /// <summary>
        /// ゲームオブジェクトを作成したホストか
        /// </summary>
        public bool IsHost { get; set; }

        /// <summary>
        /// 位置
        /// </summary>
        public Vector3 Position
        {
            get { return Transform.Get<Vector3>("Position"); }
            set { Transform.Set("Position", value); }
        }
        /// <summary>
        /// 回転
        /// </summary>
        public Vector3 Rotate
        {
            get { return Transform.Get<Vector3>("Rotate"); }
            set { Transform.Set("Rotate", value); }
        }
        /// <summary>
        /// 拡大
        /// </summary>
        public Vector3 Scale
        {
            get { return Transform.Get<Vector3>("Scale"); }
            set { Transform.Set("Scale", value); }
        }
        /// <summary>
        /// ローカル姿勢行列
        /// </summary>
        public Matrix4 LocalTransform
        {
            get { return Transform.Get<Matrix4>("LocalTransform"); }
            set { Transform.Set("LocalTransform", value); }
        }
        /// <summary>
        /// ワールド姿勢行列
        /// </summary>
        public Matrix4 WorldTransform
        {
            get { return Transform.Get<Matrix4>("WorldTransform"); }
            set { Transform.Set("WorldTransform", value); }
        }
        /// <summary>
        /// Z方向の向き
        /// </summary>
        public Vector3 DirectionX => Transform.Get<Vector3>("LocalDirectionX");
        /// <summary>
        /// Z方向の向き
        /// </summary>
        public Vector3 DirectionY => Transform.Get<Vector3>("LocalDirectionY");
        /// <summary>
        /// Z方向の向き
        /// </summary>
        public Vector3 DirectionZ => Transform.Get<Vector3>("LocalDirectionZ");
        /// <summary>
        /// 物理姿勢情報を更新する
        /// </summary>
        public void UpdatePhysicalTransform()
        {
            Transform.Exec("UpdatePhysicalTransform");
        }
        
        /// <summary>
        /// プロパティ
        /// </summary>
        public Dictionary<string, string> Properties => GameObject.SendRequest<Dictionary<string, string>>("properties");
        /// <summary>
        /// プロパティを取得する
        /// </summary>
        public string GetProiperty(string name) => GameObject.SendRequest<string>("property", name);

        /// <summary>
        /// MMWの各種データを参照するためのマスターデータ
        /// </summary>
        public IMasterData MasterData { get; set; }

        /// <summary>
        /// このスクリプトが適用されているゲームオブジェクト
        /// </summary>
        public IGameObject GameObject { get; set; }

        /// <summary>
        /// ワールドの情報
        /// </summary>
        public IWorldData WorldData { get; set; }

        private IServer server;
        public IServer Server { set { server = value; } }

        /// <summary>
        /// 親スクリプト
        /// </summary>
        public GameObjectScript ParentScript { get; set; }

        /// <summary>
        /// <para>スクリプトのレイヤ、0 ~ 15 メッシュ描画前、16 ~ 31 メッシュ描画後、32 ~ 47 HUD表示</para>
        /// </summary>
        public virtual int Layer { get; set; } = 32;

        /// <summary>
        /// 姿勢情報のコンポーネント
        /// </summary>
        public IGameComponent Transform => GameObject.FindGameComponents(c => c.Name == "Transform")[0];

        /// <summary>
        /// 更新処理
        /// </summary>
        public virtual void OnUpdate(double deltaTime) { }
        /// <summary>
        /// スクリプトが読み込まれた時に呼ばれる
        /// </summary>
        public virtual void OnLoad(byte[] status) { }
        /// <summary>
        /// スクリプトが破棄される時に呼ばれる
        /// </summary>
        public virtual void OnUnload() { }
        /// <summary>
        /// 描画処理時に呼ばれる
        /// HUD等はここで描画する
        /// </summary>
        public virtual void OnDraw(double deltaTime, IDrawer drawer, ICamera camera) { }
        /// <summary>
        /// メッシュ描画処理時に呼ばれる
        /// </summary>
        public virtual void OnMeshDraw(double deltaTime, IMeshDrawer drawer, ICamera camera) { }

        /// <summary>
        /// サーバにTCPで通信する
        /// </summary>
        /// <param name="dataType">データの種類</param>
        /// <param name="data">データ</param>
        //public void SendTcp(int dataType, byte[] data)
        //{
        //    server.SendTcp(dataType, data);
        //}

        /// <summary>
        /// ゲームオブジェクトの状態を他のプレイヤに伝える。このメソッドはホストのみ有効である
        /// </summary>
        /// <param name="data">ゲームオブジェクトのステータスを含むバイナリデータ</param>
        protected bool UpdateStatus(byte[] data)
        {
            if (!IsHost) return true;

            try
            {
                using (var ms = new MemoryStream())
                {
                    using (var bw = new BinaryWriter(ms))
                    {
                        bw.Write(GameObject.Hash);
                        bw.Write(ScriptHash);
                        bw.Write(data.Length);
                        bw.Write(data);
                    }
                    server.SendTcp(160, ms.ToArray());
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// 更新情報を受け取った時に呼ばれる関数
        /// </summary>
        /// <param name="status">ステータスを表すJSONデータ</param>
        public virtual void OnReceivedUpdateStatus(byte[] status) { }

        /// <summary>
        /// メッセージを受け取った時に呼ばれる
        /// </summary>
        public virtual void OnReceivedMessage(string message, params object[] args) { }

        /// <summary>
        /// 指定した範囲のスイッチをオンにする
        /// </summary>
        /// <param name="value">スイッチの番号</param>
        /// <param name="distance">オンにする半径距離</param>
        protected void SwitchOn(int value, float distance)
        {
            MasterData.BroadcastMessage("switch on", value, Transform.Get<Vector3>("WorldPosition"), distance);
        }
        /// <summary>
        /// 指定した範囲のスイッチをオフにする
        /// </summary>
        /// <param name="value">スイッチの番号</param>
        /// <param name="distance">オンにする半径距離</param>
        protected void SwitchOff(int value, float distance)
        {
            MasterData.BroadcastMessage("switch off", value, Transform.Get<Vector3>("WorldPosition"), distance);
        }
        /// <summary>
        /// スイッチがオンになった時呼ばれる
        /// </summary>
        public virtual void OnSwitchOn(int value) { }
        /// <summary>
        /// スイッチがオフになった時呼ばれる
        /// </summary>
        public virtual void OnSwitchOff(int value) { }

        /// <summary>
        /// 何かに衝突した時に呼ばれる
        /// </summary>
        public virtual void OnCollide(IGameObject obj, Vector3 position, Vector3 normal, float impulse, Vector3 extrusion) { }
        // TODO: Object Collision

        /// <summary>
        /// 焦点があった時に呼ばれる
        /// </summary>
        public virtual void OnFocused() { }
        /// <summary>
        /// 焦点が離れた時に呼ばれる
        /// </summary>
        public virtual void OnDefocused() { }

        /*
        /// <summary>
        /// マウスボタンが押下されている間呼ばれる
        /// </summary>
        /// <param name="btn"></param>
        public virtual void OnMouseButtonDown(MouseButton btn) { }
        /// <summary>
        /// マウスボタンが押された時呼ばれる
        /// </summary>
        /// <param name="btn"></param>
        public virtual void OnMouseButtonPressed(MouseButton btn) { }
        /// <summary>
        /// マウスボタンが離された時呼ばれる
        /// </summary>
        /// <param name="btn"></param>
        public virtual void OnMouseButtonReleased(MouseButton btn) { }
        /// <summary>
        /// キーが押されている間呼ばれる
        /// </summary>
        public virtual void OnKeyDown(Key key) { }
        /// <summary>
        /// キーが押された時呼ばれる
        /// </summary>
        public virtual void OnKeyPressed(Key key) { }
        /// <summary>
        /// キーが離された時呼ばれる
        /// </summary>
        public virtual void OnKeyReleased(Key key) { }
        */
        /// <summary>
        /// 音を再生する
        /// </summary>
        /// <param name="name">音の名前</param>
        /// <param name="volume">音量</param>
        /// <param name="loop">ループ再生</param>
        public void PlaySound(string name, float volume = 1.0f, bool loop = false)
        {
            GameObject.SendMessage("play sound", name, volume, loop);
        }
        /// <summary>
        /// 音を停止する
        /// </summary>
        /// <param name="name">音の名前</param>
        public void StopSound(string name)
        {
            GameObject.SendMessage("stop sound", name);
        }

        /// <summary>
        /// モーションを再生する
        /// </summary>
        /// <param name="name">モーションの名前</param>
        /// <param name="loop">ループ再生</param>
        public void PlayMotion(string name, bool loop = true)
        {
            GameObject.SendMessage("play motion", name, 0.001, loop);
        }
        /// <summary>
        /// モーションを一時停止する
        /// </summary>
        public void PauseMotion()
        {
            GameObject.SendMessage("pause motion");
        }
        /// <summary>
        /// モーションを再開する
        /// </summary>
        public void ResumeMotion()
        {
            GameObject.SendMessage("resume motion");
        }
        /// <summary>
        /// モーションをすべて停止する
        /// </summary>
        public void StopMotion()
        {
            GameObject.SendMessage("stop motion");
        }
        /// <summary>
        /// モーションのスピードを設定する
        /// </summary>
        /// <param name="speed">スピード(1.0fがデフォルト)</param>
        public void SetMotionSpeed(float speed)
        {
            GameObject.SendMessage("set motion speed", speed);
        }

        /// <summary>
        /// モーフィングを設定する
        /// </summary>
        /// <param name="name">モーフ名</param>
        /// <param name="rate">モーフの割合</param>
        public void SetMorphRate(string name, float rate)
        {
            GameObject.SendMessage("set morph", name, rate);
        }
        /// <summary>
        /// モーフィングを設定する
        /// </summary>
        /// <param name="name">モーフ名</param>
        /// <param name="rate">モーフの割合</param>
        public void AddMorphRate(string name, float rate)
        {
            GameObject.SendMessage("add morph", name, rate);
        }

        /// <summary>
        /// マテリアルのパラメータを設定する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="matName">マテリアル名</param>
        /// <param name="paramName">パラメータ名</param>
        /// <param name="value">設定する値</param>
        public void SetMaterial<T>(string matName, string paramName, T value)
        {
            GameObject.SendMessage("set material", matName, paramName, typeof(T), value);
        }
        /// <summary>
        /// 全マテリアルのパラメータを設定する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">パラメータ名</param>
        /// <param name="value">設定する値</param>
        public void SetAllMaterial<T>(string name, T value)
        {
            GameObject.SendMessage("set all material", name, typeof(T), value);
        }
        /// <summary>
        /// マテリアルパラメータを元に戻す
        /// </summary>
        /// <param name="name">パラメータ名</param>
        public void ResetMaterial<T>(string matName, string paramName)
        {
            GameObject.SendMessage("reset material", matName, paramName, typeof(T));
        }
        /// <summary>
        /// 全マテリアルのパラメータを元に戻す
        /// </summary>
        /// <param name="name">パラメータ名</param>
        public void ResetAllMaterial<T>(string name)
        {
            GameObject.SendMessage("reset all material", name, typeof(T));
        }

        /// <summary>
        /// ボーン数を取得する
        /// </summary>
        /// <returns></returns>
        public int GetBoneCount()
        {
            return GameObject.SendRequest<int>("get bone count");
        }
        /// <summary>
        /// ボーン名を取得する
        /// </summary>
        /// <param name="index">ボーン番号</param>
        /// <returns></returns>
        public string GetBoneName(int index)
        {
            return GameObject.SendRequest<string>("get bone name", index);
        }
        /// <summary>
        /// ボーンが存在するか
        /// </summary>
        /// <param name="name">ボーン名</param>
        /// <returns></returns>
        public bool HasBone(string name)
        {
            return GameObject.SendRequest<bool>("has bone", name);
        }
        /// <summary>
        /// ボーンのローカル姿勢行列を取得する
        /// </summary>
        /// <param name="name">ボーン名</param>
        /// <returns></returns>
        public Matrix4 GetBoneLocalTransform(string name)
        {
            return GameObject.SendRequest<Matrix4>("get bone transform", name);
        }
        /// <summary>
        /// ボーンのローカル回転を設定する
        /// </summary>
        /// <param name="name">ボーン名</param>
        /// <param name="rot">回転</param>
        public void SetBoneRotation(string name, Vector3 rot)
        {
            GameObject.SendMessage("set bone rotation", name, rot);
        }
        /// <summary>
        /// ボーンのローカル拡大率を設定する
        /// </summary>
        /// <param name="name">ボーン名</param>
        /// <param name="scale">回転</param>
        public void SetBoneScale(string name, Vector3 scale)
        {
            GameObject.SendMessage("set bone scale", name, scale);
        }

        public void Log(string message)
        {
            MasterData.BroadcastMessage("log", message);
        }

        /// <summary>
        /// オブジェクトを破棄する
        /// </summary>
        protected void Destroy()
        {
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    bw.Write(GameObject.Hash);
                }
                server.SendTcp(170, ms.ToArray());
            }
        }
    }
}
