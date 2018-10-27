using MikuMikuWorldScript;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    public class MasterData : IMasterData
    {
        public int FPS => MMW.FPS;
        public int FrameCount => MMW.FrameCount;
        public double DeltaTime => MMW.DeltaTime;
        public double TotalElapsedTime => MMW.TotalElapsedTime;
        public Vector2 ClientSize => MMW.ClientSize.ToVector2();
        public Vector2 RenderResolution => MMW.RenderResolution.ToVector2();
        public Color4 GlobalAmbient { get { return MMW.GlobalAmbient; } set { MMW.GlobalAmbient = value; } }
        public Vector3 Gravity { get { return MMW.Gravity; } set { MMW.Gravity = value; } }

        public IGameComponent MainCamera => MMW.MainCamera;

        public IAsset GetAsset(string type, string name)
        {
            return MMW.assets.Find(a => a.GetType().Name == type && a.Name == name);
        }
        public IGameComponent[] FindGameComponents(Predicate<IGameComponent> match)
        {
            return MMW.FindGameComponents((Predicate<GameComponent>)match);
        }
        public IGameObject[] FindGameObjects(Predicate<IGameObject> match)
        {
            return MMW.FindGameObjects(match);
        }
        public void BroadcastMessage(string message, params object[] args)
        {
            MMW.BroadcastMessage(message, args);
        }
    }
}
