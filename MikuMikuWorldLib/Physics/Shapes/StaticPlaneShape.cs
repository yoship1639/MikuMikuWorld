using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Physics.Shapes
{
    class StaticPlaneShape : CollisionShape
    {
        public OpenTK.Vector3 PlaneNormal { get; private set; }
        public float PlaneConstant { get; private set; }

        public StaticPlaneShape(OpenTK.Vector3 planeNormal, float planeConstant)
        {
            PlaneNormal = planeNormal;
            PlaneConstant = planeConstant;
            BulletShape = new BulletSharp.StaticPlaneShape(planeNormal, planeConstant);
            BulletShape.UserObject = this;
        }
    }
}
