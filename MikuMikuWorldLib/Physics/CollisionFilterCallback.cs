using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;

namespace MikuMikuWorld.Physics
{
    class CollisionFilterCallback : OverlapFilterCallback
    {
        public override bool NeedBroadphaseCollision(BroadphaseProxy proxy0, BroadphaseProxy proxy1)
        {
            if (proxy1 == null) return false;
            var collide = (proxy0.CollisionFilterGroup & proxy1.CollisionFilterMask) != 0;
            collide = collide && (proxy0.CollisionFilterMask & proxy1.CollisionFilterGroup) != 0;
            /*
            if (!collide) return false;

            var rigid0 = proxy0.ClientObject as BulletSharp.RigidBody;
            if (rigid0 != null)
            {
                var com = ((RigidBody)rigid0.UserObject).Shape as Shapes.CompoundShape;
                if (com == null) return false;
                var col = proxy1.ClientObject as BulletSharp.CollisionObject;
                var shape = ((CollisionObject)col.UserObject).Shape;
                if (Array.Exists(com.Shapes, (s) => s == shape)) return false;
            }

            var rigid1 = proxy1.ClientObject as BulletSharp.RigidBody;
            if (rigid1 != null)
            {
                var com = ((RigidBody)rigid1.UserObject).Shape as Shapes.CompoundShape;
                if (com == null) return false;
                var col = proxy0.ClientObject as BulletSharp.CollisionObject;
                var shape = ((CollisionObject)col.UserObject).Shape;
                if (Array.Exists(com.Shapes, (s) => s == shape)) return false;
            }
            */
            return collide;
        }
    }
}
