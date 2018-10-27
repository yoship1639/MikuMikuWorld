﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikuMikuWorld.GameComponents;
using OpenTK;
using OpenTK.Graphics;

namespace MikuMikuWorld
{
    class RotateTest : GameComponent
    {
        public override bool ComponentDupulication { get { return true; } }

        public float Speed { get; set; } = (float)Math.PI;

        protected override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            GameObject.Transform.Rotate.Y += (float)deltaTime * Speed;
        }

        public override GameComponent Clone()
        {
            return new RotateTest();
        }
    }
}
