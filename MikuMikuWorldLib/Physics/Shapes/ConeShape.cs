using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Physics.Shapes
{
    class ConeShape : CollisionShape
    {
        public float Radius { get; private set; }
        public float Height { get; private set; }

        public ConeShape(float radius, float height)
        {
            Radius = radius;
            Height = height;
            BulletShape = new BulletSharp.ConeShape(radius, height);
            BulletShape.UserObject = this;
        }
    }
}
