using BulletSharp;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Physics
{
    class RigidBody : CollisionObject
    {
        internal BulletSharp.RigidBody BulletRigidBody
        {
            get { return (BulletSharp.RigidBody)BulletCollisionObject; }
            set { BulletCollisionObject = value; }
        }
        internal float Mass
        {
            get { return 1.0f / BulletRigidBody.InvMass; }
            set { BulletRigidBody.SetMassProps(value, inertia); }
        }
        private Vector3 inertia;

        public RigidBody(CollisionShape shape, float mass, OpenTK.Matrix4 transform)
        {
            DefaultMotionState state = new DefaultMotionState(transform);
            inertia = shape.BulletShape.CalculateLocalInertia(mass);
            var info = new RigidBodyConstructionInfo(mass, state, shape.BulletShape, inertia);
            BulletRigidBody = new BulletSharp.RigidBody(info);
            BulletRigidBody.UserObject = this;
            BulletCollisionObject.Restitution = 0.5f;
        }

        internal override void Destroy()
        {
            BulletRigidBody.Dispose();
            BulletRigidBody = null;
            BulletCollisionObject = null;
        }
    }
}
