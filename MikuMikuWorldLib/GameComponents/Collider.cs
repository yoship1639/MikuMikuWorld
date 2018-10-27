using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikuMikuWorld.Physics;
using OpenTK;
using System.IO;

namespace MikuMikuWorld.GameComponents
{
    public abstract class Collider : PhysicalGameComponent
    {
        private bool active = true;
        public bool Active
        {
            get { return active; }
            set
            {
                active = value;
                if (value)
                {
                    Bullet.AddCollisionObject(collisionObject);
                    collisionObject.BulletCollisionObject.ForceActivationState(BulletSharp.ActivationState.ActiveTag);
                }
                else
                {
                    collisionObject.BulletCollisionObject.ForceActivationState(BulletSharp.ActivationState.DisableSimulation);
                    Bullet.RemoveCollisionObject(collisionObject);
                } 
            }
        }
        protected BulletSharp.CollisionFilterGroups collideGroup = BulletSharp.CollisionFilterGroups.DefaultFilter;
        public BulletSharp.CollisionFilterGroups CollideGroup
        {
            get { return collideGroup; }
            set
            {
                collideGroup = value;
                collisionObject.CollisionFilterGroup = value;
            }
        }
        protected BulletSharp.CollisionFilterGroups collideMask = BulletSharp.CollisionFilterGroups.AllFilter;
        public BulletSharp.CollisionFilterGroups CollideMask
        {
            get { return collideMask; }
            set
            {
                collideMask = value;
                collisionObject.CollisionFilterMask = value;
            }
        }
        internal Physics.CollisionObject collisionObject;
        public Vector3 Position = Vector3.Zero;
        public Vector3 Rotate = Vector3.Zero;
        public Vector3 Scale = Vector3.One;
        public Matrix4 ColliderTransform { get { return MatrixHelper.CreateTransform(ref Position, ref Rotate, ref Scale); } }
        internal abstract void RecreateShape();
        internal override void PhysicalUpdate(double deltaTime) { }
        protected ActivationState State
        {
            get
            {
                if (collisionObject == null || !collisionObject.BulletCollisionObject.IsActive) return ActivationState.Inactive;
                var rigid = collisionObject.tag as RigidBody;
                if (rigid != null)
                {
                    if (
                        rigid.rigidBody.BulletRigidBody.ActivationState == BulletSharp.ActivationState.DisableDeactivation
                        || rigid.rigidBody.BulletRigidBody.ActivationState == BulletSharp.ActivationState.ActiveTag)
                        return ActivationState.Active;
                }
                return ActivationState.Sleeping;
            }
        }

        protected internal override void OnGameComponentEnabledChanged(bool enabled)
        {
            if (enabled)
            {
                if (active)
                {
                    Bullet.AddCollisionObject(collisionObject);
                    collisionObject.BulletCollisionObject.ForceActivationState(BulletSharp.ActivationState.ActiveTag);
                }
            }
            else
            {
                collisionObject.BulletCollisionObject.ForceActivationState(BulletSharp.ActivationState.DisableSimulation);
                Bullet.RemoveCollisionObject(collisionObject);
            }
        }
        protected internal override void OnGameObjectEnabledChanged(bool enabled)
        {
            if (enabled)
            {
                if (active)
                {
                    Bullet.AddCollisionObject(collisionObject);
                    collisionObject.BulletCollisionObject.ForceActivationState(BulletSharp.ActivationState.ActiveTag);
                }
            }
            else
            {
                collisionObject.BulletCollisionObject.ForceActivationState(BulletSharp.ActivationState.DisableSimulation);
                Bullet.RemoveCollisionObject(collisionObject);
            }
        }
        protected internal override void OnUnload()
        {
            Bullet.DestroyCollisionObject(collisionObject);
            collisionObject = null;
        }
    }

    public enum ActivationState
    {
        Active,
        Sleeping,
        Inactive,
    }
}
