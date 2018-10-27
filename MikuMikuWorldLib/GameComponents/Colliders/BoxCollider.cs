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
    public class BoxCollider : Collider
    {
        public override bool ComponentDupulication { get { return true; } }

        private Vector3 boxHalfExtents = new Vector3(0.5f, 0.5f, 0.5f);
        public Vector3 BoxHalfExtents
        {
            get { return boxHalfExtents; }
            set { boxHalfExtents = value; RecreateShape(); }
        }

        public BoxCollider() { }
        public BoxCollider(Vector3 halfExtents) { this.boxHalfExtents = halfExtents; }
        public BoxCollider(
            Vector3 halfExtents,
            BulletSharp.CollisionFilterGroups group = BulletSharp.CollisionFilterGroups.DefaultFilter,
            BulletSharp.CollisionFilterGroups mask = BulletSharp.CollisionFilterGroups.AllFilter)
        {
            this.boxHalfExtents = halfExtents;
            collideGroup = group;
            collideMask = mask;
        }

        protected internal override void OnLoad()
        {
            base.OnLoad();

            var shape = Bullet.CreateBoxShape(BoxHalfExtents);
            collisionObject = Bullet.CreateCollisionObject(shape, CollideGroup, CollideMask);
            collisionObject.tag = this;
        }

        internal override void RecreateShape()
        {
            Bullet.DestroyShape(collisionObject.Shape);
            collisionObject.Shape = Bullet.CreateBoxShape(BoxHalfExtents);
        }

        internal override void PhysicalUpdate(double deltaTime)
        {
            base.PhysicalUpdate(deltaTime);
            collisionObject.WorldTransform = MatrixHelper.CreateTransform(Position, Rotate, Scale) * GameObject.Transform.WorldTransform;
        }

        protected internal override void Draw(double deltaTime, Camera camera) { }

        protected internal override void DebugDraw(double deltaTime, Camera camera)
        {
            var world = MatrixHelper.CreateTransform(Position, Rotate, Scale) * GameObject.Transform.WorldTransform;
            var mvp = world * camera.View * camera.Projection;
            var color = Color4.MidnightBlue;
            if (State == ActivationState.Active) color = Color4.LightGreen;
            else if (State == ActivationState.Inactive) color = Color4.DarkSlateGray;
            Drawer.DrawWireframeBox(boxHalfExtents, mvp, color);
        }

        public override GameComponent Clone()
        {
            return new BoxCollider()
            {
                boxHalfExtents = boxHalfExtents,
                Position = Position,
                Rotate = Rotate,
                Scale = Scale,
                collideGroup = collideGroup,
                collideMask = collideMask,
            };
        }
    }
}
