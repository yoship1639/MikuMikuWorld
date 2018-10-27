using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Physics
{
    class CollisionObject : PhysicsObject
    {
        internal bool IsInWorld { get; set; }
        internal BulletSharp.CollisionObject BulletCollisionObject { get; set; }
        internal OpenTK.Matrix4 WorldTransform
        {
            get { return BulletCollisionObject.WorldTransform; }
            set { BulletCollisionObject.WorldTransform = value; }
        }
        internal BulletSharp.CollisionFilterGroups CollisionFilterGroup
        {
            get { return BulletCollisionObject.BroadphaseHandle.CollisionFilterGroup; }
            set { BulletCollisionObject.BroadphaseHandle.CollisionFilterGroup = value; }
        }
        internal BulletSharp.CollisionFilterGroups CollisionFilterMask
        {
            get { return BulletCollisionObject.BroadphaseHandle.CollisionFilterMask; }
            set { BulletCollisionObject.BroadphaseHandle.CollisionFilterMask = value; }
        }
        internal CollisionShape Shape
        {
            get { return (CollisionShape)BulletCollisionObject.CollisionShape.UserObject; }
            set { BulletCollisionObject.CollisionShape = value.BulletShape; }
        }
        internal object tag;

        internal CollisionObject() { }
        public CollisionObject(CollisionShape shape)
        {
            BulletCollisionObject = new BulletSharp.CollisionObject();
            BulletCollisionObject.UserObject = this;
            BulletCollisionObject.CollisionShape = shape.BulletShape;
            //BulletCollisionObject.Restitution = 1.0f;
            
            IsInWorld = true;
        }

        internal override void Destroy()
        {
            BulletCollisionObject.Dispose();
            BulletCollisionObject = null;
        }
    }
}
