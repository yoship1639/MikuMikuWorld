using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Physics.Shapes
{
    class CylinderShape : CollisionShape
    {
        public OpenTK.Vector3 HalfExtents { get; private set; }

        public CylinderShape(OpenTK.Vector3 halfExtents)
        {
            HalfExtents = halfExtents;
            BulletShape = new BulletSharp.CylinderShape(halfExtents);
            BulletShape.UserObject = this;
        }
    }
}
