using MikuMikuWorld.Assets;
using MikuMikuWorld.GameComponents;
using MikuMikuWorld.GameComponents.Coliders;
using MikuMikuWorld.Networks;
using MikuMikuWorld.Networks.Commands;
using MikuMikuWorld.Physics;
using MikuMikuWorld.Walker;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Scripts.Player
{
    class TestTransform : DrawableGameComponent
    {
        private Server server;
        private WorldData worldData;

        public TestTransform(Server server)
        {
            this.server = server;
            worldData = MMW.GetAsset<WorldData>();
        }

        protected override void Update(double deltaTime)
        {
            
        }

        protected override void MeshDraw(double deltaTime, Camera camera)
        {
            var player = worldData.Players.Find(p => p.SessionID == server.SessionID);
            if (player == null) return;

            var model = MatrixHelper.CreateTransform(player.Position.FromVec3f() + new Vector3(0.0f, 0.125f, 0.0f), player.Rotation.FromVec3f(), Vector3.One);
            Drawer.DrawWireframeBox(new Vector3(0.125f), model * camera.ViewProjection, Color4.White, 2.0f);
        }

        protected override void Draw(double deltaTime, Camera camera)
        {
            
        }
    }
}
