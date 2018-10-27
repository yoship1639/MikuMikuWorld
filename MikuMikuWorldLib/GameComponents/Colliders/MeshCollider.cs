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
    public class MeshCollider : Collider
    {
        public override bool ComponentDupulication { get { return true; } }

        private Mesh mesh;
        public Mesh Mesh
        {
            get { return mesh; }
            set { mesh = value; RecreateShape(); }
        }

        private ColliderMesh colMesh;
        public ColliderMesh ColliderMesh
        {
            get { return colMesh; }
            set { colMesh = value; RecreateShape(); }
        }

        public MeshCollider() { }
        public MeshCollider(Mesh mesh) { this.mesh = mesh; }
        public MeshCollider(ColliderMesh colMesh) { this.colMesh = colMesh; }
        public MeshCollider(
            Mesh mesh,
            BulletSharp.CollisionFilterGroups group = BulletSharp.CollisionFilterGroups.DefaultFilter,
            BulletSharp.CollisionFilterGroups mask = BulletSharp.CollisionFilterGroups.AllFilter)
        {
            this.mesh = mesh;
            collideGroup = group;
            collideMask = mask;
        }
        public MeshCollider(
            ColliderMesh colMesh,
            BulletSharp.CollisionFilterGroups group = BulletSharp.CollisionFilterGroups.DefaultFilter,
            BulletSharp.CollisionFilterGroups mask = BulletSharp.CollisionFilterGroups.AllFilter)
        {
            this.colMesh = colMesh;
            collideGroup = group;
            collideMask = mask;
        }

        protected internal override void OnLoad()
        {
            base.OnLoad();

            Physics.CollisionShape shape = null;
            if (mesh == null && colMesh == null) shape = Bullet.CreateBoxShape(Vector3.Zero);
            else if (colMesh != null)
            {
                shape = Bullet.CreateMeshShape(colMesh.Vertices, colMesh.Indices);
            }
            else
            {
                var indices = new List<int>();
                for (var i = 0; i < mesh.SubMeshCount; i++) indices.AddRange(mesh.subMeshes[i].indices);
                shape = Bullet.CreateMeshShape(mesh.Vertices, indices.ToArray());
            }
            collisionObject = Bullet.CreateCollisionObject(shape, CollideGroup, CollideMask);
            collisionObject.tag = this;
        }

        internal override void RecreateShape()
        {
            if (mesh == null && colMesh == null) return;
            Bullet.DestroyShape(collisionObject.Shape);
            if (mesh != null)
            {
                var indices = new List<int>();
                for (var i = 0; i < mesh.SubMeshCount; i++) indices.AddRange(mesh.subMeshes[i].indices);
                collisionObject.Shape = Bullet.CreateMeshShape(mesh.Vertices, indices.ToArray());
            }
            else
            {
                collisionObject.Shape = Bullet.CreateMeshShape(colMesh.Vertices, colMesh.Indices);
            }
        }

        internal override void PhysicalUpdate(double deltaTime)
        {
            base.PhysicalUpdate(deltaTime);
            collisionObject.WorldTransform = MatrixHelper.CreateTransform(Position, Rotate, Scale) * GameObject.Transform.WorldTransform;
        }

        protected internal override void Draw(double deltaTime, Camera camera) { }

        protected internal override void DebugDraw(double deltaTime, Camera camera)
        {
            if (mesh == null) return;
            var world = MatrixHelper.CreateTransform(Position, Rotate, Scale) * GameObject.Transform.WorldTransform;
            var mvp = world * camera.View * camera.Projection;
            var color = Color4.MidnightBlue;
            if (State == ActivationState.Active) color = Color4.LightGreen;
            else if (State == ActivationState.Inactive) color = Color4.DarkSlateGray;
            Drawer.DrawWireframeMesh(mesh, mvp, color);
        }

        public override GameComponent Clone()
        {
            return new MeshCollider()
            {
                mesh = mesh,
                Position = Position,
                Rotate = Rotate,
                Scale = Scale,
                collideGroup = collideGroup,
                collideMask = collideMask,
            };
        }
    }
}
