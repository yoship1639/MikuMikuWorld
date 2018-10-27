using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Network
{
    public class NwPhysicalMaterial
    {
        public bool IsRigidBody = false;

        public float Mass = 60.0f;
        public float Friction = 0.5f;
        public float RollingFriction = 0.0f;
        public Vector3f AnisotropicFriction = new Vector3f(0.0f, 0.0f, 0.0f);
        public float LinearDamping= 0.1f;
        public float AngulerDamping= 0.1f;
        public float Restitution= 0.0f;

        public bool FreezePosition = false;
        public bool FreezeRotation= false;
        public bool Kinematic = false;
        public bool DisableDeactivation = false;

        public CollisionFilter Group = CollisionFilter.Default;
        public CollisionFilter Mask = CollisionFilter.Default | CollisionFilter.Static | CollisionFilter.Character;
    }

    public enum CollisionFilter
    {
        All = -1,
        None = 0,
        Default = 1,
        Static = 2,
        Kinematic = 4,
        Debris = 8,
        SensorTrigger = 16,
        Character = 32,
    }
}
