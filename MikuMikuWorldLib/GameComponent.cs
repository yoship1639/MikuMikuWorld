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

using MikuMikuWorld.GameComponents;
using MikuMikuWorldScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    /// <summary>
    /// GameObjectに追加されるコンポーネントのベースとなるクラス
    /// </summary>
    public abstract class GameComponent : IGameComponent
    {
        public static readonly int LayerAfterMeshRender = 16;
        public static readonly int LayerUI = 32;

        public virtual int[] TcpAcceptDataTypes { get; protected set; } = new int[0];
        public virtual int[] UdpAcceptDataTypes { get; protected set; } = new int[0];

        /// <summary>
        /// 同じコンポーネントをゲームオブジェクトに複数追加することができるか
        /// </summary>
        public virtual bool ComponentDupulication { get { return true; } }

        public virtual int Layer { get; set; }

        /// <summary>
        /// ゲームコンポーネントが有効か
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; OnGameComponentEnabledChanged(value); }
        }
        private bool enabled = true;

        /// <summary>
        /// コンポーネントが破棄されているか
        /// </summary>
        public bool Destroyed { get; internal set; } = false;

        /// <summary>
        /// このコンポーネントを保持しているゲームオブジェクト
        /// </summary>
        public GameObject GameObject { get; internal set; }

        public Transform Transform => GameObject.Transform;

        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// コンポーネントが生成されたときに呼ばれるメソッド
        /// </summary>
        internal protected virtual void OnLoad() { }

        /// <summary>
        /// コンポーネントが破棄された時に呼ばれるメソッド
        /// </summary>
        internal protected virtual void OnUnload() { }

        /// <summary>
        /// 全てのコンポーネントのUpdateが呼ばれる前に呼ばれるメソッド
        /// </summary>
        /// <param name="deltaTime">前フレームから経過した時間</param>
        protected internal virtual void BeforeUpdate(double deltaTime) { }

        /// <summary>
        /// コンポーネントの更新処理を行うメソッド
        /// </summary>
        /// <param name="deltaTime">前フレームから経過した時間</param>
        protected internal virtual void Update(double deltaTime) { }

        /// <summary>
        /// 全てのコンポーネントのUpdateが呼ばれた後に呼ばれるメソッド
        /// </summary>
        /// <param name="deltaTime">前フレームから経過した時間</param>
        protected internal virtual void AfterUpdate(double deltaTime) { }

        /// <summary>
        /// ゲームオブジェクトにコンポーネントが追加されたときに呼ばれる
        /// </summary>
        /// <param name="com"></param>
        protected internal virtual void OnGameComponentAdded(GameComponent com) { }

        /// <summary>
        /// コンポーネントの有効が変更されたときに呼ばれる
        /// </summary>
        /// <param name="enabled"></param>
        protected internal virtual void OnGameComponentEnabledChanged(bool enabled) { }

        /// <summary>
        /// ゲームオブジェクトの有効が変更されたときに呼ばれる
        /// </summary>
        /// <param name="enabled"></param>
        protected internal virtual void OnGameObjectEnabledChanged(bool enabled) { }

        /// <summary>
        /// メッセージを受け取ったときに呼ばれるメソッド
        /// </summary>
        /// <param name="message">受け取ったメッセージ</param>
        /// <param name="args">メッセージの引数</param>
        internal protected virtual void OnReceivedMessage(string message, params object[] args) { }

        internal protected virtual RequestResult<T> OnReceivedRequest<T>(string request, params object[] args) => null;

        /// <summary>
        /// TCPのデータを受け取った時に呼ばれる
        /// </summary>
        /// <param name="dataType">受け取ったデータの種類</param>
        /// <param name="data">受け取ったTCPデータ</param>
        public virtual void OnTcpReceived(int dataType, byte[] data) { }

        /// <summary>
        /// UDPのデータを受け取った時に呼ばれる
        /// </summary>
        /// <param name="dataType">受け取ったデータの種類</param>
        /// <param name="data">受け取ったUDPデータ</param>
        public virtual void OnUdpReceived(int dataType, byte[] data) { }

        /// <summary>
        /// コンポーネントを破棄する
        /// </summary>
        public Result Destroy()
        {
            if (Destroyed) return Result.AlreadyDestroyed;

            OnUnload();
            GameObject.gameComponents.Remove(this);
            Destroyed = true;

            return Result.Success;
        }

        public virtual GameComponent Clone() { return null; }

        #region Script
        public virtual string Name { get; protected set; } = "Component";

        IGameObject IGameComponent.GameObject => GameObject;

        protected Dictionary<string, Func<GameComponent, object>> getter = new Dictionary<string, Func<GameComponent, object>>()
        {
            {"TagCount", (obj) => obj.Tags.Count },
            {"Enabled", (obj) => obj.enabled },
            {"Name", (obj) => obj.Name },
        };
        public T Get<T>(string name)
        {
            Func<GameComponent, object> func;
            if (getter.TryGetValue(name, out func))
            {
                try
                {
                    return (T)func(this);
                }
                catch { }
            }
            return default(T);
        }

        public virtual bool Has(string name)
        {
            var idx = name.IndexOf("tag:");
            if (idx == 0)
            {
                var tag = name.Remove(0, 4);
                return Tags.Contains(tag);
            }
            return false;
        }

        protected Dictionary<string, Action<GameComponent, object>> setter = new Dictionary<string, Action<GameComponent, object>>()
        {
            {"Enabled", (obj, value) => obj.Enabled = (bool)value },
        };
        public bool Set(string name, object value)
        {
            Action<GameComponent, object> func;
            if (setter.TryGetValue(name, out func))
            {
                try
                {
                    func(this, value);
                    return true;
                }
                catch { }
            }
            return false;
        }

        protected Dictionary<string, Func<GameComponent, object[], object>> execs = new Dictionary<string, Func<GameComponent, object[], object>>()
        {
            {"AddTag", (obj, args) =>
            {
                string tag = args[0] as string;
                obj.Tags.Add(tag);
                return true;
            } },
            {"RemoveTag", (obj, args) =>
            {
                string tag = args[0] as string;
                return obj.Tags.Remove(tag);
            } },
        };
        public object Exec(string func, params object[] param)
        {
            Func<GameComponent, object[], object> f;
            if (execs.TryGetValue(func, out f))
            {
                try
                {
                    return f(this, param);
                }
                catch { }
            }
            return false;
        }
        #endregion
    }

    /// <summary>
    /// 描画可能なゲームコンポーネント
    /// </summary>
    public abstract class DrawableGameComponent : GameComponent
    {
        protected internal virtual void MeshDraw(double deltaTime, Camera camera) { }
        protected internal abstract void Draw(double deltaTime, Camera camera);
        protected internal virtual void DebugDraw(double deltaTime, Camera camera) { }
    }

    /// <summary>
    /// 物理に関するゲームコンポーネント
    /// </summary>
    public abstract class PhysicalGameComponent : DrawableGameComponent
    {
        internal abstract void PhysicalUpdate(double deltaTime);
        internal void OnCollision(GameObject obj, BulletSharp.ManifoldPoint[] mps)
        {
            Collide(this, new Collision()
            {
                GameObject = obj,
                ManifoldPoints = mps
            });
        }

        public event EventHandler<Collision> Collide = delegate { };
    }
}
