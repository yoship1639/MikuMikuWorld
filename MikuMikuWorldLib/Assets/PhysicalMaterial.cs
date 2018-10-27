using MikuMikuWorld.Network;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Assets
{
    public class PhysicalMaterial : IAsset
    {
        public string Name { get; set; }
        public bool Loaded => true;
        public Result Load() => Result.Success;
        public Result Unload() => Result.Success;

        public bool IsRigidBody;
        public float Mass { get; set; } = 50.0f;
        public float Friction { get; set; } = 0.5f;
        public float RollingFriction { get; set; } = 0.0f;
        public Vector3 AnisotropicFriction { get; set; } = Vector3.Zero;
        public float LinearDamping { get; set; } = 0.1f;
        public float AngulerDamping { get; set; } = 0.4f;
        public float Restitution { get; set; } = 0.0f;

        public bool FreezePosition { get; set; } = false;
        public bool FreezeRotation { get; set; } = true;
        public bool DisableDeactivation { get; set; } = false;
        public bool Kinematic { get; set; } = false;

        public CollisionFilter Group = CollisionFilter.Default;
        public CollisionFilter Mask = CollisionFilter.Default | CollisionFilter.Static | CollisionFilter.Character;
    }

    
}
