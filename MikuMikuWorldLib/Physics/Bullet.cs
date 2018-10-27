using BulletSharp;
using MikuMikuWorld.GameComponents;
using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Physics
{
    public static class Bullet
    {
        public static Vector3 Gravity
        {
            get { return world.Gravity; }
            set { world.Gravity = value; }
        }

        private static DiscreteDynamicsWorld world;
        private static CollisionDispatcher dispatcher;
        private static DbvtBroadphase broadphase;
        private static ConstraintSolver solver;
        private static CollisionConfiguration collisionConf;
        private static OverlapFilterCallback filterCB;

        private static List<CollisionShape> collisionShapes = new List<CollisionShape>();
        private static List<CollisionObject> collisionObjects = new List<CollisionObject>();
        private static List<RigidBody> rigidBodies = new List<RigidBody>();
        private static bool initialized = false;

        internal static void Init()
        {
            collisionConf = new DefaultCollisionConfiguration();
            dispatcher = new CollisionDispatcher(collisionConf);

            broadphase = new DbvtBroadphase();
            solver = new SequentialImpulseConstraintSolver();
            world = new DiscreteDynamicsWorld(dispatcher, broadphase, solver, collisionConf);
            
            //filterCB = new CollisionFilterCallback();
            //world.PairCache.SetOverlapFilterCallback(filterCB);

            initialized = true;
        }

        #region Create Shape

        /// <summary>
        /// スフィア物理形状を作成
        /// </summary>
        /// <param name="radius">半径</param>
        /// <returns></returns>
        internal static Shapes.SphereShape CreateSphereShape(float radius)
        {
            var shape = new Shapes.SphereShape(radius);
            collisionShapes.Add(shape);
            return shape;
        }

        /// <summary>
        /// ボックス物理形状を作成
        /// </summary>
        /// <param name="halfExtents"></param>
        /// <returns></returns>
        internal static Shapes.BoxShape CreateBoxShape(OpenTK.Vector3 halfExtents)
        {
            var shape = new Shapes.BoxShape(halfExtents);
            collisionShapes.Add(shape);
            return shape;
        }

        /// <summary>
        /// カプセル物理形状を作成
        /// </summary>
        /// <param name="radius">半径</param>
        /// <param name="height">高さ</param>
        /// <returns></returns>
        internal static Shapes.CapsuleShape CreateCapsuleShape(float radius, float height)
        {
            var shape = new Shapes.CapsuleShape(radius, height);
            collisionShapes.Add(shape);
            return shape;
        }

        /// <summary>
        /// 円柱物理形状を作成
        /// </summary>
        /// <param name="halfExtents"></param>
        /// <returns></returns>
        internal static Shapes.CylinderShape CreateCylinderShape(OpenTK.Vector3 halfExtents)
        {
            var shape = new Shapes.CylinderShape(halfExtents);
            collisionShapes.Add(shape);
            return shape;
        }

        /// <summary>
        /// 三角錐物理形状を作成
        /// </summary>
        /// <param name="radius">半径</param>
        /// <param name="height">高さ</param>
        /// <returns></returns>
        internal static Shapes.ConeShape CreateConeShape(float radius, float height)
        {
            var shape = new Shapes.ConeShape(radius, height);
            collisionShapes.Add(shape);
            return shape;
        }

        /// <summary>
        /// 無限平面物理形状を作成
        /// </summary>
        /// <param name="normal">平面法線</param>
        /// <param name="constant"></param>
        /// <returns></returns>
        internal static Shapes.StaticPlaneShape CreateStaticPlaneShape(OpenTK.Vector3 normal, float constant = 0.0f)
        {
            var shape = new Shapes.StaticPlaneShape(normal, constant);
            collisionShapes.Add(shape);
            return shape;
        }

        /// <summary>
        /// メッシュ物理形状を作成
        /// </summary>
        /// <param name="vertices">頂点配列</param>
        /// <param name="indices">インデックス配列</param>
        /// <returns></returns>
        internal static Shapes.MeshShape CreateMeshShape(OpenTK.Vector3[] vertices, int[] indices)
        {
            var shape = new Shapes.MeshShape(vertices, indices);
            collisionShapes.Add(shape);
            return shape;
        }

        /// <summary>
        /// 複合物理形状を作成
        /// </summary>
        /// <param name="shapes">物理形状の配列</param>
        /// <param name="transforms">物理形状のローカル姿勢行列</param>
        /// <returns></returns>
        internal static Shapes.CompoundShape CreateCompoundShape(CollisionShape[] shapes, OpenTK.Matrix4[] transforms)
        {
            var shape = new Shapes.CompoundShape(shapes, transforms);
            collisionShapes.Add(shape);
            return shape;
        }

        #endregion

        internal static CollisionObject CreateCollisionObject(
            CollisionShape shape,
            CollisionFilterGroups group = CollisionFilterGroups.DefaultFilter,
            CollisionFilterGroups mask = CollisionFilterGroups.AllFilter)
        {
            if (shape == null) return null;
            
            var o = new CollisionObject(shape);
            collisionObjects.Add(o);
            world.AddCollisionObject(o.BulletCollisionObject, group, mask);
            return o;
        }

        internal static List<Collision> ContactTest(CollisionObject col)
        {
            var cb = new ContactResultCallback()
            {
                colObj = col,
                gameObject = ((GameComponent)col.tag).GameObject,
                CollisionFilterGroup = col.CollisionFilterGroup,
                CollisionFilterMask = col.CollisionFilterMask,
            };
            world.ContactTest(col.BulletCollisionObject, cb);
            return cb.Collides;
        }
        internal static bool ContactPairTest(CollisionObject colA, CollisionObject colB)
        {
            var cb = new ContactResultCallback();
            world.ContactPairTest(colA.BulletCollisionObject, colB.BulletCollisionObject, cb);
            return cb.Collides.Count > 0;
        }
        public static List<RayTestResult> RayTest(OpenTK.Vector3 from, OpenTK.Vector3 to, params GameObject[] ignoreObjects)
        {
            var cb = new RayResultCB(ignoreObjects);
            world.RayTest(from, to, cb);
            var res = cb.Results;

            foreach (var r in res)
            {
                r.Position = Vector3.Lerp(from, to, r.Rate);
            }

            res.Sort((r1, r2) => r1.Rate - r2.Rate < 0.0f ? -1 : 1);

            return res;
        }

        internal static RigidBody CreateRigidBody(float mass, OpenTK.Matrix4 transform, CollisionShape shape,
            CollisionFilterGroups group = CollisionFilterGroups.DefaultFilter,
            CollisionFilterGroups mask = CollisionFilterGroups.AllFilter)
        {
            if (shape == null)
            {
                shape = CreateBoxShape(Vector3.Zero);
            }

            RigidBody body = new RigidBody(shape, mass, transform);
            world.AddRigidBody(body.BulletRigidBody, group, mask);
            rigidBodies.Add(body);
            return body;
        }

        internal static void Update(float deltaTime)
        {
            world.StepSimulation(deltaTime, 100);

            world.PerformDiscreteCollisionDetection();

            for (var i = 0; i < world.Dispatcher.NumManifolds; i++)
            {
                var mani = world.Dispatcher.GetManifoldByIndexInternal(i);
                mani.RefreshContactPoints(mani.Body0.WorldTransform, mani.Body1.WorldTransform);

                var colA = (mani.Body0.UserObject as CollisionObject).tag as PhysicalGameComponent;
                var colB = (mani.Body1.UserObject as CollisionObject).tag as PhysicalGameComponent;

                if (colA == colB) continue;

                var mps = new ManifoldPoint[mani.NumContacts];
                for (int j = 0; j < mani.NumContacts; j++)
                {
                    mps[j] = mani.GetContactPoint(j);
                }

                colA.OnCollision(colB.GameObject, mps);
                colB.OnCollision(colA.GameObject, mps);
            }
        }

        internal static void AddCollisionObject(CollisionObject col)
        {
            if (!col.IsInWorld) world.AddCollisionObject(col.BulletCollisionObject, col.CollisionFilterGroup, col.CollisionFilterMask);
        }
        internal static void RemoveCollisionObject(CollisionObject col)
        {
            if (col.IsInWorld) world.RemoveCollisionObject(col.BulletCollisionObject);
        }
        internal static void AddRigidBody(RigidBody rigid)
        {
            if (!rigid.BulletRigidBody.IsInWorld) world.AddRigidBody(rigid.BulletRigidBody);
        }
        internal static void RemoveRigidBody(RigidBody rigid)
        {
            if (rigid.BulletRigidBody.IsInWorld) world.RemoveRigidBody(rigid.BulletRigidBody);
        }

        internal static void DestroyShape(CollisionShape shape)
        {
            collisionShapes.Remove(shape);
            shape.Destroy();
        }
        internal static void DestroyCollisionObject(CollisionObject col)
        {
            if (collisionObjects.Remove(col))
            {
                world.RemoveCollisionObject(col.BulletCollisionObject);
            }
            col.Destroy();
        }
        internal static void DestroyRigidBody(RigidBody rigid)
        {
            if (rigidBodies.Remove(rigid))
            {
                world.RemoveRigidBody(rigid.BulletRigidBody);
            }
            rigid.Destroy();
        }
        internal static void Destroy()
        {
            if (!initialized) return;

            foreach (CollisionShape shape in collisionShapes) shape.Destroy();
            collisionShapes.Clear();

            // 衝突オブジェクトの解放
            foreach (CollisionObject obj in collisionObjects)
            {
                world.RemoveCollisionObject(obj.BulletCollisionObject);
                obj.Destroy();
            }
            collisionObjects.Clear();

            // 剛体オブジェクトの解放
            foreach (RigidBody rigid in rigidBodies)
            {
                world.RemoveRigidBody(rigid.BulletRigidBody);
                rigid.Destroy();
            }
            rigidBodies.Clear();

            //filterCB.Dispose();
            dispatcher.Dispose();
            collisionConf.Dispose();
            solver.Dispose();
            broadphase.Dispose();
            //world.Dispose();

            initialized = false;
        }
    }
}
