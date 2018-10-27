using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Physics
{
    abstract class CollisionShape : PhysicsObject
    {
        protected internal BulletSharp.CollisionShape BulletShape { get; protected set; }

        internal override void Destroy()
        {
            BulletShape.Dispose();
            BulletShape = null;
        }
    }
}
