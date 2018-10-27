using MikuMikuWorld.Scripts.HUD;
using MikuMikuWorldScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;

namespace MikuMikuWorld.Scripts
{
    public class Scripting
    {
        private GameObject obj;
        public Scripting() { }
        public Scripting(GameObject obj)
        {
            this.obj = obj;
        }

        public void Log(object log)
        {
            MMW.BroadcastMessage("log", log.ToString());
        }

        public IGameObject FindGameObject(Predicate<IGameObject> match)
        {
            return MMW.FindGameObject(match);
        }
        public IGameObject[] FindGameObjects(Predicate<IGameObject> match)
        {
            return MMW.FindGameObjects(match);
        }
        public IGameComponent FindGameComponent(Predicate<IGameComponent> match)
        {
            var res = MMW.FindGameComponents<GameComponent>(match);
            if (res == null) return null;
            return res[0];
        }
        public IGameComponent[] FindGameComponents(Predicate<IGameComponent> match)
        {
            var res = MMW.FindGameComponents<GameComponent>(match);
            if (res == null) return null;
            return res;
        }

        public void SwitchOn(int sw, float length = float.MaxValue)
        {
            var player = MMW.FindGameObject(o => o.Tags.Contains("player"));
            MMW.BroadcastMessage("switch on", sw, player.Transform.WorldPosition, length);
        }
        public void SwitchOff(int sw, float length = float.MaxValue)
        {
            var player = MMW.FindGameObject(o => o.Tags.Contains("player"));
            MMW.BroadcastMessage("switch off", sw, player.Transform.WorldPosition, length);
        }
        public void Trigger(int tr, float length = float.MaxValue)
        {
            var player = MMW.FindGameObject(o => o.Tags.Contains("player"));
            MMW.BroadcastMessage("trigger", tr, player.Transform.WorldPosition, length);
        }

        public void AddUpdateAction(Action<double> act, double time = 3.0)
        {
            AddUpdateAction(obj, act, time);
        }
        public void AddUpdateAction(IGameObject go, Action<double> act, double time = 3.0)
        {
            if (go == null) return;

            var obj = go as GameObject;
            if (obj != null && !obj.Destroyed)
            {
                var t = new EventHandler<GameObject.UpdateEventArgs>((s, e) => act(e.deltaTime));
                obj.UpdateAction += t;

                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep((int)(time * 1000.0));

                    if (obj != null && !obj.Destroyed) obj.UpdateAction -= t;
                });
            }
        }

        public void BroadcastMessage(string message, params object[] args)
        {
            MMW.BroadcastMessage(message, args);
        }
        public void SendMessage(string message, params object[] args)
        {
            if (obj != null) MMW.SendMessage(obj, message, args);
        }
    }
}

/*
var coins = FindGameObjects(g => g.Has("tag:coin"));

foreach (var c in coins)
{
    AddUpdateAction(c, (time) =>
    {
        var t = c.FindGameComponents(com => com.Name == "Transform")[0];
        var r = t.Get<Vector3>("Rotate");
        r.Y += (float)time * 2.0f;
        t.Set("Rotate", r);
    }, 20.0f);
}
 */
