using MikuMikuWorld.Assets;
using MikuMikuWorld.GameComponents;
using MikuMikuWorld.GameComponents.Coliders;
using MikuMikuWorld.Scripts.HUD;
using MikuMikuWorld.Networks;
using MikuMikuWorld.Networks.Commands;
using MikuMikuWorld.Physics;
using MikuMikuWorld.Walker;
using MikuMikuWorld.Walker.Network;
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
    class PlayerRayResolver : DrawableGameComponent
    {
        private float distance = 3.0f;
        public float Distance
        {
            get
            {
                return distance + (fp ? 0.0f : (MMW.MainCamera.Transform.WorldPosition - player.Transform.WorldPosition).Length);
            }
            set { distance = value; }
        }

        private List<RayTestResult> rays;
        private RayTestResult closed = null;

        private GameObject player;
        private bool fp = false;
        private PlayerRayData data;

        protected override void OnLoad()
        {
            player = MMW.FindGameObject(o => o.Tags.Contains("player"));
            data = new PlayerRayData();
        }

        protected override void Update(double deltaTime)
        {
            var cam = MMW.MainCamera;
            var from = cam.Transform.WorldPosition;
            var to = from + ((cam.Target - from).Normalized() * Distance);

            rays = Bullet.RayTest(from, to, GameObject);

            var prev = closed;
            closed = null;
            foreach (var r in rays)
            {
                if (r.GameObject.Destroyed) continue;
                closed = r;
                break;
            }

            float rot = 0.0f;
            {
                var t = (cam.Target - from);
                t.Y = 0.0f;
                t.Normalize();
                rot = Vector3.CalculateAngle(-Vector3.UnitZ, t);
                var cross = Vector3.Cross(-Vector3.UnitZ, t);
                rot *= cross.Y >= 0.0f ? -1.0f : 1.0f;
            }

            if (closed != null)
            {
                data.gameobject = closed.GameObject;
                data.position = closed.Position;
                data.normal = closed.Normal;
                data.rotate = new Vector3(0.0f, rot, 0.0f);
                data.distance = Distance * closed.Rate;
            }

            if (prev == null && closed != null)
            {
                MMW.BroadcastMessage("focus enter", closed.GameObject, data);
            }
            else if (prev != null && closed == null)
            {
                MMW.BroadcastMessage("focus leave", prev.GameObject, data);
            }
            else if (prev != null && closed != null && prev != closed)
            {
                MMW.BroadcastMessage("focus leave", prev.GameObject, data);
                MMW.BroadcastMessage("focus enter", closed.GameObject, data);
            }
        }

        protected override void MeshDraw(double deltaTime, Camera camera)
        {
            /*
            if (rays.Count > 0)
            {
                var cam = MMW.MainCamera;
                var from = cam.Transform.WorldPosition;
                var to = (cam.Target - from);
                to.Y = 0.0f;
                to.Normalize();

                var rot = Vector3.CalculateAngle(Vector3.UnitZ, to);
                var cross = Vector3.Cross(Vector3.UnitZ, to);
                rot *= cross.Y >= 0.0f ? -1.0f : 1.0f;

                var model = MatrixHelper.CreateTransform(rays[0].Position + new Vector3(0.0f, 0.125f, 0.0f), new Vector3(0.0f, rot, 0.0f), Vector3.One);
                Drawer.DrawWireframeBox(new Vector3(0.125f), model * camera.ViewProjection, Color4.White, 4.0f);
            }
            */
        }

        protected override void Draw(double deltaTime, Camera camera) { }

        protected override void OnReceivedMessage(string message, params object[] args)
        {
            if (message == "camera change")
            {
                if ((string)args[0] == "first person") fp = true;
                else if ((string)args[0] == "third person") fp = false;
            }
        }
    }

    class PlayerRayData
    {
        public GameObject gameobject;
        public Vector3 normal;
        public Vector3 position;
        public Vector3 rotate;
        public float distance;
    }
}
