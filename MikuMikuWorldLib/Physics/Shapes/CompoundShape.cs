using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Physics.Shapes
{
    class CompoundShape : CollisionShape
    {
        private BulletSharp.CompoundShape com;

        private List<CollisionShape> shapes = new List<CollisionShape>();
        private List<OpenTK.Matrix4> transforms = new List<OpenTK.Matrix4>();

        public CollisionShape[] Shapes { get { return shapes.ToArray(); } }
        public OpenTK.Matrix4[] Transforms { get { return transforms.ToArray(); } }

        public CompoundShape(CollisionShape[] shapes, OpenTK.Matrix4[] transforms)
        {
            this.shapes.AddRange(shapes);
            this.transforms.AddRange(transforms);

            com = new BulletSharp.CompoundShape();
            
            for (var i = 0; i < shapes.Length; i++)
            {
                com.AddChildShape(transforms[i], shapes[i].BulletShape);
            }
            BulletShape = com;
            BulletShape.UserObject = this;
        }

        public void AddShape(CollisionShape shape, OpenTK.Matrix4 transform)
        {
            com.AddChildShape(transform, shape.BulletShape);
            transforms.Add(transform);
            shapes.Add(shape);
        }

        public void RemoveShape(CollisionShape shape)
        {
            com.RemoveChildShape(shape.BulletShape);
            var index = shapes.IndexOf(shape);
            shapes.RemoveAt(index);
            transforms.RemoveAt(index);
        }

        public void UpdateTransform(CollisionShape shape, OpenTK.Matrix4 transform)
        {
            var index = shapes.IndexOf(shape);
            com.UpdateChildTransform(index, transform);
        }
    }
}
