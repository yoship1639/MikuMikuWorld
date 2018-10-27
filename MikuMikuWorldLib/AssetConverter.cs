using MikuMikuWorld.Assets;
using MikuMikuWorld.Network;
using MikuMikuWorldScript;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld
{
    public static class AssetConverter
    {
        public static World FromNwWorld(NwWorld world)
        {
            var w = new World();
            w.nwWorld = world;
            w.Name = world.Name;
            w.Texture2Ds = new Texture2D[world.Texture2Ds.Length];
            for (var i = 0; i < world.Texture2Ds.Length; i++)
            {
                w.Texture2Ds[i] = FromNwTexture2D(world.Texture2Ds[i]);
                w.Texture2Ds[i].UseMipmap = true;
                w.Texture2Ds[i].MinFilter = OpenTK.Graphics.OpenGL4.TextureMinFilter.LinearMipmapLinear;
                w.Texture2Ds[i].MagFilter = OpenTK.Graphics.OpenGL4.TextureMagFilter.Linear;
                w.Texture2Ds[i].WrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat;
            }

            w.TextureCubes = new TextureCube[world.Cubemaps.Length];
            for (var i = 0; i < world.Cubemaps.Length; i++)
            {
                w.TextureCubes[i] = FromNwTextureCube(world.Cubemaps[i]);
                w.TextureCubes[i].UseMipmap = true;
                w.TextureCubes[i].MinFilter = OpenTK.Graphics.OpenGL4.TextureMinFilter.LinearMipmapLinear;
                w.TextureCubes[i].MagFilter = OpenTK.Graphics.OpenGL4.TextureMagFilter.Linear;
                w.TextureCubes[i].WrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat;
            }

            w.Meshes = new Mesh[world.Meshes.Length];
            for (var i = 0; i < world.Meshes.Length; i++) w.Meshes[i] = FromNwMesh(world.Meshes[i]);

            w.ColliderMeshes = new ColliderMesh[world.ColliderMeshes.Length];
            for (var i = 0; i < world.ColliderMeshes.Length; i++) w.ColliderMeshes[i] = FromNwColliderMesh(world.ColliderMeshes[i]);

            w.Materials = new Material[world.Materials.Length];
            for (var i = 0; i < world.Materials.Length; i++) w.Materials[i] = FromNwMaterial(world.Materials[i], ref world.Texture2Ds, ref w.Texture2Ds);

            w.Environments = new Assets.Environment[world.Environments.Length];
            for (var i = 0; i < world.Environments.Length; i++)
            {
                var idx = Array.FindIndex(world.Cubemaps, tex => tex.Hash == world.Environments[i].EnvMap);
                w.Environments[i] = FromNwEnvironment(world.Environments[i], idx >= 0 ? w.TextureCubes[idx] : null);
            }

            return w;
        }
        public static Character FromNwCharacter(NwCharacter ch)
        {
            var c = new Character();
            c.nwCharacter = ch;
            c.Name = ch.Name;
            c.Texture2Ds = new Texture2D[ch.Texture2Ds.Length];
            for (var i = 0; i < ch.Texture2Ds.Length; i++)
            {
                c.Texture2Ds[i] = FromNwTexture2D(ch.Texture2Ds[i]);
                c.Texture2Ds[i].UseMipmap = true;
                c.Texture2Ds[i].MinFilter = OpenTK.Graphics.OpenGL4.TextureMinFilter.LinearMipmapLinear;
                c.Texture2Ds[i].MagFilter = OpenTK.Graphics.OpenGL4.TextureMagFilter.Linear;
                c.Texture2Ds[i].WrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat;
            }

            c.TextureCubes = new TextureCube[ch.Cubemaps.Length];
            for (var i = 0; i < ch.Cubemaps.Length; i++)
            {
                c.TextureCubes[i] = FromNwTextureCube(ch.Cubemaps[i]);
                c.TextureCubes[i].UseMipmap = true;
                c.TextureCubes[i].MinFilter = OpenTK.Graphics.OpenGL4.TextureMinFilter.LinearMipmapLinear;
                c.TextureCubes[i].MagFilter = OpenTK.Graphics.OpenGL4.TextureMagFilter.Linear;
                c.TextureCubes[i].WrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat;
            }

            if (ch.Bones != null && ch.Bones.Length > 0)
            {
                c.Bones = FromNwBones(ch.Bones);
            }

            c.Mesh = FromNwMesh(ch.Mesh);

            c.Materials = new Material[ch.Materials.Length];
            for (var i = 0; i < ch.Materials.Length; i++) c.Materials[i] = FromNwMaterial(ch.Materials[i], ref ch.Texture2Ds, ref c.Texture2Ds);

            if (ch.EyePosition == null) ch.EyePosition = new Vector3f(0.0f, 1.4f, 0.1f);
            c.EyePosition = new Vector3(ch.EyePosition.X, ch.EyePosition.Y, ch.EyePosition.Z);
            c.PhysicalMaterial = new PhysicalMaterial()
            {
                Mass = ch.Mass,
            };

            c.Sounds = new Sound[ch.Sounds.Length];
            for (var i = 0; i < c.Sounds.Length; i++) c.Sounds[i] = FromNwSound(ch.Sounds[i]);

            c.Motions = new Motion[ch.Motions.Length];
            for (var i = 0; i < c.Motions.Length; i++)
            {
                c.Motions[i] = FromNwMotion(ch.Motions[i]);
            }

            var scripts = new List<Assembly>();
            foreach (var s in ch.Scripts)
            {
                try
                {
                    var asm = Assembly.Load(s.Assembly);
                    scripts.Add(asm);
                }
                catch { }
            }
            c.Scripts = scripts.ToArray();

            c.Properties = ch.Properties;

            c.CollisionShape = FromNwCollisionShape(ch.CollisionShape);

            return c;
        }
        public static WorldObject FromNwObject(NwObject obj)
        {
            var o = new WorldObject();
            o.nwObject = obj;
            o.Name = obj.Name;
            o.Desc = obj.Description;
            o.Hash = obj.Hash;
            o.ItemType = obj.ItemType;
            o.MaxStack = obj.MaxStack;
            o.Price = (int)obj.ItemPrice;
            o.Consume = obj.ItemConsume;
            o.Sync = obj.Sync;
            o.Purchasable = obj.Purchasable;

            if (obj.Thumbnail != null) o.Icon = Util.ToBitmap(obj.Thumbnail.Image);

            if (!string.IsNullOrWhiteSpace(obj.Tags)) o.Tags = obj.Tags.Split(',');

            o.Texture2Ds = new Texture2D[obj.Texture2Ds.Length];
            for (var i = 0; i < obj.Texture2Ds.Length; i++)
            {
                o.Texture2Ds[i] = FromNwTexture2D(obj.Texture2Ds[i]);
                o.Texture2Ds[i].UseMipmap = true;
                o.Texture2Ds[i].MinFilter = OpenTK.Graphics.OpenGL4.TextureMinFilter.LinearMipmapLinear;
                o.Texture2Ds[i].MagFilter = OpenTK.Graphics.OpenGL4.TextureMagFilter.Linear;
                o.Texture2Ds[i].WrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat;
            }

            o.TextureCubes = new TextureCube[obj.Cubemaps.Length];
            for (var i = 0; i < obj.Cubemaps.Length; i++)
            {
                o.TextureCubes[i] = FromNwTextureCube(obj.Cubemaps[i]);
                o.TextureCubes[i].UseMipmap = true;
                o.TextureCubes[i].MinFilter = OpenTK.Graphics.OpenGL4.TextureMinFilter.LinearMipmapLinear;
                o.TextureCubes[i].MagFilter = OpenTK.Graphics.OpenGL4.TextureMagFilter.Linear;
                o.TextureCubes[i].WrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat;
            }

            if (obj.Bones != null && obj.Bones.Length > 0)
            {
                o.Bones = FromNwBones(obj.Bones);
            }

            o.Mesh = FromNwMesh(obj.Mesh);

            o.Materials = new Material[obj.Materials.Length];
            for (var i = 0; i < obj.Materials.Length; i++) o.Materials[i] = FromNwMaterial(obj.Materials[i], ref obj.Texture2Ds, ref o.Texture2Ds);

            o.Sounds = new Sound[obj.Sounds.Length];
            for (var i = 0; i < o.Sounds.Length; i++) o.Sounds[i] = FromNwSound(obj.Sounds[i]);

            if (obj.Thumbnail != null) o.Thumbnail = FromNwTexture2D(obj.Thumbnail);

            o.CollisionShape = FromNwCollisionShape(obj.CollisionShape);
            o.PhysicalMaterial = FromNwPhysicalMaterial(obj.PhysicalMaterial);

            o.Motions = new Motion[obj.Motions.Length];
            for (var i = 0; i < o.Motions.Length; i++)
            {
                o.Motions[i] = FromNwMotion(obj.Motions[i]);
            }

            var scripts = new List<Assembly>();
            foreach (var s in obj.Scripts)
            {
                try
                {
                    var asm = Assembly.Load(s.Assembly);
                    scripts.Add(asm);
                }
                catch { }
            }
            o.Scripts = scripts.ToArray();

            o.Properties = obj.Properties;

            return o;
        }

        public static NwBone ToNwBone(Bone bone)
        {
            var b = new NwBone();
            b.Name = bone.Name;
            b.Index = bone.Index;
            if (bone.Parent != null) b.Parent = bone.Parent.Index;
            b.Position = bone.Position.ToVec3f();
            b.Type = bone.BoneType;

            if (bone.IKLinks != null)
            {
                b.IKLinks = new NwIKLink[bone.IKLinks.Length];
                for (var i = 0; i < b.IKLinks.Length; i++)
                {
                    b.IKLinks[i] = new NwIKLink()
                    {
                        Bone = bone.IKLinks[i].Bone.Index,
                        LimitAngle = bone.IKLinks[i].LimitAngle,
                        LowerLimitAngle = bone.IKLinks[i].LowerLimitAngle.ToVec3f(),
                        UpperLimitAngle = bone.IKLinks[i].UpperLimitAngle.ToVec3f(),
                    };

                    if (bone.IKLinks[i].Bone != null) b.IKLinks[i].Bone = bone.IKLinks[i].Bone.Index;
                }
            }
            b.IKLoop = bone.IKLoop;
            b.IKRotLimit = bone.IKRotLimit;
            if (bone.IKTarget != null) b.IKTarget = bone.IKTarget.Index;

            return b;
        }
        public static NwMesh ToNwMesh(Mesh mesh)
        {
            var m = new NwMesh();
            m.Name = mesh.Name;

            m.Vertices = Conv(mesh.Vertices, (v) => new Vector3f(v.X, v.Y, v.Z));
            m.Normals = Conv(mesh.Normals, (v) => new Vector3f(v.X, v.Y, v.Z));
            m.Colors = Conv(mesh.Colors, (v) => new Color4f(v.R, v.G, v.B, v.A));
            m.UVs = Conv(mesh.UVs, (v) => new Vector2f(v.X, v.Y));
            m.UV1s = Conv(mesh.UV1s, (v) => new Vector4f(v.X, v.Y, v.Z, v.W));
            m.UV2s = Conv(mesh.UV2s, (v) => new Vector4f(v.X, v.Y, v.Z, v.W));
            m.UV3s = Conv(mesh.UV3s, (v) => new Vector4f(v.X, v.Y, v.Z, v.W));
            m.UV4s = Conv(mesh.UV4s, (v) => new Vector4f(v.X, v.Y, v.Z, v.W));
            m.BoneWeights = Conv(mesh.BoneWeights, (v) => new BoneWeight4f()
            {
                Index0 = v.boneIndex0,
                Index1 = v.boneIndex1,
                Index2 = v.boneIndex2,
                Index3 = v.boneIndex3,
                Weight0 = v.weight0,
                Weight1 = v.weight1,
                Weight2 = v.weight2,
                Weight3 = v.weight3,
            });
            m.SubMeshes = Conv(mesh.subMeshes.ToArray(), (v) => new NwSubMesh()
            {
                MatIndex = v.materialIndex,
                Indices = v.indices,
                BeginMode = (int)v.mode,
            });
            
            return m;
        }
        public static NwColliderMesh ToNwColliderMesh(Mesh mesh)
        {
            var cm = new NwColliderMesh();

            cm.Vertices = Conv(mesh.Vertices, (v) => new Vector3f(v.X, v.Y, v.Z));
           
            var indices = new List<int>();
            foreach (var sm in mesh.subMeshes)
            {
                indices.AddRange(sm.indices);
            }

            cm.Indices = indices.ToArray();

            return cm;
        }
        public static NwTexture2D ToNwTexture2D(Bitmap bitmap, string name)
        {
            var bm = new NwTexture2D();

            bm.Name = name;
            bm.Image = Util.FromBitmap(bitmap);
            bm.Hash = Util.ComputeHash(bm.Image, 12);

            return bm;
        }
        public static NwCubemap ToNwTextureCube(Bitmap[] bitmaps, string name)
        {
            var cube = new NwCubemap();
            cube.Name = name;
            cube.ImagePX = Util.FromBitmap(bitmaps[0]);
            cube.ImagePY = Util.FromBitmap(bitmaps[1]);
            cube.ImagePZ = Util.FromBitmap(bitmaps[2]);
            cube.ImageNX = Util.FromBitmap(bitmaps[3]);
            cube.ImageNY = Util.FromBitmap(bitmaps[4]);
            cube.ImageNZ = Util.FromBitmap(bitmaps[5]);

            cube.Hash = Util.ComputeHash(
                cube.ImagePX
                .Concat(cube.ImagePY)
                .Concat(cube.ImagePZ)
                .Concat(cube.ImageNX)
                .Concat(cube.ImageNY)
                .Concat(cube.ImageNZ)
                .ToArray()
                , 12);

            return cube;
        }
        public static NwMaterial ToNwMaterial(Material m, string name, ref List<NwTexture2D> bitmaps, ref List<NwCubemap> cubemaps)
        {
            var mat = new NwMaterial();
            mat.Name = name;

            foreach (var v in m.floatParams) mat.Floats.Add(v.Key, v.Value.value);
            foreach (var v in m.vec2Params) mat.Vector2s.Add(v.Key, v.Value.value.ToVec2f());
            foreach (var v in m.vec3Params) mat.Vector3s.Add(v.Key, v.Value.value.ToVec3f());
            foreach (var v in m.vec4Params) mat.Vector4s.Add(v.Key, v.Value.value.ToVec4f());
            foreach (var v in m.colorParams) mat.Color4s.Add(v.Key, v.Value.value.ToColor4f());
            foreach (var v in m.mat4Params) mat.Matrix4s.Add(v.Key, v.Value.value.ToMat4f());
            foreach (var v in m.tex2DParams)
            {
                if (v.Value.value != null)
                {
                    var bm = ToNwTexture2D(v.Value.value.SrcBitmap, null);
                    if (!bitmaps.Exists(b => b.Hash == bm.Hash)) bitmaps.Add(bm);
                    mat.Texture2Ds.Add(v.Key, bm.Hash);
                }
                else mat.Texture2Ds.Add(v.Key, null);
            }

            return mat;
        }
        public static NwEnvironment ToNwEnvironment(Assets.Environment env)
        {
            var e = new NwEnvironment()
            {
                Ambient = env.Ambient.ToColor4f(),
                CastShadow = env.CastShadow,
                ColorCollect = new NwColorCollect()
                {
                    Brightness = env.ColorCollect.Brightness,
                    Contrast = env.ColorCollect.Contrast,
                    Gamma = env.ColorCollect.Gamma,
                    Hue = env.ColorCollect.Hue,
                    Saturation = env.ColorCollect.Saturation,
                },
                DirLight = new NwDirectionalLight()
                {
                    Color = env.DirLightColor.ToColor4f(),
                    Direction = env.DirLightDir.ToVec3f(),
                    Intensity = env.DirLightIntensity,
                },
                EnvMap = GetHash(env.EnvMap),
            };

            return e;
        }

        public static Bone[] FromNwBones(NwBone[] bones)
        {
            var bs = new Bone[bones.Length];
            for (var i = 0; i < bs.Length; i++)
            {
                bs[i] = new Bone()
                {
                    BoneType = bones[i].Type,
                    IKLoop = bones[i].IKLoop,
                    IKRotLimit = bones[i].IKRotLimit,
                    Index = bones[i].Index,
                    Name = bones[i].Name,
                    Position = bones[i].Position.FromVec3f(),
                };

                if (bones[i].IKLinks != null && bones[i].IKLinks.Length > 0)
                {
                    bs[i].IKLinks = new IKLink[bones[i].IKLinks.Length];
                    for (var j = 0; j < bones[i].IKLinks.Length; j++)
                    {
                        bs[i].IKLinks[j] = new IKLink();
                        bs[i].IKLinks[j].LimitAngle = bones[i].IKLinks[j].LimitAngle;
                        bs[i].IKLinks[j].LowerLimitAngle = bones[i].IKLinks[j].LowerLimitAngle.FromVec3f();
                        bs[i].IKLinks[j].UpperLimitAngle = bones[i].IKLinks[j].UpperLimitAngle.FromVec3f();
                    }
                }
            }

            for (var i = 0; i < bs.Length; i++)
            {
                if (bones[i].Parent != -1) bs[i].Parent = bs[bones[i].Parent];
                if (bones[i].IKTarget != -1) bs[i].IKTarget = bs[bones[i].IKTarget];
                

                if (bs[i].IKLinks != null)
                {
                    for (var j = 0; j < bs[i].IKLinks.Length; j++)
                    {
                        bs[i].IKLinks[j].Bone = bs[bones[i].IKLinks[j].Bone];
                    }
                }
            }

            for (var i = 0; i < bs.Length; i++)
            {
                var children = Array.FindAll(bs, b => b.Parent != null && b.Parent.Index == bs[i].Index);
                if (children.Length > 0)
                    bs[i].Children = children;
            }

            return bs;
        }
        public static Mesh FromNwMesh(NwMesh mesh)
        {
            var m = new Mesh();
            m.Name = mesh.Name;

            m.Vertices = Conv(mesh.Vertices, (v) => new Vector3(v.X, v.Y, v.Z));
            m.Normals = Conv(mesh.Normals, (v) => new Vector3(v.X, v.Y, v.Z));
            m.Colors = Conv(mesh.Colors, (v) => new Color4(v.R, v.G, v.B, v.A));
            m.UVs = Conv(mesh.UVs, (v) => new Vector2(v.X, v.Y));
            m.UV1s = Conv(mesh.UV1s, (v) => new Vector4(v.X, v.Y, v.Z, v.W));
            m.UV2s = Conv(mesh.UV2s, (v) => new Vector4(v.X, v.Y, v.Z, v.W));
            m.UV3s = Conv(mesh.UV3s, (v) => new Vector4(v.X, v.Y, v.Z, v.W));
            m.UV4s = Conv(mesh.UV4s, (v) => new Vector4(v.X, v.Y, v.Z, v.W));
            m.BoneWeights = Conv(mesh.BoneWeights, (v) => new BoneWeight()
            {
                boneIndex0 = v.Index0,
                boneIndex1 = v.Index1,
                boneIndex2 = v.Index2,
                boneIndex3 = v.Index3,
                weight0 = v.Weight0,
                weight1 = v.Weight1,
                weight2 = v.Weight2,
                weight3 = v.Weight3,
            });
            foreach (var sub in mesh.SubMeshes)
            {
                m.SetIndices(sub.MatIndex, sub.Indices, (OpenTK.Graphics.OpenGL4.BeginMode)sub.BeginMode);
            }

            return m;
        }
        public static ColliderMesh FromNwColliderMesh(NwColliderMesh mesh)
        {
            var m = new ColliderMesh();

            m.Vertices = Conv(mesh.Vertices, (v) => new Vector3(v.X, v.Y, v.Z));
            m.Indices = mesh.Indices;

            return m;
        }
        public static Texture2D FromNwTexture2D(NwTexture2D tex)
        {
            var bm = Util.ToBitmap(tex.Image);

            var t = new Texture2D(bm, tex.Name, false);

            return t;
        }
        public static TextureCube FromNwTextureCube(NwCubemap tex)
        {
            var px = Util.ToBitmap(tex.ImagePX);
            var py = Util.ToBitmap(tex.ImagePY);
            var pz = Util.ToBitmap(tex.ImagePZ);
            var nx = Util.ToBitmap(tex.ImageNX);
            var ny = Util.ToBitmap(tex.ImageNY);
            var nz = Util.ToBitmap(tex.ImageNZ);

            var t = new TextureCube(nx, ny, nz, px, py, pz, tex.Name);

            return t;
        }
        public static Material FromNwMaterial(NwMaterial mat, ref NwTexture2D[] texs, ref Texture2D[] texs2)
        {
            var m = new Material();

            foreach (var v in mat.Floats) m.AddParam(v.Key, v.Value);
            foreach (var v in mat.Vector2s) m.AddParam(v.Key, v.Value.FromVec2f());
            foreach (var v in mat.Vector3s) m.AddParam(v.Key, v.Value.FromVec3f());
            foreach (var v in mat.Vector4s) m.AddParam(v.Key, v.Value.FromVec4f());
            foreach (var v in mat.Color4s) m.AddParam(v.Key, v.Value.FromColor4f());
            foreach (var v in mat.Matrix4s) m.AddParam(v.Key, v.Value.FromMat4f());
            foreach (var v in mat.Texture2Ds)
            {
                var idx = Array.FindIndex(texs, t => t.Hash == v.Value);
                if (idx != -1) m.AddParam(v.Key, texs2[idx]);
                else m.AddParam<Texture2D>(v.Key, null);
            }

            return m;
        }
        public static Sound FromNwSound(NwSound s)
        {
            var sound = new Sound(new MemoryStream(s.Data), s.Format);
            return sound;
        }
        public static Assets.Environment FromNwEnvironment(NwEnvironment env, TextureCube envMap)
        {
            var e = new Assets.Environment()
            {
                Ambient = new Color4(env.Ambient.R, env.Ambient.G, env.Ambient.B, env.Ambient.A),
                CastShadow = env.CastShadow,
                ColorCollect = new ColorCollect()
                {
                    Brightness = env.ColorCollect.Brightness,
                    Contrast = env.ColorCollect.Contrast,
                    Gamma = env.ColorCollect.Gamma,
                    Hue = env.ColorCollect.Hue,
                    Saturation = env.ColorCollect.Saturation,
                },
                DirLightColor = new Color4(env.DirLight.Color.R, env.DirLight.Color.G, env.DirLight.Color.B, env.DirLight.Color.A),
                DirLightDir = new Vector3(env.DirLight.Direction.X, env.DirLight.Direction.Y, env.DirLight.Direction.Z),
                DirLightIntensity = env.DirLight.Intensity,

                EnvMap = envMap,
            };

            return e;
        }
        public static PhysicalMaterial FromNwPhysicalMaterial(NwPhysicalMaterial mat)
        {
            if (mat == null) return null;
            var pm = new PhysicalMaterial();
            pm.AngulerDamping = mat.AngulerDamping;
            pm.AnisotropicFriction = mat.AnisotropicFriction.FromVec3f();
            pm.FreezePosition = mat.FreezePosition;
            pm.FreezeRotation = mat.FreezeRotation;
            pm.Friction = mat.Friction;
            pm.Group = mat.Group;
            pm.LinearDamping = mat.LinearDamping;
            pm.Mask = mat.Mask;
            pm.Mass = mat.Mass;
            pm.Restitution = mat.Restitution;
            pm.RollingFriction = mat.RollingFriction;
            pm.DisableDeactivation = mat.DisableDeactivation;
            pm.Kinematic = mat.Kinematic;
            pm.IsRigidBody = mat.IsRigidBody;

            return pm;
        }
        public static CollisionShape FromNwCollisionShape(NwCollisionShape shape)
        {
            if (shape is NwCollisionCapsule)
            {
                var c = shape as NwCollisionCapsule;
                return new CollisionCapsule() { Height = c.Height, Radius = c.Radius };
            }
            else if (shape is NwCollisionCylinder)
            {
                var c = shape as NwCollisionCylinder;
                return new CollisionCylinder() { Height = c.Height, Radius = c.Radius };
            }
            else if (shape is NwCollisionBox)
            {
                var c = shape as NwCollisionBox;
                return new CollisionBox() { HalfExtents = c.HalfExtents.FromVec3f() };
            }
            else if (shape is NwCollisionSphere)
            {
                var c = shape as NwCollisionSphere;
                return new CollisionSphere() { Radius = c.Radius };
            }
            else if (shape is NwCollisionMesh)
            {
                var c = shape as NwCollisionMesh;
                return new CollisionMesh() { MeshIndices = c.MeshIndices.ToArray() };
            }

            return null;
        }
        public static Motion FromNwMotion(NwMotion motion)
        {
            var m = new Motion();
            m.Name = motion.Key;

            if (motion.BoneMotion != null)
            {
                m.BoneMotions = new Dictionary<string, BoneMotion>();
                foreach (var bm in motion.BoneMotion.Values)
                {
                    var bmv = new BoneMotion();
                    bmv.BoneName = bm.BoneName;
                    bmv.Keys = new List<KeyFrame<BoneMotionValue>>();
                    foreach (var key in bm.Keys)
                    {
                        var f = new KeyFrame<BoneMotionValue>();
                        f.FrameNo = key.FrameNo;
                        if (key.FrameNo > m.FrameNoMax) m.FrameNoMax = key.FrameNo;
                        f.Interpolate = new BezierInterpolate() { p1 = key.Interpolate.P1.FromVec2f(), p2 = key.Interpolate.P2.FromVec2f() };
                        var r = key.Rotation.FromVec4f();
                        f.Value = new BoneMotionValue()
                        {
                            location = key.Location.FromVec3f(),
                            rotation = new Quaternion(r.X, r.Y, r.Z, r.W),
                            scale = key.Scale.FromVec3f(),
                        };
                        bmv.Keys.Add(f);
                    }

                    m.BoneMotions.Add(bm.BoneName, bmv);
                }
            }

            if (motion.MorphMotion != null)
            {
                m.SkinMotions = new Dictionary<string, SkinMotion>();
                foreach (var mm in motion.MorphMotion.Values)
                {
                    var smv = new SkinMotion();
                    smv.MorphName = mm.MorphName;
                    smv.Keys = new List<KeyFrame<float>>();
                    foreach (var key in mm.Keys)
                    {
                        var f = new KeyFrame<float>();
                        f.FrameNo = key.FrameNo;
                        if (key.FrameNo > m.FrameNoMax) m.FrameNoMax = key.FrameNo;
                        f.Interpolate = new BezierInterpolate() { p1 = key.Interpolate.P1.FromVec2f(), p2 = key.Interpolate.P2.FromVec2f() };
                        f.Value = key.Rate;
                        smv.Keys.Add(f);
                    }

                    m.SkinMotions.Add(mm.MorphName, smv);
                }
            }

            return m;
        }

        private static string ToCompString<T>(T[] src, Action<T, BinaryWriter> conv)
        {
            if (src == null) return null;
            byte[] buf = null;
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    foreach (var v in src)
                    {
                        conv(v, bw);
                    }
                }
                buf = ms.ToArray();
            }

            return Util.ToCompString(buf);
        }
        private static byte[] ToCompBytes<T>(T[] src, Action<T, BinaryWriter> conv)
        {
            if (src == null) return null;
            byte[] buf = null;
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    foreach (var v in src)
                    {
                        conv(v, bw);
                    }
                }
                buf = ms.ToArray();
            }

            return Util.Compress(buf);
        }

        private static T2[] Conv<T1, T2>(T1[] src, Func<T1, T2> conv)
        {
            if (src == null) return null;
            T2[] data = new T2[src.Length];

            for (var i = 0; i < src.Length; i++)
            {
                data[i] = conv(src[i]);
            }

            return data;
        }

        public static string GetHash(Texture2D tex)
        {
            return ToNwTexture2D(tex.SrcBitmap, null).Hash;
        }
        public static string GetHash(TextureCube tex)
        {
            return ToNwTextureCube(new Bitmap[]
            {
                tex.SrcBitmapPosX,
                tex.SrcBitmapPosY,
                tex.SrcBitmapPosZ,
                tex.SrcBitmapNegX,
                tex.SrcBitmapNegY,
                tex.SrcBitmapNegZ
            }, null).Hash;
        }

        public static Vector2f ToVec2f(this OpenTK.Vector2 v)
        {
            return new Vector2f(v.X, v.Y);
        }
        public static Vector3f ToVec3f(this OpenTK.Vector3 v)
        {
            return new Vector3f(v.X, v.Y, v.Z);
        }
        public static Vector4f ToVec4f(this OpenTK.Vector4 v)
        {
            return new Vector4f(v.X, v.Y, v.Z, v.W);
        }
        public static Color4f ToColor4f(this Color4 c)
        {
            return new Color4f(c.R, c.G, c.B, c.A);
        }
        public static Matrix4f ToMat4f(this OpenTK.Matrix4 m)
        {
            return new Matrix4f()
            {
                M00 = m.M11,
                M01 = m.M12,
                M02 = m.M13,
                M03 = m.M14,

                M10 = m.M21,
                M11 = m.M22,
                M12 = m.M23,
                M13 = m.M24,

                M20 = m.M31,
                M21 = m.M32,
                M22 = m.M33,
                M23 = m.M34,

                M30 = m.M41,
                M31 = m.M42,
                M32 = m.M43,
                M33 = m.M44,
            };
        }

        public static Vector2 FromVec2f(this Vector2f v)
        {
            return new Vector2(v.X, v.Y);
        }
        public static Vector3 FromVec3f(this Vector3f v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }
        public static Vector4 FromVec4f(this Vector4f v)
        {
            return new Vector4(v.X, v.Y, v.Z, v.W);
        }
        public static Color4 FromColor4f(this Color4f c)
        {
            return new Color4(c.R, c.G, c.B, c.A);
        }
        public static Matrix4 FromMat4f(this Matrix4f m)
        {
            return new Matrix4()
            {
                M11 = m.M00,
                M12 = m.M01,
                M13 = m.M02,
                M14 = m.M03,

                M21 = m.M10,
                M22 = m.M11,
                M23 = m.M12,
                M24 = m.M13,

                M31 = m.M20,
                M32 = m.M21,
                M33 = m.M22,
                M34 = m.M23,

                M41 = m.M30,
                M42 = m.M31,
                M43 = m.M32,
                M44 = m.M33,
            };
        }
    }
}
