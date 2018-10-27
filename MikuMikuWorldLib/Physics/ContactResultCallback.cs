using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;
using MikuMikuWorld.GameComponents;

namespace MikuMikuWorld.Physics
{
    class ContactResultCallback : BulletSharp.ContactResultCallback
    {
        public List<Collision> Collides = new List<Collision>();
        public CollisionObject colObj;
        public GameObject gameObject;

        public override bool NeedsCollision(BroadphaseProxy proxy0)
        {
            var collide = (proxy0.CollisionFilterGroup & CollisionFilterMask) != 0;
            collide = collide && (proxy0.CollisionFilterMask & CollisionFilterGroup) != 0;

            return collide;
        }

        public override float AddSingleResult(ManifoldPoint cp, CollisionObjectWrapper colObj0Wrap, int partId0, int index0, CollisionObjectWrapper colObj1Wrap, int partId1, int index1)
        {
            return 1.0f;
            /*
            CollisionObject col = null;
            if (colObj0Wrap.CollisionObject == colObj.BulletCollisionObject) col = (CollisionObject)colObj1Wrap.CollisionObject.UserObject;
            else if (colObj1Wrap.CollisionObject == colObj.BulletCollisionObject) col = (CollisionObject)colObj0Wrap.CollisionObject.UserObject;

            var com = (GameComponent)col.tag;
            if (com.GameObject == gameObject) return 0.0f;

            var collision = new Collision();
            collision.GameObject = com.GameObject;
            collision.Collider = com as Collider;
            collision.RigidBody = com as MikuMikuWorld.GameComponents.RigidBody;
            collision.Impulse = cp.AppliedImpulse;

            if (Collides.Exists((c) =>
            {
                if (collision.Collider != null && c.Collider == collision.Collider) return true;
                else if (collision.RigidBody != null && c.RigidBody == collision.RigidBody) return true;
                return false;
            }))
                return 0.0f;

            Collides.Add(collision);

            return cp.CombinedFriction;*/
        }
    }
}
