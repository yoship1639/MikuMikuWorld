using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Physics.Shapes
{
    class BoxShape : CollisionShape
    {
        public OpenTK.Vector3 HalfExtent { get; private set; }

        public BoxShape(OpenTK.Vector3 halfExtents)
        {
            HalfExtent = halfExtents;
            BulletShape = new BulletSharp.BoxShape(halfExtents);
            BulletShape.UserObject = this;
        }
    }
}
