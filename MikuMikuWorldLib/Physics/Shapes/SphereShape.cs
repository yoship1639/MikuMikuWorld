using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Physics.Shapes
{
    class SphereShape : CollisionShape
    {
        public float Radius { get; private set; }

        public SphereShape(float radius)
        {
            Radius = radius;
            BulletShape = new BulletSharp.SphereShape(radius);
            BulletShape.UserObject = this;
        }
    }
}
