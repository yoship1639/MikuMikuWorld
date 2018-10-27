using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Network
{
    public abstract class NwCollisionShape
    {
    }

    public class NwCollisionCapsule : NwCollisionShape
    {
        public float Height = 1.6f;
        public float Radius = 0.3f;
    }

    public class NwCollisionCylinder : NwCollisionShape
    {
        public float Height = 1.6f;
        public float Radius = 0.3f;
    }

    public class NwCollisionBox : NwCollisionShape
    {
        public Vector3f HalfExtents = new Vector3f(0.3f, 0.3f, 0.3f);
    }

    public class NwCollisionSphere : NwCollisionShape
    {
        public float Radius = 0.3f;
    }

    public class NwCollisionMesh : NwCollisionShape
    {
        public int[] MeshIndices;
    }
}
