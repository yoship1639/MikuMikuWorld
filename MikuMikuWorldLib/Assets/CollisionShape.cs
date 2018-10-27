using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Assets
{
    public abstract class CollisionShape
    {
    }

    public class CollisionCapsule : CollisionShape
    {
        public float Height;
        public float Radius;
    }

    public class CollisionCylinder : CollisionShape
    {
        public float Height;
        public float Radius;
    }

    public class CollisionBox : CollisionShape
    {
        public Vector3 HalfExtents;
    }

    public class CollisionSphere : CollisionShape
    {
        public float Radius;
    }

    public class CollisionMesh : CollisionShape
    {
        public int[] MeshIndices;
    }
}
