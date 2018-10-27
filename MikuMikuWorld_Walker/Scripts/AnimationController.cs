using MikuMikuWorld.GameComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Scripts
{
    class AnimationController : GameComponent
    {
        public double Priority { get; set; } = 1.0;
        public double Speed { get; set; } = 1.0;
        public bool Pause { get; set; } = false;

        private double time;
        private double frame;
        public AAnimator Animator { get; set; }

        private string prevAnimName = "";
        private string animName = "";
        private double delay = 0.0;
        private double setdelay = 0.0;
        private float maxframe = 500.0f;

        protected override void OnLoad()
        {
            Animator = GameObject.GetComponent<AAnimator>();
        }

        protected override void Update(double deltaTime)
        {
            if (Pause) return;
            var dist = (Transform.WorldPosition - MMW.MainCamera.Transform.WorldPosition).Length;

            var interval = dist / (300.0 * Priority);
            if (interval > 0.2) interval = 0.2;

            time += deltaTime;
            frame += deltaTime * Speed;
            if (time >= interval)
            {
                time -= interval;
                Animator.Frame = MMWMath.Repeat((float)(frame * 30.0), 0.0f, maxframe);
            }

            if (setdelay > 0.0)
            {
                delay -= deltaTime;
                if (delay < 0.0) delay = 0.0;
                var rate = (float)(delay / setdelay);
                if (prevAnimName != null) Animator.SetRate(prevAnimName, rate);
                Animator.SetRate(animName, 1.0f - rate);

                if (delay == 0.0)
                {
                    prevAnimName = animName;
                    setdelay = 0.0;
                }
            }
        }

        public void Play(string name, double delay = 0.001, bool loop = true)
        {
            setdelay = delay;
            this.delay = delay;
            animName = name;
            frame = 0.0;
            maxframe = Animator.GetMaxFrame(name);
            Pause = false;
        }

        public void Stop()
        {
            Animator.SetRate(prevAnimName, 0.0f);
            Animator.SetRate(animName, 0.0f);
            setdelay = 0.0;
            delay = 0.0;
            frame = 0.0f;
            Pause = true;
        }

        protected override void OnReceivedMessage(string message, params object[] args)
        {
            if (message == "play motion")
            {
                Play((string)args[0], (double)args[1], (bool)args[2]);
            }
            else if (message == "pause motion")
            {
                Pause = true;
            }
            else if (message == "resume motion")
            {
                Pause = false;
            }
            else if (message == "stop motion")
            {
                Stop();
            }
            else if (message == "set motion speed")
            {
                Speed = (float)args[0];
            }
        }
    }
}
