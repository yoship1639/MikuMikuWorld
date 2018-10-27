using MikuMikuWorld.Assets;
using MikuMikuWorldScript;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    public class Drawer3D : IMeshDrawer
    {
        private Mesh boxMesh;
        public Drawer3D()
        {
            boxMesh = Mesh.CreateSimpleBoxMesh(Vector3.One);
        }

        public Graphics Graphics => Drawer.GetGraphics();

        public void DrawBillboard(Bitmap texture, Vector3 position, Vector2 size, Color4 color, bool freezeYAxis = true)
        {
            throw new NotImplementedException();
        }
        public void DrawBillboard(Bitmap texture, RectangleF srcRect, Vector3 position, Vector2 size, Color4 color, bool freezeYAxis = true)
        {
            throw new NotImplementedException();
        }

        public void DrawLine(Vector3 from, Vector3 to, Matrix4 viewProj, Color4 color, float width = 1)
        {
            Drawer.DrawLine(ref from, ref to, ref viewProj, ref color, width);
        }

        public void DrawBox(Vector3 halfExtents, Matrix4 mvp, Color4 color)
        {
            Drawer.DrawSubMesh(boxMesh.subMeshes[0]);
        }
    }
}
