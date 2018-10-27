using OpenTK;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.GameComponents
{
    public class AudioListener : GameComponent
    {
        private Vector3 prevPos;

        protected internal override void Update(double deltaTime)
        {
            var cam = MMW.MainCamera;
            var pos = GameObject.Transform.WorldPosition;

            var vel = (pos - prevPos) * (float)deltaTime;
            var dirv = GameObject.Transform.WorldDirectionZ;
            var ori = new float[] { dirv.X, dirv.Y, dirv.Z, cam.Up.X, cam.Up.Y, cam.Up.Z };
            AL.Listener(ALListener3f.Position, ref cam.Transform.Position);
            AL.Listener(ALListener3f.Velocity, ref vel);
            AL.Listener(ALListenerfv.Orientation, ref ori);

            prevPos = pos;
        }
    }
}
