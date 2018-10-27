using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Assets
{
    public interface IInterpolate
    {
        float GetRate(float value);
    }

    public static class Interpolates
    {
        public static readonly IInterpolate Linear = new LinearInterpolate();
        public static readonly IInterpolate Smoothstep = new SmoothstepInterpolate();
    }

    public class LinearInterpolate : IInterpolate
    {
        public float GetRate(float value) => value;
    }
    public class SmoothstepInterpolate : IInterpolate
    {
        public float GetRate(float value)
        {
            return value * value * (3 - 2 * value);
        }
    }
    public class BezierInterpolate : IInterpolate
    {
        public Vector2 p1;
        public Vector2 p2;

        public float GetRate(float value)
        {
            var t = value;
            var t0 = 1.0f - t;
            return 3.0f * t0 * t0 * t * p1.Y + 3.0f * t0 * t * t * p2.Y + t * t * t;
        }
    }
}
