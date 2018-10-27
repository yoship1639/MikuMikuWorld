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
using OpenTK;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    /// <summary>
    /// Miku Miku World に登場するオブジェクトのベースとなるクラス
    /// </summary>
    public class GameObject : IGameObject
    {
        public static readonly int LayerAfterRender = 16;
        public static readonly int LayerUI = 32;

        /// <summary>
        /// ゲームオブジェクトの名前
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ゲームオブジェクトが有効か
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                enabled = value;
                foreach (var com in gameComponents) com.OnGameObjectEnabledChanged(value);
            }
        }
        private bool enabled = true;

        /// <summary>
        /// ゲームオブジェクトの姿勢情報
        /// </summary>
        public Transform Transform { get; private set; }
        IGameComponent IGameObject.Transform => Transform;

        /// <summary>
        /// ゲームオブジェクトがMMWから破棄されているか
        /// </summary>
        public bool Destroyed { get; internal set; } = false;

        /// <summary>
        /// ゲームオブジェクトがMMWに登録されているか
        /// </summary>
        public bool Registered { get; internal set; } = false;

        /// <summary>
        /// ゲームオブジェクトが属しているレイヤ
        /// </summary>
        //public int Layer { get; set; } = 0;

        public string Hash { get; set; }

        /// <summary>
        /// タグ
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// プロパティ
        /// </summary>
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        public class UpdateEventArgs : EventArgs
        {
            public double deltaTime;
        }

        private UpdateEventArgs updateEventArgs = new UpdateEventArgs();
        public event EventHandler<UpdateEventArgs> BeforeUpdateAction = delegate { };
        public event EventHandler<UpdateEventArgs> UpdateAction = delegate { };
        public event EventHandler<UpdateEventArgs> AfterUpdateAction = delegate { };

        //internal List<string> tags = new List<string>();
        internal List<GameComponent> gameComponents = new List<GameComponent>();
        private List<GameComponent> updateComponents = new List<GameComponent>(256);

        public GameObject() : this("GameObject") { }
        public GameObject(string name) : this(name, Matrix4.Identity) { }
        public GameObject(string name, params string[] tags) : this(name, Matrix4.Identity, tags) { }
        public GameObject(string name, Matrix4 transform, params string[] tags)
        {
            Name = name;
            Hash = Util.CreateHash();
            if (Tags != null) Tags.AddRange(tags);
            Transform = AddComponent<Transform>();
            Transform.LocalTransform = transform;
        }
        public GameObject(GameObject src)
        {
            Name = src.Name;
            Hash = Util.CreateHash();
            Tags = src.Tags.ToList();
            //Layer = src.Layer;
            Enabled = src.Enabled;

            // コンポーネントのコピー
            foreach (var c in src.gameComponents)
            {
                var com = c.Clone();
                com.GameObject = this;
                com.Enabled = c.Enabled;
                com.Destroyed = c.Destroyed;
                com.Tags = c.Tags.ToList();
                com.OnLoad();
                gameComponents.Add(com);
            }

            Transform = GetComponent<Transform>();
        }

        /// <summary>
        /// 全てのコンポーネントのUpdateが呼ばれる前に呼ばれるメソッド
        /// </summary>
        /// <param name="deltaTime">前フレームから経過した時間</param>
        internal void BeforeUpdate(double deltaTime)
        {
            if (!Enabled) return;

            updateEventArgs.deltaTime = deltaTime;
            BeforeUpdateAction(this, updateEventArgs);

            updateComponents.Clear();
            updateComponents.AddRange(gameComponents);
            var comNum = updateComponents.Count;
            for (int c = 0; c < comNum; c++)
            {
                if (updateComponents[c].Destroyed || !updateComponents[c].Enabled) continue;
                updateComponents[c].BeforeUpdate(deltaTime);
            } 
        }

        internal void PhysicalUpdate(double deltaTime)
        {
            if (!Enabled) return;
            var comNum = updateComponents.Count;
            for (int c = 0; c < comNum; c++)
            {
                var physical = updateComponents[c] as PhysicalGameComponent;
                if (physical != null) physical.PhysicalUpdate(deltaTime);
            } 
        }

        /// <summary>
        /// コンポーネントの更新処理を行うメソッド
        /// </summary>
        /// <param name="deltaTime">前フレームから経過した時間</param>
        internal void Update(double deltaTime)
        {
            if (!Enabled) return;

            UpdateAction(this, updateEventArgs);

            var comNum = updateComponents.Count;
            for (int c = 0; c < comNum; c++)
            {
                if (updateComponents[c].Destroyed || !updateComponents[c].Enabled) continue;
                updateComponents[c].Update(deltaTime);
            } 
        }

        /// <summary>
        /// 全てのコンポーネントのUpdateが呼ばれた後に呼ばれるメソッド
        /// </summary>
        /// <param name="deltaTime">前フレームから経過した時間</param>
        internal void AfterUpdate(double deltaTime)
        {
            if (!Enabled) return;

            AfterUpdateAction(this, updateEventArgs);

            var comNum = updateComponents.Count;
            for (int c = 0; c < comNum; c++)
            {
                if (updateComponents[c].Destroyed || !updateComponents[c].Enabled) continue;
                updateComponents[c].AfterUpdate(deltaTime);
            } 
        }

        /// <summary>
        /// ゲームオブジェクトに新しくコンポーネントを追加する
        /// </summary>
        /// <typeparam name="T">追加したいコンポーネントの型</typeparam>
        /// <returns>新しく追加されたコンポーネント</returns>
        public T AddComponent<T>(params object[] args) where T : GameComponent
        {
            var t = typeof(T);
            var component = t.InvokeMember(t.Name, System.Reflection.BindingFlags.CreateInstance, null, null, args) as T;
            if (!component.ComponentDupulication)
            {
                if (GetComponent<T>() != null) return null;
            }
            component.GameObject = this;
            gameComponents.Add(component);
            component.OnLoad();
            foreach (var c in gameComponents)
            {
                if (c == component) continue;
                c.OnGameComponentAdded(component);
            }
            return component;
        }

        /// <summary>
        /// ゲームオブジェクトが保持するコンポーネントを取得する
        /// </summary>
        /// <typeparam name="T">取得したいコンポーネントの型</typeparam>
        /// <returns></returns>
        public T GetComponent<T>() where T : GameComponent
        {
            return this.gameComponents.Find((c) => c is T) as T;
        }

        /// <summary>
        /// ゲームオブジェクトが保持するコンポーネントを取得する
        /// </summary>
        /// <typeparam name="T">取得したいコンポーネントの型</typeparam>
        /// <param name="match">取得したいゲームコンポーネントの条件</param>
        /// <returns></returns>
        public T GetComponent<T>(Predicate<T> match) where T : GameComponent
        {
            return this.gameComponents.Find((c) =>
            {
                var com = c as T;
                return com != null && match(com);
            }) as T;
        }

        /// <summary>
        /// ゲームオブジェクトが保持するコンポーネントを取得する
        /// </summary>
        /// <param name="index">取得したいコンポーネントの番号</param>
        /// <returns></returns>
        public GameComponent GetComponent(int index)
        {
            if (index < 0 || index >= gameComponents.Count) return null;
            return gameComponents[index];
        }

        /// <summary>
        /// ゲームオブジェクトが保持するコンポーネントをすべて取得する
        /// </summary>
        /// <returns></returns>
        public GameComponent[] GetComponents()
        {
            return gameComponents.ToArray();
        }

        /// <summary>
        /// ゲームオブジェクトが保持するコンポーネントを複数取得する
        /// </summary>
        /// <typeparam name="T">取得したいコンポーネントの型</typeparam>
        /// <returns></returns>
        public T[] GetComponents<T>() where T : GameComponent
        {
            var list = this.gameComponents.FindAll((c) => c is T);
            var array = new T[list.Count];
            for (var i = 0; i < array.Length; i++) array[i] = (T)list[i];
            return array;
        }

        /// <summary>
        /// ゲームオブジェクトが保持するコンポーネントを複数取得する
        /// </summary>
        /// <typeparam name="T">取得したいコンポーネントの型</typeparam>
        /// <param name="match">取得したいゲームコンポーネントの条件</param>
        /// <returns></returns>
        public T[] GetComponents<T>(Predicate<T> match) where T : GameComponent
        {
            var list = this.gameComponents.FindAll((c) =>
            {
                var com = c as T;
                return com != null && match(com);
            });
            var array = new T[list.Count];
            for (var i = 0; i < array.Length; i++) array[i] = (T)list[i];
            return array;
        }

        /// <summary>
        /// コンポーネントを所持しているか
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool HasComponent<T>() where T : GameComponent
        {
            return gameComponents.Find((c) => c is T) != null;
        }

        /// <summary>
        /// コンポーネントを所持しているか
        /// </summary>
        /// <param name="com"></param>
        /// <returns></returns>
        public bool HasComponent(GameComponent com)
        {
            return gameComponents.Exists((c) => c == com);
        }

        /// <summary>
        /// 指定のコンポーネントを削除する
        /// </summary>
        /// <param name="com"></param>
        /// <returns></returns>
        public Result RemoveComponent(GameComponent com)
        {
            if (com.Destroyed) return Result.AlreadyDestroyed;
            if (!HasComponent(com)) return Result.Success;

            com.Destroy();
            gameComponents.Remove(com);

            return Result.Success;
        }

        /// <summary>
        /// ゲームオブジェクトを破棄する
        /// </summary>
        /// <returns></returns>
        internal Result Destroy()
        {
            if (Destroyed) return Result.AlreadyDestroyed;

            foreach (var c in this.gameComponents.ToArray()) c.Destroy();
            this.gameComponents.Clear();

            Destroyed = true;
            Registered = false;

            return Result.Success;
        }

        /// <summary>
        /// メッセージを受け取ったときに呼ばれるメソッド
        /// </summary>
        /// <param name="message">受け取ったメッセージ</param>
        /// <param name="args">メッセージの引数</param>
        internal protected virtual void OnReceivedMessage(string message, params object[] args)
        {
            if (Destroyed) return;
            var comNum = gameComponents.Count;
            for (var c = 0; c < comNum; c++)
            {
                gameComponents[c].OnReceivedMessage(message, args);
            } 
        }

        internal protected virtual List<RequestResult<T>> OnReceivedRequest<T>(string request, params object[] args)
        {
            if (Destroyed) return null;

            var list = new List<RequestResult<T>>();
            var comNum = gameComponents.Count;
            for (var c = 0; c < comNum; c++)
            {
                var res = gameComponents[c].OnReceivedRequest<T>(request, args);
                if (res != null) list.Add(res);
            }
            return list;
        }

        public T SendRequest<T>(string request, params object[] args)
        {
            try
            {
                return MMW.SendRequest<T>(this, request, args);
            }
            catch { }

            return default(T);
        }

        public bool SendMessage(string message, params object[] args)
        {
            try
            {
                MMW.SendMessage(this, message, args);
                return true;
            }
            catch { }

            return false;
        }

        #region Script
        public IGameComponent[] FindGameComponents(Predicate<IGameComponent> match)
        {
            return gameComponents.FindAll(match).ToArray();
        }

        private Dictionary<string, Func<GameObject, object>> getter = new Dictionary<string, Func<GameObject, object>>()
        {
            {"ComponentCount", (obj) => obj.gameComponents.Count },
            {"TagCount", (obj) => obj.Tags.Count },
            {"Enabled", (obj) => obj.enabled },
            {"Name", (obj) => obj.Name },
            //{"Layer", (obj) => obj.Layer },
        };
        public object Get(string name)
        {
            Func<GameObject, object> func;
            if (getter.TryGetValue(name, out func))
            {
                return func(this);
            }
            return null;
        }
        public T Get<T>(string name)
        {
            Func<GameObject, object> func;
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

        public bool Has(string name)
        {
            var idx = name.IndexOf("tag:");
            if (idx == 0)
            {
                var tag = name.Remove(0, 4);
                return Tags.Contains(tag);
            }
            return false;
        }

        private Dictionary<string, Action<GameObject, object>> setter = new Dictionary<string, Action<GameObject, object>>()
        {
            {"Enabled", (obj, value) => obj.Enabled = (bool)value },
        };
        public bool Set(string name, object value)
        {
            Action<GameObject, object> func;
            if (setter.TryGetValue(name, out func))
            {
                func(this, value);
            }
            return false;
        }

        private Dictionary<string, Func<GameObject, object[], object>> execs = new Dictionary<string, Func<GameObject, object[], object>>()
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
            Func<GameObject, object[], object> f;
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

        void IGameObject.SendMessage(string message, params object[] args)
        {
            MMW.SendMessage(this, message, args);
        }

        #endregion
    }
}
