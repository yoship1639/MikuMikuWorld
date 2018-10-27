using BulletSharp;
using MikuMikuWorld.GameComponents;
using MikuMikuWorld.GameComponents.Coliders;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Physics
{
    class RayResultCB : RayResultCallback
    {
        List<GameObject> objects;

        public RayResultCB(params GameObject[] objects)
        {
            this.objects = new List<GameObject>();
            if (objects != null) this.objects.AddRange(objects);
        }

        public List<RayTestResult> Results = new List<RayTestResult>();

        public override float AddSingleResult(LocalRayResult rayResult, bool normalInWorldSpace)
        {
            if (rayResult.HitFraction >= 0.0f && rayResult.HitFraction <= 1.0f)
            {
                var col = rayResult.CollisionObject;
                if (col == null) return 1.0f;

                var uo = col.UserObject as CollisionObject;
                if (uo == null) return 1.0f;

                var go = uo.tag as GameComponent;
                if (go == null) return 1.0f;

                var obj = go.GameObject;
                if (obj == null) return 1.0f;

                if (objects.Contains(obj))
                    return 1.0f;

                Results.Add(new RayTestResult()
                {
                    Rate = rayResult.HitFraction,
                    Normal = rayResult.HitNormalLocal,
                    GameObject = obj,
                });
                return rayResult.HitFraction;
            }

            return 1.0f;
        }
    }
}
