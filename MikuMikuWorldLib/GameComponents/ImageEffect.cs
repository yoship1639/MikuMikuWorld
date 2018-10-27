using MikuMikuWorld.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.GameComponents
{
    public abstract class ImageEffect : GameComponent
    {
        public override bool ComponentDupulication { get { return true; } }
        public abstract void Draw(double deltaTime);
        public virtual RequireMap[] RequireMaps { get; } = new RequireMap[0];

        public Texture2D DepthMap;
        public Texture2D PositionMap;
        public Texture2D NormalMap;
        public Texture2D VelocityMap;
        public Texture2D ShadowMap;
    }

    public enum RequireMap
    {
        Depth,
        Velocity,
        Albedo,
        Normal,
        Position,
        Shadow,
    }
}
