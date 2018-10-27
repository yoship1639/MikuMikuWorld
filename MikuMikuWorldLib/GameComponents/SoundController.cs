using MikuMikuWorld.Assets;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.GameComponents
{
    public class SoundController : GameComponent
    {
        protected internal override void Update(double deltaTime)
        {
            var pos = Transform.WorldPosition;
            var dir = Transform.WorldDirectionZ;
            var vel = (pos - Transform.OldWorldTransfom.ExtractTranslation()) * (float)deltaTime;
            foreach (var s in Sounds.Values)
            {
                AL.Source(s.Source, ALSource3f.Position, ref pos);
                AL.Source(s.Source, ALSource3f.Direction, ref dir);
                AL.Source(s.Source, ALSource3f.Velocity, ref vel);
            }
        }

        public Dictionary<string, Sound> Sounds = new Dictionary<string, Sound>();

        public void Play(string name, float volume = 1.0f, bool loop = false)
        {
            Sound s;
            if (!Sounds.TryGetValue(name, out s)) return;

            AL.Source(s.Source, ALSourcef.Gain, volume);
            AL.SourcePlay(s.Source);
        }
        public void Stop(string name)
        {
            Sound s;
            if (!Sounds.TryGetValue(name, out s)) return;

            AL.SourceStop(s.Source);
        }

        protected internal override void OnReceivedMessage(string message, params object[] args)
        {
            if (message == "play sound")
            {
                if (Sounds.ContainsKey((string)args[0]))
                {
                    Play((string)args[0], (float)args[1], (bool)args[2]);
                }
            }
            else if (message == "stop sound")
            {
                if (Sounds.ContainsKey((string)args[0]))
                {
                    Stop((string)args[0]);
                }
            }
        }

        protected internal override void OnUnload()
        {
            base.OnUnload();

            foreach (var s in Sounds)
            {
                Stop(s.Key);
                s.Value.Unload();
            }
        }
    }
}
