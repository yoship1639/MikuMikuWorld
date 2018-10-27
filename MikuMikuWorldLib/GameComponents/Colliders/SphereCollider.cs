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
    public class SphereCollider : Collider
    {
        public override bool ComponentDupulication { get { return true; } }

        private float radius = 1.0f;
        public float Radius
        {
            get { return radius; }
            set { radius = value; RecreateShape(); }
        }

        public SphereCollider() { }
        public SphereCollider(float radius)
        {
            this.radius = radius;
        }
        public SphereCollider(
            float radius,
            BulletSharp.CollisionFilterGroups group = BulletSharp.CollisionFilterGroups.DefaultFilter,
            BulletSharp.CollisionFilterGroups mask = BulletSharp.CollisionFilterGroups.AllFilter)
        {
            this.radius = radius;
            collideGroup = group;
            collideMask = mask;
        }

        protected internal override void OnLoad()
        {
            base.OnLoad();

            var shape = Bullet.CreateSphereShape(radius);
            collisionObject = Bullet.CreateCollisionObject(shape, CollideGroup, CollideMask);
            collisionObject.tag = this;
        }

        internal override void RecreateShape()
        {
            Bullet.DestroyShape(collisionObject.Shape);
            collisionObject.Shape = Bullet.CreateSphereShape(radius);
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
            Drawer.DrawWireframeSphere(radius, mvp, color);
        }

        public override GameComponent Clone()
        {
            return new SphereCollider()
            {
                radius = radius,
                Position = Position,
                Rotate = Rotate,
                Scale = Scale,
                collideGroup = collideGroup,
                collideMask = collideMask,
            };
        }
    }
}
