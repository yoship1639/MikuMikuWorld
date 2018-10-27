using MikuMikuWorld.GameComponents;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    class PhysicsTest : GameComponent
    {
        public override bool ComponentDupulication { get { return true; } }

        RigidBody rigidBody;
        Collider[] colliders;
        Random rand;

        protected override void OnLoad()
        {
            base.OnLoad();

            rigidBody = GameObject.GetComponent<RigidBody>();
            colliders = GameObject.GetComponents<Collider>();
            rand = new Random(GetHashCode());

            rigidBody.DisableDeactivation = true;
            rigidBody.AngulerDamping = 0.4f;
            rigidBody.Friction = 0.5f;
            rigidBody.AnisotropicFriction = new Vector3(0.5f);
            rigidBody.RollingFriction = 0.5f;
        }

        protected override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            if (Input.IsKeyPressed(OpenTK.Input.Key.I))
            {
                rigidBody.ApplyImpulse(Vector3.UnitY * rand.Next(30, 100));
            }
            if (Input.IsKeyPressed(OpenTK.Input.Key.T))
            {
                rigidBody.ApplyTorqueImpulse(Vector3.UnitY * rand.Next(10, 50));
            }
        }

        public override GameComponent Clone()
        {
            throw new NotImplementedException();
        }
    }
}
