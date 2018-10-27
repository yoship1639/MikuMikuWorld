using MikuMikuWorld.Assets;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.GameComponents
{
    public abstract class AMorpher : GameComponent
    {
        public abstract bool EnableAsyncCalc { get; }

        public abstract bool HasMorph(string name);
        public abstract bool AddMorph(string name, Morph morph);
        public abstract bool RemoveMorph(string name);

        public abstract void SetRate(string name, float value);
        public abstract void AddRate(string name, float value);
        public abstract void AddRate(string name, float value, float min, float max);

        public abstract BoneMorph[] GetBoneTransforms();

        public abstract void CalcMorph();
        public abstract void UpdateData();

        public abstract void UseMorph(int binding);
        public abstract void UnuseMorph(int binding);
    }
}
