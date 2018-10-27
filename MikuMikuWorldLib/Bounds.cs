using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    public struct Bounds
    {
        public Vector3 Center;
        public Vector3 Extents;
        
        public Vector3 Min { get { return Center - Extents; } }
        public Vector3 Max { get { return Center + Extents; } }
        public Vector3 Size { get { return Extents * 2; } }

        public Bounds(Vector3 center, Vector3 extents)
        {
            Center = center;
            Extents = extents;
        }
    }
}
