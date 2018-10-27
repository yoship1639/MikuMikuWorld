using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MikuMikuWorld.Physics;
using MikuMikuWorld.Assets;
using OpenTK.Graphics;

namespace MikuMikuWorld.GameComponents.Coliders
{
    public class PlaneCollider : Collider
    {
        public override bool ComponentDupulication { get { return true; } }

        private float halfExtentsX = 10.0f;
        public float HalfExtentsX
        {
            get { return halfExtentsX; }
            set { halfExtentsX = value;  RecreateShape(); }
        }

        private float halfExtentsZ = 10.0f;
        public float HalfExtentsZ
        {
            get { return halfExtentsZ; }
            set { halfExtentsZ = value; RecreateShape(); }
        }

        public PlaneCollider() { }
        public PlaneCollider(
            float halfExtentsX,
            float halfExtentsZ)
        {
            this.halfExtentsX = halfExtentsX;
            this.halfExtentsZ = halfExtentsZ;
        }
        public PlaneCollider(
            float halfExtentsX,
            float halfExtentsZ,
            BulletSharp.CollisionFilterGroups group = BulletSharp.CollisionFilterGroups.DefaultFilter,
            BulletSharp.CollisionFilterGroups mask = BulletSharp.CollisionFilterGroups.AllFilter)
        {
            this.halfExtentsX = halfExtentsX;
            this.halfExtentsZ = halfExtentsZ;
            collideGroup = group;
            collideMask = mask;
        }

        private Mesh debugMesh;

        protected internal override void OnLoad()
        {
            base.OnLoad();

            var planeMesh = Mesh.CreateSimplePlaneMesh(halfExtentsX, halfExtentsZ);
            var shape = Bullet.CreateMeshShape(planeMesh.Vertices, planeMesh.subMeshes[0].indices);
            collisionObject = Bullet.CreateCollisionObject(shape, CollideGroup, CollideMask);
            collisionObject.tag = this;

            debugMesh = Mesh.CreateSimplePlaneMesh(1.0f, 1.0f, true);
            debugMesh.Load();
        }

        internal override void RecreateShape()
        {
            var planeMesh = Mesh.CreateSimplePlaneMesh(halfExtentsX, halfExtentsZ);
            collisionObject.Shape = Bullet.CreateMeshShape(planeMesh.Vertices, planeMesh.subMeshes[0].indices);
        }

        internal override void PhysicalUpdate(double deltaTime)
        {
            base.PhysicalUpdate(deltaTime);
            collisionObject.WorldTransform = MatrixHelper.CreateTransform(Position, Rotate, Scale) * GameObject.Transform.WorldTransform;
        }

        protected internal override void Draw(double deltaTime, Camera camera) { }

        protected internal override void DebugDraw(double deltaTime, Camera camera)
        {
            var world = MatrixHelper.CreateTransform(Position, Rotate, new Vector3(halfExtentsX, 1.0f, halfExtentsZ) * Scale) * GameObject.Transform.WorldTransform;
            var mvp = world * camera.View * camera.Projection;
            var color = Color4.MidnightBlue;
            if (State == ActivationState.Active) color = Color4.LightGreen;
            else if (State == ActivationState.Inactive) color = Color4.DarkSlateGray;
            Drawer.DrawWireframeMesh(debugMesh, mvp, color);
        }

        public override GameComponent Clone()
        {
            return new PlaneCollider()
            {
                halfExtentsX = halfExtentsX,
                halfExtentsZ = halfExtentsZ,
                Position = Position,
                Rotate = Rotate,
                Scale = Scale,
                collideGroup = collideGroup,
                collideMask = collideMask,
            };
        }
    }
}
