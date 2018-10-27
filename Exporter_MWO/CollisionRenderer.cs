using MikuMikuWorld;
using MikuMikuWorld.GameComponents;
using OpenTK;
using OpenTK.Graphics;

namespace Exporter_MMW
{
    class CollisionRenderer : DrawableGameComponent
    {
        public ICollisionShape Shape { get; set; }

        protected override void Draw(double deltaTime, Camera camera)
        {
            if (Shape == null) return;

            try
            {
                if (Shape is CollisionCapsule)
                {
                    var c = Shape as CollisionCapsule;
                    var radius = (float)c.numericUpDown_radius.Value;
                    var height = (float)c.numericUpDown_height.Value;
                    var vp = camera.ViewProjection;
                    Drawer.DrawWireframeSphere(radius, Matrix4.CreateTranslation(0.0f, radius, 0.0f) * vp, Color4.White);
                    Drawer.DrawWireframeSphere(radius, Matrix4.CreateTranslation(0.0f, height - radius, 0.0f) * vp, Color4.White);
                }

                if (Shape is CollisionBox)
                {
                    var c = Shape as CollisionBox;
                    var v = new Vector3();
                    v.X = (float)c.numericUpDown_x.Value;
                    v.Y = (float)c.numericUpDown_y.Value;
                    v.Z = (float)c.numericUpDown_z.Value;

                    Drawer.DrawWireframeBox(v, Matrix4.CreateTranslation(0.0f, v.Y, 0.0f) * camera.ViewProjection, Color4.White);
                }

                if (Shape is CollisionSphere)
                {
                    var c = Shape as CollisionSphere;
                    var r = (float)c.numericUpDown_radius.Value;
                    Drawer.DrawWireframeSphere(r, Matrix4.CreateTranslation(0.0f, r, 0.0f) * camera.ViewProjection, Color4.White);
                }
            }
            catch { }
        }
    }
}