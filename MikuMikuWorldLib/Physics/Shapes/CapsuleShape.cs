using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Physics.Shapes
{
    class CapsuleShape : CollisionShape
    {
        public float Radius { get; private set; }
        public float Height { get; private set; }

        public CapsuleShape(float radius, float height)
        {
            Radius = radius;
            Height = height;
            BulletShape = new BulletSharp.CapsuleShape(radius, height);
            BulletShape.UserObject = this;
        }
    }
}
