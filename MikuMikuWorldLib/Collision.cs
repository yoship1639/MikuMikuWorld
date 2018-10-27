using MikuMikuWorld.GameComponents;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    public class Collision
    {
        public GameObject GameObject;
        public BulletSharp.ManifoldPoint[] ManifoldPoints;

        public Vector3 TotalExtrusion
        {
            get
            {
                var v = Vector3.Zero;
                foreach (var mp in ManifoldPoints)
                {
                    v += mp.PositionWorldOnB - mp.PositionWorldOnA;
                }
                return v;
            }
        }

        public Vector3 Normal
        {
            get
            {
                var v = Vector3.Zero;
                foreach (var mp in ManifoldPoints)
                {
                    v += mp.NormalWorldOnB;
                }
                return v.Normalized();
            }
        }

        public Vector3 WorldPoint
        {
            get
            {
                var v = Vector3.Zero;
                foreach (var mp in ManifoldPoints)
                {
                    v += mp.PositionWorldOnB;
                }
                v /= ManifoldPoints.Length;
                return v;
            }
        }

        public float TotalDistance
        {
            get
            {
                var d = 0.0f;
                foreach (var mp in ManifoldPoints)
                {
                    d += mp.Distance;
                }
                return d;
            }
        }

        public float TotalImpulse
        {
            get
            {
                var d = 0.0f;
                foreach (var mp in ManifoldPoints)
                {
                    d += mp.AppliedImpulse;
                }
                return d;
            }
        }
    }
}
