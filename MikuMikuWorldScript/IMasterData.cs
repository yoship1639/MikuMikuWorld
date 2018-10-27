using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorldScript
{
    public interface IMasterData
    {
        int FPS { get; }
        int FrameCount { get; }
        double DeltaTime { get; }
        double TotalElapsedTime { get; }
        Vector2 ClientSize { get; }
        Vector2 RenderResolution { get; }
        Color4 GlobalAmbient { get; set; }
        Vector3 Gravity { get; set; }

        IGameComponent MainCamera { get; }

        IAsset GetAsset(string type, string name);
        IGameObject[] FindGameObjects(Predicate<IGameObject> match);
        IGameComponent[] FindGameComponents(Predicate<IGameComponent> match);

        void BroadcastMessage(string message, params object[] args);
    }
}
