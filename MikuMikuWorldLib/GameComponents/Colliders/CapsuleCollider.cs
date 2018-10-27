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
    public class CapsuleCollider : Collider
    {
        public override bool ComponentDupulication { get { return true; } }

        private float radius = 0.5f;
        public float Radius
        {
            get { return radius; }
            set { radius = value; RecreateShape(); }
        }

        private float height = 1.0f;
        public float Height
        {
            get { return height; }
            set { height = value; RecreateShape(); }
        }

        public CapsuleCollider() { }
        public CapsuleCollider(float radius, float height)
        {
            this.radius = radius;
            this.height = height;
        }
        public CapsuleCollider(
            float radius,
            float height,
            BulletSharp.CollisionFilterGroups group = BulletSharp.CollisionFilterGroups.DefaultFilter,
            BulletSharp.CollisionFilterGroups mask = BulletSharp.CollisionFilterGroups.AllFilter)
        {
            this.radius = radius;
            this.height = height;
            collideGroup = group;
            collideMask = mask;
        }

        protected internal override void OnLoad()
        {
            base.OnLoad();

            var shape = Bullet.CreateCapsuleShape(radius, height);
            collisionObject = Bullet.CreateCollisionObject(shape, CollideGroup, CollideMask);
            collisionObject.tag = this;
        }

        internal override void RecreateShape()
        {
            Bullet.DestroyShape(collisionObject.Shape);
            collisionObject.Shape = Bullet.CreateCapsuleShape(radius, height);
        }

        internal override void PhysicalUpdate(double deltaTime)
        {
            base.PhysicalUpdate(deltaTime);
            collisionObject.WorldTransform = MatrixHelper.CreateTransform(Position, Rotate, Scale) * GameObject.Transform.WorldTransform;
        }

        protected internal override void Draw(double deltaTime, Camera camera) { }

        protected internal override void DebugDraw(double deltaTime, Camera camera)
        {
            var color = Color4.MidnightBlue;
            if (State == ActivationState.Active) color = Color4.LightGreen;
            else if (State == ActivationState.Inactive) color = Color4.DarkSlateGray;
            var t = Matrix4.CreateTranslation(Vector3.UnitY * height * 0.5f);
            var wvp = MatrixHelper.CreateTransform(Position, Rotate, Scale) * GameObject.Transform.WorldTransform * camera.ViewProjection;
            var mvp = t * wvp;
            Drawer.DrawWireframeSphere(radius, mvp, color);
            t.M42 *= -1.0f;
            mvp = t * wvp;
            Drawer.DrawWireframeSphere(radius, mvp, color);
        }

        public override GameComponent Clone()
        {
            return new CapsuleCollider()
            {
                radius = radius,
                height = height,
                Position = Position,
                Rotate = Rotate,
                Scale = Scale,
                collideGroup = collideGroup,
                collideMask = collideMask,
            };
        }
    }
}
