using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikuMikuWorld.GameComponents;
using MikuMikuWorld.Assets;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace MikuMikuWorld
{
    class BoneVisibleTest : DrawableGameComponent
    {
        public override bool ComponentDupulication {
            get { return true; } }

        public Bone Bone { get; set; }

        public BoneVisibleTest() { }
        public BoneVisibleTest(Bone bone)
        {
            Bone = bone;
        }

        public override GameComponent Clone()
        {
            throw new NotImplementedException();
        }

        protected override void Draw(double deltaTime, Camera camera)
        {
            var wp = GameObject.Transform.WorldPosition;
            var vp = camera.View * camera.Projection;
            var mvp = GameObject.Transform.WorldTransform * vp;
            GL.Disable(EnableCap.DepthTest);

            var color = Color4.Gray;
            if (Bone.BoneType == "IK") color = Color4.Red;
            if (Bone.BoneType == "UnderIK") color = Color4.Green;
            if (Bone.BoneType == "IKConnect") color = Color4.Blue;
            if (Bone.BoneType != "Standard")
            {
                Drawer.DrawWireframeSphere(0.01f, mvp, color);

                if (Bone.Children != null)
                {
                    foreach (var b in Bone.Children)
                    {
                        var p = GameObject.Transform.Position;
                        var obj = MMW.FindGameObject(o =>
                        {
                            var bvt = o.GetComponent<BoneVisibleTest>();
                            if (bvt == null) return false;
                            return bvt.Bone == b;
                        });
                        Drawer.DrawLine(wp, obj.Transform.WorldPosition, vp, Color4.Blue);
                    }
                }
            } 
            
            //Drawer.DrawLine(wp, wp + Bone.AxisX * 0.1f, vp, Color4.Red);
            //Drawer.DrawLine(wp, wp + Bone.AxisZ * 0.1f, vp, Color4.Blue);
            //Drawer.DrawLine(wp, wp + Bone.FixedAxis * 0.1f, vp, Color4.Purple);
            GL.Enable(EnableCap.DepthTest);
        }
    }
}
