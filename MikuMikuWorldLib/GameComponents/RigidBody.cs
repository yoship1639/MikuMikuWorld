using MikuMikuWorld.Physics;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.GameComponents
{
    public class RigidBody : PhysicalGameComponent
    {
        public RigidBody()
        {
            getter.Add("Active", (obj) => active);
            getter.Add("Mass", (obj) => mass);
            getter.Add("Friction", (obj) => friction);
            getter.Add("RollingFriction", (obj) => rollingFriction);
            getter.Add("AnisotropicFriction", (obj) => anisotropicFriction);
            getter.Add("LinearDamping", (obj) => linearDamping);
            getter.Add("AngulerDamping", (obj) => angulerDamping);
            getter.Add("Restitution", (obj) => restitution);
            getter.Add("LinearVelocity", (obj) => LinearVelocity);
            getter.Add("LinearVelocityLimitXZ", (obj) => LinearVelocityLimitXZ);
            getter.Add("LinearVelocityLimitY", (obj) => LinearVelocityLimitY);
            getter.Add("FreezePosition", (obj) => FreezePosition);
            getter.Add("FreezeRotation", (obj) => FreezeRotation);
            getter.Add("Kinematic", (obj) => Kinematic);
            getter.Add("DisableDeactivation", (obj) => DisableDeactivation);

            setter.Add("Active", (obj, value) => Active = (bool)value);
            setter.Add("Mass", (obj, value) => Mass = (float)value);
            setter.Add("Friction", (obj, value) => Friction = (float)value);
            setter.Add("RollingFriction", (obj, value) => RollingFriction = (float)value);
            setter.Add("AnisotropicFriction", (obj, value) => AnisotropicFriction = (Vector3)value);
            setter.Add("LinearDamping", (obj, value) => LinearDamping = (float)value);
            setter.Add("AngulerDamping", (obj, value) => AngulerDamping = (float)value);
            setter.Add("Restitution", (obj, value) => Restitution = (float)value);
            setter.Add("LinearVelocity", (obj, value) => LinearVelocity = (Vector3)value);
            setter.Add("LinearVelocityLimitXZ", (obj, value) => LinearVelocityLimitXZ = (float)value);
            setter.Add("LinearVelocityLimitY", (obj, value) => LinearVelocityLimitY = (float)value);
            setter.Add("FreezePosition", (obj, value) => FreezePosition = (bool)value);
            setter.Add("FreezeRotation", (obj, value) => FreezeRotation = (bool)value);
            setter.Add("Kinematic", (obj, value) => Kinematic = (bool)value);
            setter.Add("DisableDeactivation", (obj, value) => DisableDeactivation = (bool)value);
        }

        public override bool ComponentDupulication { get { return false; } }

        #region プロパティ


        private bool active = true;
        public bool Active
        {
            get { return active; }
            set
            {
                active = value;
                if (active)
                {
                    Physics.Bullet.AddRigidBody(rigidBody);
                    rigidBody.BulletRigidBody.ForceActivationState(BulletSharp.ActivationState.ActiveTag);
                }
                else
                {
                    rigidBody.BulletRigidBody.ForceActivationState(BulletSharp.ActivationState.DisableSimulation);
                    Physics.Bullet.RemoveRigidBody(rigidBody);
                } 
            }
        }
        /// <summary>
        /// 質量
        /// </summary>
        private float mass = 10.0f;
        public float Mass
        {
            get { return mass; }
            set
            {
                mass = value;
                rigidBody.Mass = value;
                var inertia = rigidBody.BulletCollisionObject.CollisionShape.CalculateLocalInertia(mass);
                rigidBody.BulletRigidBody.SetMassProps(mass, inertia);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private Vector3 centerofmass;
        public Vector3 CenterOfMass
        {
            get { return centerofmass; }
            set
            {
                centerofmass = value;
                //rigidBody.BulletRigidBody.MotionState.WorldTransform = Matrix4.CreateTranslation(centerofmass) * GameObject.Transform.WorldTransform;
                rigidBody.BulletRigidBody.CenterOfMassTransform = Matrix4.CreateTranslation(centerofmass);
            }
        }
        /// <summary>
        /// 摩擦
        /// </summary>
        private float friction = 0.5f;
        public float Friction
        {
            get { return friction; }
            set { friction = value; rigidBody.BulletRigidBody.Friction = value; }
        }
        /// <summary>
        /// 転がり摩擦
        /// </summary>
        private float rollingFriction = 0.0f;
        public float RollingFriction
        {
            get { return rollingFriction; }
            set { rollingFriction = value; rigidBody.BulletRigidBody.RollingFriction = value; }
        }
        /// <summary>
        /// 回転摩擦
        /// </summary>
        private Vector3 anisotropicFriction = Vector3.Zero;
        public Vector3 AnisotropicFriction
        {
            get { return anisotropicFriction; }
            set { anisotropicFriction = value; rigidBody.BulletRigidBody.AnisotropicFriction = value; }
        }
        /// <summary>
        /// 移動減衰
        /// </summary>
        private float linearDamping = 0.0f;
        public float LinearDamping
        {
            get { return linearDamping; }
            set { linearDamping = value; rigidBody.BulletRigidBody.SetDamping(value, angulerDamping); }
        }
        /// <summary>
        /// 回転減衰
        /// </summary>
        private float angulerDamping = 0.4f;
        public float AngulerDamping
        {
            get { return angulerDamping; }
            set { angulerDamping = value; rigidBody.BulletRigidBody.SetDamping(linearDamping, value); }
        }
        /// <summary>
        /// 反発係数
        /// </summary>
        private float restitution = 0.0f;
        public float Restitution
        {
            get { return restitution;  }
            set { restitution = value; rigidBody.BulletRigidBody.Restitution = value; }
        }
        /// <summary>
        /// 線形速度
        /// </summary>
        public Vector3 LinearVelocity
        {
            get { return rigidBody.BulletRigidBody.LinearVelocity; }
            set { rigidBody.BulletRigidBody.LinearVelocity = value; }
        }
        public float LinearVelocityLimitXZ { get; set; } = 100.0f;
        public float LinearVelocityLimitY { get; set; } = 100.0f;
        /// <summary>
        /// 移動を制御するか
        /// </summary>
        public bool FreezePosition { get; set; }
        /// <summary>
        /// 回転を制御するか
        /// </summary>
        public bool FreezeRotation { get; set; }
        /*
        private Vector3 linearFactor = Vector3.Zero;
        public Vector3 LinearFactor
        {
            get { return linearFactor; }
            set { linearFactor = value; rigidBody.BulletRigidBody.LinearFactor = linearFactor.ToVector3(); }
        }

        private Vector3 angularFactor = Vector3.Zero;
        public Vector3 AngularFactor
        {
            get { return angularFactor; }
            set { angularFactor = value; rigidBody.BulletRigidBody.AngularFactor = angularFactor.ToVector3(); }
        }*/

        public bool Kinematic
        {
            get { return rigidBody.BulletRigidBody.IsKinematicObject; }
            set
            {
                if (value)
                {
                    
                    rigidBody.BulletRigidBody.CollisionFlags = BulletSharp.CollisionFlags.KinematicObject;
                    rigidBody.BulletRigidBody.ActivationState = BulletSharp.ActivationState.DisableDeactivation;
                    rigidBody.BulletRigidBody.Activate();
                }
                else
                {
                    rigidBody.BulletRigidBody.CollisionFlags = BulletSharp.CollisionFlags.None;
                    rigidBody.BulletRigidBody.Activate();
                } 
            }
        }
        public bool DisableDeactivation
        {
            get { return (rigidBody.BulletRigidBody.ActivationState & BulletSharp.ActivationState.DisableDeactivation) != 0; }
            set
            {
                if (value) rigidBody.BulletRigidBody.ActivationState = BulletSharp.ActivationState.DisableDeactivation;
                else rigidBody.BulletRigidBody.ForceActivationState(BulletSharp.ActivationState.WantsDeactivation);
                rigidBody.BulletRigidBody.Activate();
            }
        }

        #endregion

        internal Physics.RigidBody rigidBody;

        protected BulletSharp.CollisionFilterGroups collideGroup = BulletSharp.CollisionFilterGroups.DefaultFilter;
        public BulletSharp.CollisionFilterGroups CollideGroup
        {
            get { return collideGroup; }
            set
            {
                collideGroup = value;
                rigidBody.CollisionFilterGroup = value;
            }
        }
        protected BulletSharp.CollisionFilterGroups collideMask = BulletSharp.CollisionFilterGroups.AllFilter;
        public BulletSharp.CollisionFilterGroups CollideMask
        {
            get { return collideMask; }
            set
            {
                collideMask = value;
                rigidBody.CollisionFilterMask = value;
            }
        }

        protected internal override void OnLoad()
        {
            base.OnLoad();

            var shape = CreateCollisionShape();
            
            rigidBody = Bullet.CreateRigidBody(Mass, GameObject.Transform.WorldTransform, shape, CollideGroup, CollideMask);
            rigidBody.tag = this;
            SetIgnore(true);
        }

        private Physics.Shapes.CompoundShape CreateCollisionShape()
        {
            var cols = GameObject.GetComponents<Collider>();
            if (cols == null) return null;
            var shapes = new Physics.CollisionShape[cols.Length];
            var transforms = new Matrix4[cols.Length];
            for (var i = 0; i < cols.Length; i++)
            {
                shapes[i] = cols[i].collisionObject.Shape;
                cols[i].collisionObject.tag = this;
                transforms[i] = cols[i].ColliderTransform;
            }
            return Bullet.CreateCompoundShape(shapes, transforms);
        }

        private void SetIgnore(bool ignore)
        {
            var cols = GameObject.GetComponents<Collider>();
            foreach (var col in cols)
            {
                col.collisionObject.BulletCollisionObject.SetIgnoreCollisionCheck(rigidBody.BulletRigidBody, ignore);
            }
        }

        internal override void PhysicalUpdate(double deltaTime)
        {
            var v = rigidBody.BulletRigidBody.LinearVelocity;
            if (Math.Sqrt(v.X * v.X + v.Z * v.Z) > LinearVelocityLimitXZ)
            {
                var vxz = new Vector3(v.X, 0.0f, v.Z);
                vxz.NormalizeFast();
                vxz *= LinearVelocityLimitXZ;
                v.X = vxz.X;
                v.Z = vxz.Z;
            }
            if (Math.Abs(v.Y) > LinearVelocityLimitY) v.Y = LinearVelocityLimitY;
            rigidBody.BulletRigidBody.LinearVelocity = v;

            if (Kinematic || (FreezePosition && FreezeRotation)) rigidBody.BulletRigidBody.WorldTransform = GameObject.Transform.WorldTransform;
            else if (!FreezePosition && !FreezeRotation) GameObject.Transform.WorldTransform = /*Matrix4.CreateTranslation(-centerofmass) */ rigidBody.BulletRigidBody.WorldTransform;
            else if (FreezePosition && !FreezeRotation)
            {
                var m = GameObject.Transform.WorldTransform;
                var pos = m.ExtractTranslation();
                var scale = m.ExtractScale();

                var mat = rigidBody.BulletRigidBody.WorldTransform;
                var rot = mat.ExtractEulerRotation();

                if (FreezePosition)
                {
                    mat.M41 = pos.X;
                    mat.M42 = pos.Y;
                    mat.M43 = pos.Z;
                }
                rigidBody.BulletRigidBody.WorldTransform = mat;

                GameObject.Transform.WorldTransform = MatrixHelper.CreateTransform(ref pos, ref rot, ref scale);
            }
            else
            {
                var m = GameObject.Transform.WorldTransform;
                var pos = m.ExtractTranslation();
                var rot = m.ExtractEulerRotation();
                var scale = m.ExtractScale();

                var mm = rigidBody.BulletRigidBody.WorldTransform;
                var p = mm.ExtractTranslation();
                var s = mm.ExtractScale();

                Matrix4 mat;
                MatrixHelper.CreateTransform(ref p, ref rot, ref s, out mat);

                rigidBody.BulletRigidBody.WorldTransform = mat;

                GameObject.Transform.WorldTransform = /*Matrix4.CreateTranslation(-centerofmass)*/ MatrixHelper.CreateTransform(ref p, ref rot, ref scale);
            }
        }

        protected internal override void OnGameComponentAdded(GameComponent com)
        {
            if (com is Collider)
            {
                rigidBody.Shape = CreateCollisionShape();
                SetIgnore(true);
            } 
        }

        protected internal override void OnGameComponentEnabledChanged(bool enabled)
        {
            if (enabled)
            {
                if (active)
                {
                    Physics.Bullet.AddRigidBody(rigidBody);
                    rigidBody.BulletRigidBody.ForceActivationState(BulletSharp.ActivationState.ActiveTag);
                }
            }
            else
            {
                rigidBody.BulletRigidBody.ForceActivationState(BulletSharp.ActivationState.DisableSimulation);
                Physics.Bullet.RemoveRigidBody(rigidBody);
            }
        }

        protected internal override void OnGameObjectEnabledChanged(bool enabled)
        {
            if (enabled)
            {
                if (active)
                {
                    Physics.Bullet.AddRigidBody(rigidBody);
                    rigidBody.BulletRigidBody.ForceActivationState(BulletSharp.ActivationState.ActiveTag);
                }
            }
            else
            {
                rigidBody.BulletRigidBody.ForceActivationState(BulletSharp.ActivationState.DisableSimulation);
                Physics.Bullet.RemoveRigidBody(rigidBody);
            }
        }

        protected internal override void Draw(double deltaTime, Camera camera) { }

        public void ApplyForce(Vector3 force)
        {
            rigidBody.BulletRigidBody.ApplyCentralForce(force);
        }
        public void ApplyImpulse(Vector3 impulse)
        {
            rigidBody.BulletRigidBody.ApplyCentralImpulse(impulse);
        }
        public void ApplyForce(Vector3 force, Vector3 relativePos)
        {
            rigidBody.BulletRigidBody.ApplyForce(force, relativePos);
        }
        public void ApplyImpulse(Vector3 impulse, Vector3 relativePos)
        {
            rigidBody.BulletRigidBody.ApplyImpulse(impulse, relativePos);
        }
        public void ApplyTorque(Vector3 torque)
        {
            rigidBody.BulletRigidBody.ApplyTorque(torque);
        }
        public void ApplyTorqueImpulse(Vector3 torqueImpulse)
        {
            rigidBody.BulletRigidBody.ApplyTorqueImpulse(torqueImpulse);
        }
        public void ClearForces()
        {
            rigidBody.BulletRigidBody.ClearForces();
        }

        public void SetContinuousCollisionDetection(float radius, float threshold)
        {
            rigidBody.BulletRigidBody.CcdSweptSphereRadius = radius;
            rigidBody.BulletRigidBody.CcdMotionThreshold = threshold;
        }

        protected internal override void OnUnload()
        {
            Bullet.DestroyRigidBody(rigidBody);
            rigidBody = null;
        }

        public override GameComponent Clone()
        {
            return new RigidBody()
            {
                mass = mass,
                friction = friction,
                rollingFriction = rollingFriction,
                anisotropicFriction = anisotropicFriction,
                linearDamping = linearDamping,
                angulerDamping = angulerDamping,

            };
        }

    }
}
