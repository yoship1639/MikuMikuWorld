using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Physics.Shapes
{
    class MeshShape : CollisionShape
    {
        public OpenTK.Vector3[] Vertices { get; private set; }
        public int[] Indices { get; private set; }

        public MeshShape(OpenTK.Vector3[] vertices, int[] indices)
        {
            Vertices = vertices;
            Indices = indices;
            float[] verts = new float[vertices.Length * 3];
            for (var i = 0; i < vertices.Length; i++)
            {
                verts[i * 3 + 0] = vertices[i].X;
                verts[i * 3 + 1] = vertices[i].Y;
                verts[i * 3 + 2] = vertices[i].Z;
            }
            var buffer = new BulletSharp.TriangleIndexVertexArray(indices, verts);
            BulletShape = new BulletSharp.BvhTriangleMeshShape(buffer, true);
            BulletShape.UserObject = this;
        }
    }
}
