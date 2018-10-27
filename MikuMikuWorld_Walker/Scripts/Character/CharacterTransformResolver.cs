using MikuMikuWorld.Networks;
using MikuMikuWorld.Walker;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Scripts.Character
{
    class CharacterTransformResolver : GameComponent
    {
        private WalkerPlayer player;
        private float rotSpeed = 0;
        private Vector3 targetRot;

        public CharacterTransformResolver(WalkerPlayer player)
        {
            this.player = player;
        }

        protected override void Update(double deltaTime)
        {
            {
                Transform.Position = Vector3.Lerp(Transform.Position, player.Position.FromVec3f(), (float)deltaTime * 2.0f);
            }

            var r = player.Rotation.FromVec3f();
            if (targetRot != r)
            {
                targetRot = r;

                var r1 = targetRot.Y;
                var r2 = targetRot.Y + MathHelper.TwoPi;
                var r3 = targetRot.Y - MathHelper.TwoPi;
                var d1 = Math.Abs(r1 - Transform.Rotate.Y);
                var d2 = Math.Abs(r2 - Transform.Rotate.Y);
                var d3 = Math.Abs(r3 - Transform.Rotate.Y);
                var s = d1;
                if (s > d2) s = d2;
                if (s > d3) s = d3;

                rotSpeed = s * 2.5f;
            }

            {
                var rot = Transform.Rotate;
                var to = r;
                var to2 = r + Vector3.UnitY * MathHelper.TwoPi;
                var to3 = r - Vector3.UnitY * MathHelper.TwoPi;
                var dir = (to - rot);
                var dir2 = (to2 - rot);
                var dir3 = (to3 - rot);
                if (dir.Length > dir2.Length) dir = dir2;
                if (dir.Length > dir3.Length) dir = dir3;

                if (dir.Length > 0)
                {
                    if (dir.Length < (float)deltaTime * rotSpeed) rot = to;
                    else
                    {
                        rot += dir.Normalized() * (float)deltaTime * rotSpeed;
                    }
                }
                
                Transform.Rotate = rot;
            }

            Transform.UpdatePhysicalTransform();
        }

        protected override void OnReceivedMessage(string message, params object[] args)
        {
            if (message == "enable transform resolver")
            {
                Enabled = true;
            }
            else if (message == "disable transform resolver")
            {
                Enabled = false;
            }
        }
    }
}
