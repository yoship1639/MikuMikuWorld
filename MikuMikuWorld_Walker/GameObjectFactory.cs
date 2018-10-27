using MikuMikuWorld.Assets;
using MikuMikuWorld.GameComponents;
using MikuMikuWorld.GameComponents.Coliders;
using MikuMikuWorld.Scripts;
using MikuMikuWorld.Scripts.Character;
using MikuMikuWorld.Walker;
using MikuMikuWorld.Walker.Network;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    static class GameObjectFactory
    {
        private static Dictionary<string, ImportedObject> importedObjects = new Dictionary<string, ImportedObject>();

        private static GameObject[] CreateBoneObject(Transform parent, Bone[] children)
        {
            var list = new List<GameObject>();
            foreach (var c in children)
            {
                var obj = new GameObject(c.Name, Matrix4.Identity, "bone");
                //obj.Layer = GameObject.LayerUI;
                obj.Transform.Parent = parent;
                obj.Transform.WorldTransform = Matrix4.CreateTranslation(c.Position);
                //obj.AddComponent<BoneVisibleTest>(c);
                list.Add(obj);

                if (c.Children != null)
                {
                    list.AddRange(CreateBoneObject(obj.Transform, c.Children));
                }

                //MMW.RegistGameObject(obj);
            }
            return list.ToArray();
        }
        public static GameObject CreateMeshObject(string path, string shader = "Physical")
        {
            var obj = importedObjects.FindValue((i) => i.Path == path);

            if (obj == null)
            {
                var imp = MMW.GetSupportedImporter(path);
                obj = imp.Import(path, Importers.ImportType.Full)[0];

                if (obj.Textures != null)
                {
                    foreach (var tex in obj.Textures) if (tex != null) tex.Load();
                }
                if (obj.Meshes != null)
                {
                    foreach (var mesh in obj.Meshes) if (mesh != null) mesh.Load();
                }

                importedObjects.Add(path, obj);
            }
            var sh = MMW.GetAsset<Shader>(shader);
            if (sh == null) sh = MMW.GetAsset<Shader>("Error");
            foreach (var mat in obj.Materials) mat.Shader = sh;

            var go = new GameObject(obj.Name);

            // bone
            if (obj.Bones != null && obj.Bones.Length > 0)
            {
                CreateBoneObject(go.Transform, new Bone[] { obj.Bones[0] });
            }

            if (obj.Meshes != null)
            {
                for (var i = 0; i < obj.Meshes.Length; i++)
                {
                    var mr = go.AddComponent<MeshRenderer>();
                    mr.Bones = obj.Bones;
                    mr.Mesh = obj.Meshes[i];
                    for (var m = 0; m < mr.Mesh.SubMeshCount; m++)
                    {
                        var matIndex = mr.Mesh.GetMaterialIndex(m);
                        mr.SetMaterial(matIndex, obj.Materials[matIndex], false);
                    }
                }
            }

            if (obj.Morphs != null)
            {
                var morpher = go.AddComponent<ComputeMorpher>();

                foreach (var m in obj.Morphs)
                {
                    if (m == null) continue;
                    morpher.AddMorph(m.Name, m);
                }
            }

            return go;
        }
        public static GameObject CreateStage(string path, string shader = "Deferred Physical")
        {
            var obj = importedObjects.FindValue((i) => i.Path == path);

            if (obj == null)
            {
                var imp = MMW.GetSupportedImporter(path);
                obj = imp.Import(path, Importers.ImportType.Full)[0];

                foreach (var tex in obj.Textures) if (tex != null) tex.Load();
                foreach (var mesh in obj.Meshes) mesh.Load();

                importedObjects.Add(path, obj);
            }
            var sh = MMW.GetAsset<Shader>(shader);
            foreach (var mat in obj.Materials) mat.Shader = sh;

            var go = new GameObject(obj.Name);

            for (var i = 0; i < obj.Meshes.Length; i++)
            {
                var mr = go.AddComponent<MeshRenderer>();
                mr.ForceRendering = true;
                mr.Mesh = obj.Meshes[i];
                for (var m = 0; m < mr.Mesh.SubMeshCount; m++)
                {
                    var matIndex = mr.Mesh.GetMaterialIndex(m);
                    mr.SetMaterial(matIndex, obj.Materials[matIndex], false);
                }
            }

            for (var i = 0; i < obj.Meshes.Length; i++)
            {
                go.AddComponent<MeshCollider>(obj.Meshes[i]);
            }

            return go;
        }

        public static GameObject CreateWorld(World world, string name, string shader = "Deferred Physical")
        {
            var sh = MMW.GetAsset<Shader>(shader);
            foreach (var mat in world.Materials) mat.Shader = sh;

            var go = new GameObject(name);
            go.Tags.Add("world");
            //go.Layer = GameObject.LayerAfterRender + 1;

            if (world.Meshes != null)
            {
                for (var i = 0; i < world.Meshes.Length; i++)
                {
                    var mr = go.AddComponent<MeshRenderer>();
                    mr.ForceRendering = true;
                    mr.Mesh = world.Meshes[i];
                    for (var m = 0; m < mr.Mesh.SubMeshCount; m++)
                    {
                        var matIndex = mr.Mesh.GetMaterialIndex(m);
                        mr.SetMaterial(matIndex, world.Materials[matIndex], false);
                    }
                }
            }

            for (var i = 0; i < world.ColliderMeshes.Length; i++)
            {
                var mc = go.AddComponent<MeshCollider>(world.ColliderMeshes[i]);
                mc.CollideGroup = BulletSharp.CollisionFilterGroups.StaticFilter;
                mc.CollideMask = BulletSharp.CollisionFilterGroups.KinematicFilter | BulletSharp.CollisionFilterGroups.CharacterFilter | BulletSharp.CollisionFilterGroups.DefaultFilter;
            }

            return go;
        }
        public static GameObject CreateCharacter(Character ch, string name, string shader = "Deferred Physical Skin")
        {
            var sh = MMW.GetAsset<Shader>(shader);
            foreach (var mat in ch.Materials) mat.Shader = sh;

            var go = new GameObject(name);
            go.AddComponent<WalkerGameObjectScript>(go, new DummyGameObjectScript(), null);
            go.Tags.Add("character");
            //go.Layer = GameObject.LayerUI + 1;

            var mr = go.AddComponent<MeshRenderer>();
            mr.Mesh = ch.Mesh;
            mr.Bones = ch.Bones;
            for (var m = 0; m < mr.Mesh.SubMeshCount; m++)
            {
                var matIndex = mr.Mesh.GetMaterialIndex(m);
                mr.SetMaterial(matIndex, ch.Materials[matIndex], false);
            }

            // motion
            if (ch.Bones != null)
            {
                var animator = go.AddComponent<ComputeAnimator>();
                animator.Bones = mr.Bones;

                if (ch.Motions != null)
                {
                    foreach (var m in ch.Motions)
                    {
                        animator.AddMotion(m.Name, m);
                    }
                }

                var ac = go.AddComponent<AnimationController>();
                ac.Play("idle");
            }

            if (ch.Morphs != null)
            {
                var morpher = go.AddComponent<Morpher>();

                foreach (var m in ch.Morphs)
                {
                    if (m == null) continue;
                    morpher.AddMorph(m.Name, m);
                }
            }

            if (ch.CollisionShape != null)
            {
                Collider c = null;
                if (ch.CollisionShape is CollisionCapsule)
                {
                    var s = ch.CollisionShape as CollisionCapsule;
                    c = go.AddComponent<CapsuleCollider>(s.Radius, s.Height - s.Radius * 2.0f);
                    c.Position.Y = s.Height * 0.5f;
                }
                else if (ch.CollisionShape is CollisionCylinder)
                {
                    //var s = obj.CollisionShape as CollisionCylinder;
                    //c = go.AddComponent<CylinderCollider>(s.Radius, s.Height - s.Radius * 2.0f);
                    //c.Position.Y = s.Height * 0.5f;
                }
                else if (ch.CollisionShape is CollisionBox)
                {
                    var s = ch.CollisionShape as CollisionBox;
                    c = go.AddComponent<BoxCollider>(s.HalfExtents);
                    c.Position.Y = s.HalfExtents.Y;
                }
                else if (ch.CollisionShape is CollisionSphere)
                {
                    var s = ch.CollisionShape as CollisionSphere;
                    c = go.AddComponent<SphereCollider>(s.Radius);
                    c.Position.Y = s.Radius;
                }

                c.CollideGroup = BulletSharp.CollisionFilterGroups.CharacterFilter;
                c.CollideMask = BulletSharp.CollisionFilterGroups.StaticFilter | BulletSharp.CollisionFilterGroups.CharacterFilter;
            }

            /*
            var rb = go.AddComponent<RigidBody>();
            rb.CollideGroup = BulletSharp.CollisionFilterGroups.CharacterFilter;
            rb.CollideMask = BulletSharp.CollisionFilterGroups.StaticFilter | BulletSharp.CollisionFilterGroups.CharacterFilter;
            rb.Mass = ch.Mass;
            rb.FreezeRotation = true;
            rb.DisableDeactivation = true;
            rb.LinearDamping = 0.5f;
            rb.LinearVelocityLimitXZ = 20.0f;
            rb.Friction = 0.95f;
            */

            return go;
        }
        public static GameObject CreatePlayer(Character ch, string name, string shader = "Deferred Physical Skin")
        {
            var sh = MMW.GetAsset<Shader>(shader);
            foreach (var mat in ch.Materials) mat.Shader = sh;

            var go = new GameObject(name);
            go.AddComponent<WalkerGameObjectScript>(go, new DummyGameObjectScript(), null);
            go.Tags.Add("player");
            //go.Layer = GameObject.LayerAfterRender + 1;

            var mr = go.AddComponent<MeshRenderer>();
            mr.Mesh = ch.Mesh;
            mr.Bones = ch.Bones;
            for (var m = 0; m < mr.Mesh.SubMeshCount; m++)
            {
                var matIndex = mr.Mesh.GetMaterialIndex(m);
                mr.SetMaterial(matIndex, ch.Materials[matIndex], false);
            }

            // motion
            if (ch.Bones != null)
            {
                var animator = go.AddComponent<ComputeAnimator>();
                animator.Bones = mr.Bones;

                if (ch.Motions != null)
                {
                    foreach (var m in ch.Motions)
                    {
                        animator.AddMotion(m.Name, m);
                    }
                }

                var ac = go.AddComponent<AnimationController>();
                ac.Play("idle");
            }

            if (ch.Morphs != null)
            {
                var morpher = go.AddComponent<Morpher>();

                foreach (var m in ch.Morphs)
                {
                    if (m == null) continue;
                    morpher.AddMorph(m.Name, m);
                }
            }

            if (ch.CollisionShape != null)
            {
                Collider c = null;
                if (ch.CollisionShape is CollisionCapsule)
                {
                    var s = ch.CollisionShape as CollisionCapsule;
                    c = go.AddComponent<CapsuleCollider>(s.Radius, s.Height - s.Radius * 2.0f);
                    c.Position.Y = s.Height * 0.5f;
                }
                else if (ch.CollisionShape is CollisionCylinder)
                {
                    //var s = obj.CollisionShape as CollisionCylinder;
                    //c = go.AddComponent<CylinderCollider>(s.Radius, s.Height - s.Radius * 2.0f);
                    //c.Position.Y = s.Height * 0.5f;
                }
                else if (ch.CollisionShape is CollisionBox)
                {
                    var s = ch.CollisionShape as CollisionBox;
                    c = go.AddComponent<BoxCollider>(s.HalfExtents);
                    c.Position.Y = s.HalfExtents.Y;
                }
                else if (ch.CollisionShape is CollisionSphere)
                {
                    var s = ch.CollisionShape as CollisionSphere;
                    c = go.AddComponent<SphereCollider>(s.Radius);
                    c.Position.Y = s.Radius;
                }

                c.CollideGroup = BulletSharp.CollisionFilterGroups.CharacterFilter;
                c.CollideMask = BulletSharp.CollisionFilterGroups.AllFilter;
            }

            var rb = go.AddComponent<RigidBody>();
            rb.CollideGroup = BulletSharp.CollisionFilterGroups.CharacterFilter;
            rb.CollideMask = BulletSharp.CollisionFilterGroups.AllFilter;
            rb.Mass = ch.PhysicalMaterial.Mass;
            rb.FreezeRotation = true;
            rb.DisableDeactivation = true;
            rb.LinearDamping = 0.2f;
            rb.LinearVelocityLimitXZ = 3.0f;
            rb.Friction = 0.95f;

            return go;
        }
        public static GameObject CreateGameObject(NwWalkerGameObject wgo, WorldObject obj, string name, string shader = "Deferred Physical Skin")
        {
            if (!obj.Loaded) obj.Load();

            var sh = MMW.GetAsset<Shader>(shader);
            foreach (var mat in obj.Materials) mat.Shader = sh;

            var go = new GameObject(name);
            go.Tags.Add("world object");
            //go.Layer = GameObject.LayerAfterRender + 1;

            if (obj.Tags != null) foreach (var t in obj.Tags) go.Tags.Add(t);

            //  mesh renderer
            var mr = go.AddComponent<MeshRenderer>();
            mr.Mesh = obj.Mesh;
            mr.Bones = obj.Bones;
            for (var m = 0; m < mr.Mesh.SubMeshCount; m++)
            {
                var matIndex = mr.Mesh.GetMaterialIndex(m);
                mr.SetMaterial(matIndex, obj.Materials[matIndex], false);
            }

            // motion
            if (obj.Bones != null)
            {
                var animator = go.AddComponent<ComputeAnimator>();
                animator.Bones = mr.Bones;

                if (obj.Motions != null)
                {
                    foreach (var m in obj.Motions)
                    {
                        animator.AddMotion(m.Name, m);
                    }
                }

                var ac = go.AddComponent<AnimationController>();
                ac.Play("idle");
            }

            // morph
            if (obj.Morphs != null)
            {
                var morpher = go.AddComponent<Morpher>();

                foreach (var m in obj.Morphs)
                {
                    if (m == null) continue;
                    morpher.AddMorph(m.Name, m);
                }
            }

            // physics
            var center = Vector3.Zero;
            if (obj.CollisionShape != null)
            {
                Collider c = null;
                if (obj.CollisionShape is CollisionCapsule)
                {
                    var s = obj.CollisionShape as CollisionCapsule;
                    c = go.AddComponent<CapsuleCollider>(s.Radius, s.Height - s.Radius * 2.0f);
                    if (!obj.PhysicalMaterial.IsRigidBody) c.Position.Y = s.Height * 0.5f;
                    center = new Vector3(0.0f, s.Height * 0.5f, 0.0f);
                }
                else if (obj.CollisionShape is CollisionCylinder)
                {
                    //var s = obj.CollisionShape as CollisionCylinder;
                    //c = go.AddComponent<CylinderCollider>(s.Radius, s.Height - s.Radius * 2.0f);
                    //c.Position.Y = s.Height * 0.5f;
                }
                else if (obj.CollisionShape is CollisionBox)
                {
                    var s = obj.CollisionShape as CollisionBox;
                    c = go.AddComponent<BoxCollider>(s.HalfExtents);
                    if (!obj.PhysicalMaterial.IsRigidBody) c.Position.Y = s.HalfExtents.Y;
                    center = new Vector3(0, s.HalfExtents.Y, 0);
                }
                else if (obj.CollisionShape is CollisionSphere)
                {
                    var s = obj.CollisionShape as CollisionSphere;
                    c = go.AddComponent<SphereCollider>(s.Radius);
                    //if (!obj.PhysicalMaterial.IsRigidBody) c.Position.Y = s.Radius;
                    //center = new Vector3(0, s.Radius, 0);
                }

                c.CollideGroup = (BulletSharp.CollisionFilterGroups)obj.PhysicalMaterial.Group;
                c.CollideMask = (BulletSharp.CollisionFilterGroups)obj.PhysicalMaterial.Mask | BulletSharp.CollisionFilterGroups.CharacterFilter;
            }
            if (obj.PhysicalMaterial.IsRigidBody)
            {
                var rb = go.AddComponent<RigidBody>();

                rb.CollideGroup = (BulletSharp.CollisionFilterGroups)obj.PhysicalMaterial.Group;
                rb.CollideMask = (BulletSharp.CollisionFilterGroups)obj.PhysicalMaterial.Mask | BulletSharp.CollisionFilterGroups.CharacterFilter;
                rb.Mass = obj.PhysicalMaterial.Mass;
                rb.Friction = obj.PhysicalMaterial.Friction;
                rb.RollingFriction = obj.PhysicalMaterial.RollingFriction;
                rb.AnisotropicFriction = obj.PhysicalMaterial.AnisotropicFriction;
                rb.Restitution = obj.PhysicalMaterial.Restitution;
                rb.LinearDamping = obj.PhysicalMaterial.LinearDamping;
                rb.AngulerDamping = obj.PhysicalMaterial.AngulerDamping;
                
                rb.FreezePosition = obj.PhysicalMaterial.FreezePosition;
                rb.FreezeRotation = obj.PhysicalMaterial.FreezeRotation;
                rb.DisableDeactivation = obj.PhysicalMaterial.DisableDeactivation;
                rb.Kinematic = obj.PhysicalMaterial.Kinematic;

                //rb.CenterOfMass = center;

                go.Transform.Position = center;
            }

            // sound
            if (obj.Sounds != null)
            {
                var sc = go.AddComponent<SoundController>();
                foreach (var s in obj.Sounds) sc.Sounds.Add(s.Name, s.Clone());
            }

            go.AddComponent<WalkerObjectInfo>(obj);

            // scripts
            if (obj.Scripts != null)
            {
                var wr = MMW.GetAsset<WorldResources>();
                foreach (var asm in obj.Scripts)
                {
                    var scr = WorldResources.CreateScript(asm);
                    scr.ScriptHash = wr.GetHash(asm);
                    scr.HostUserID = wgo.UserID;
                    scr.IsHost = MMW.GetAsset<UserData>().UserID == wgo.UserID;
                    go.AddComponent<WalkerGameObjectScript>(go, scr, null);
                }
            }

            return go;
        }

        public static GameObject CreateItem(NwWalkerGameObject wgo, WorldObject obj, WalkerPlayer player, string name, string shader = "Deferred Physical Skin")
        {
            if (!obj.Loaded) obj.Load();

            var sh = MMW.GetAsset<Shader>(shader);
            foreach (var mat in obj.Materials) mat.Shader = sh;

            var go = new GameObject(name);
            go.Tags.Add("item");
            //go.Layer = GameObject.LayerAfterRender + 1;

            if (obj.Tags != null) foreach (var t in obj.Tags) go.Tags.Add(t);

            //  mesh renderer
            var mr = go.AddComponent<MeshRenderer>();
            mr.Mesh = obj.Mesh;
            mr.Bones = obj.Bones;
            for (var m = 0; m < mr.Mesh.SubMeshCount; m++)
            {
                var matIndex = mr.Mesh.GetMaterialIndex(m);
                mr.SetMaterial(matIndex, obj.Materials[matIndex], false);
            }

            // motion
            if (obj.Bones != null)
            {
                var animator = go.AddComponent<ComputeAnimator>();
                animator.Bones = mr.Bones;

                if (obj.Motions != null)
                {
                    foreach (var m in obj.Motions)
                    {
                        animator.AddMotion(m.Name, m);
                    }
                }

                var ac = go.AddComponent<AnimationController>();
                ac.Play("idle");
            }

            // morph
            if (obj.Morphs != null)
            {
                var morpher = go.AddComponent<Morpher>();

                foreach (var m in obj.Morphs)
                {
                    if (m == null) continue;
                    morpher.AddMorph(m.Name, m);
                }
            }

            // physics
            /*
            var center = Vector3.Zero;
            if (obj.CollisionShape != null)
            {
                Collider c = null;
                if (obj.CollisionShape is CollisionCapsule)
                {
                    var s = obj.CollisionShape as CollisionCapsule;
                    c = go.AddComponent<CapsuleCollider>(s.Radius, s.Height - s.Radius * 2.0f);
                    if (!obj.PhysicalMaterial.IsRigidBody) c.Position.Y = s.Height * 0.5f;
                    center = new Vector3(0.0f, s.Height * 0.5f, 0.0f);
                }
                else if (obj.CollisionShape is CollisionCylinder)
                {
                    //var s = obj.CollisionShape as CollisionCylinder;
                    //c = go.AddComponent<CylinderCollider>(s.Radius, s.Height - s.Radius * 2.0f);
                    //c.Position.Y = s.Height * 0.5f;
                }
                else if (obj.CollisionShape is CollisionBox)
                {
                    var s = obj.CollisionShape as CollisionBox;
                    c = go.AddComponent<BoxCollider>(s.HalfExtents);
                    if (!obj.PhysicalMaterial.IsRigidBody) c.Position.Y = s.HalfExtents.Y;
                    center = new Vector3(0, s.HalfExtents.Y, 0);
                }
                else if (obj.CollisionShape is CollisionSphere)
                {
                    var s = obj.CollisionShape as CollisionSphere;
                    c = go.AddComponent<SphereCollider>(s.Radius);
                    if (!obj.PhysicalMaterial.IsRigidBody) c.Position.Y = s.Radius;
                    center = new Vector3(0, s.Radius, 0);
                }

                c.CollideGroup = BulletSharp.CollisionFilterGroups.DefaultFilter;
                c.CollideMask = BulletSharp.CollisionFilterGroups.SensorTrigger;
            }
            */
            /*
            if (obj.PhysicalMaterial.IsRigidBody)
            {
                var rb = go.AddComponent<RigidBody>();

                rb.CollideGroup = BulletSharp.CollisionFilterGroups.DefaultFilter;
                rb.CollideMask = BulletSharp.CollisionFilterGroups.SensorTrigger;
                rb.Mass = obj.PhysicalMaterial.Mass;
                rb.Friction = obj.PhysicalMaterial.Friction;
                rb.RollingFriction = obj.PhysicalMaterial.RollingFriction;
                rb.AnisotropicFriction = obj.PhysicalMaterial.AnisotropicFriction;
                rb.Restitution = obj.PhysicalMaterial.Restitution;
                rb.LinearDamping = obj.PhysicalMaterial.LinearDamping;
                rb.AngulerDamping = obj.PhysicalMaterial.AngulerDamping;

                rb.FreezePosition = obj.PhysicalMaterial.FreezePosition;
                rb.FreezeRotation = obj.PhysicalMaterial.FreezeRotation;
                rb.DisableDeactivation = obj.PhysicalMaterial.DisableDeactivation;
                rb.Kinematic = obj.PhysicalMaterial.Kinematic;

                rb.CenterOfMass = center;

                go.Transform.Position = center;
            }
            */

            // sound
            if (obj.Sounds != null)
            {
                var sc = go.AddComponent<SoundController>();
                foreach (var s in obj.Sounds) sc.Sounds.Add(s.Name, s.Clone());
            }

            go.AddComponent<WalkerObjectInfo>(obj);

            // scripts
            if (obj.Scripts != null)
            {
                var pl = MMW.FindGameComponent<WalkerScript>().Players.Find(p => (string)p.Properties["userID"] == player.UserID);
                var ws = pl.GetComponent<WalkerGameObjectScript>().Script;
                var wr = MMW.GetAsset<WorldResources>();
                foreach (var asm in obj.Scripts)
                {
                    var scr = WorldResources.CreateScript(asm);
                    scr.ParentScript = ws;
                    scr.ScriptHash = wr.GetHash(asm);
                    scr.HostUserID = wgo.UserID;
                    scr.IsHost = MMW.GetAsset<UserData>().UserID == wgo.UserID;
                    go.AddComponent<WalkerGameObjectScript>(go, scr, null);
                }
            }

            return go;
        }
    }
}
