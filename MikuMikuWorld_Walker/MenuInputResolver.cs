using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    class MenuInputResolver
    {
        public double FirstSpan
        {
            get { return trigger.FirstSpan; }
            set { trigger.FirstSpan = value; }
        }

        public double Span
        {
            get { return trigger.Span; }
            set { trigger.Span = value; }
        }

        TimeTrigger trigger;

        public MenuInputResolver()
        {
            trigger = new TimeTrigger();
        }

        public Key Down { get; set; } = Key.Down;
        public Key Up { get; set; } = Key.Up;
        public Key Right { get; set; } = Key.Right;
        public Key Left { get; set; } = Key.Left;
        public Key Select { get; set; } = Key.Z;
        public Key Back { get; set; } = Key.X;

        public bool IsDown { get; private set; }
        public bool IsUp { get; private set; }
        public bool IsRight { get; private set; }
        public bool IsLeft { get; private set; }
        public bool IsSelect { get; private set; }
        public bool IsBack { get; private set; }

        public void Update(double deltaTime)
        {
            IsDown = false;
            IsUp = false;
            IsRight = false;
            IsLeft = false;
            IsSelect = false;
            IsBack = false;

            var down = Input.IsKeyDown(Down);
            var up = Input.IsKeyDown(Up);
            var right = Input.IsKeyDown(Right);
            var left = Input.IsKeyDown(Left);
            IsSelect = Input.IsKeyReleased(Select);
            IsBack = Input.IsKeyReleased(Back);

            var cursorMove = trigger.Trigger(deltaTime, down || up || right || left);

            if (cursorMove)
            {
                IsDown = down;
                IsUp = up;
                IsRight = right;
                IsLeft = left;
            }
        }
    }
}
