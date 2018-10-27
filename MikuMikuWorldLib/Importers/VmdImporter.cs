using MikuMikuWorld.Assets;
using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VmdMotionImporter;

namespace MikuMikuWorld.Importers
{
    public class VmdImporter : IImporter
    {
        public bool DirectoryImporter => false;

        public string[] Extensions => new string[]
        {
            ".vmd",
        };

        public float ImportScale { get; set; } = 0.0795f;

        public ImportedObject[] Import(string path, ImportType type)
        {
            ImportedObject obj = new ImportedObject()
            {
                Result = Result.Failed,
                Type = ImportedObjectType.Motion,
                Path = path,
            };

            VmdImportResult res;
            {
                var importer = new VmdMotionImporter.VmdMotionImporter();
                res = importer.Import(path, type == ImportType.Full);
                if (res == null || res.result != VmdImportResult.Result.Success)
                {
                    return new ImportedObject[] { obj };
                }
            }

            var vmd = res.vmd;
            
            obj.Result = Result.Success;
            obj.Name = Path.GetFileNameWithoutExtension(path);
            obj.Property = new ImportedProperty()
            {
                Name = vmd.Header.ModelName
            };

            if (type == ImportType.OverviewOnly)
            {
                obj.Result = Result.Success;
                return new ImportedObject[] { obj };
            }

            var anim = new Motion();
            anim.Name = obj.Name;
            var max = 0;

            // bone
            {
                var dic = new Dictionary<string, BoneMotion>();
                foreach (var m in vmd.MotionList.Motions)
                {
                    BoneMotion bm = null;
                    if (!dic.ContainsKey(m.Name))
                    {
                        dic.Add(m.Name, bm = new BoneMotion()
                        {
                            BoneName = m.Name,
                            Keys = new List<KeyFrame<BoneMotionValue>>(),
                        });
                    }
                    bm = dic[m.Name];

                    var key = new KeyFrame<BoneMotionValue>();
                    key.FrameNo = (int)m.FrameNo;
                    if (key.FrameNo > max) max = key.FrameNo;
                    key.Value = new BoneMotionValue()
                    {
                        location = m.Location.ToVec3(true) * ImportScale,
                        rotation = m.Rotation.ToQuaternion(),
                        scale = OpenTK.Vector3.One,
                    };

                    key.Interpolate = new BezierInterpolate()
                    {
                        p1 = new OpenTK.Vector2(m.BezierX1.W, m.BezierY1.W),
                        p2 = new OpenTK.Vector2(m.BezierX2.W, m.BezierY2.W),
                    };
                    
                    bm.Keys.Add(key);
                }
                foreach (var bm in dic.Values)
                {
                    bm.Keys.Sort((k1, k2) => { return k1.FrameNo - k2.FrameNo; });
                }
                anim.BoneMotions = dic;
            }

            // skin
            {
                var dic = new Dictionary<string, SkinMotion>();
                foreach (var s in vmd.SkinList.Skins)
                {
                    SkinMotion sm = null;
                    if (!dic.ContainsKey(s.Name))
                    {
                        dic.Add(s.Name, sm = new SkinMotion()
                        {
                            MorphName = s.Name,
                            Keys = new List<KeyFrame<float>>(),
                        });
                    }
                    sm = dic[s.Name];

                    var key = new KeyFrame<float>();
                    key.FrameNo = (int)s.FrameNo;
                    if (key.FrameNo > max) max = key.FrameNo;
                    key.Value = s.Weight;

                    key.Interpolate = Interpolates.Smoothstep;
                    
                    sm.Keys.Add(key);
                }
                foreach (var sm in dic.Values)
                {
                    sm.Keys.Sort((k1, k2) => { return k1.FrameNo - k2.FrameNo; });
                }
                anim.SkinMotions = dic;
            }

            anim.FrameNoMax = max;

            obj.Motions = new Motion[] { anim };
            obj.Result = Result.Success;
            return new ImportedObject[] { obj };
        }
    }

    static class VmdExtension
    {
        public static OpenTK.Vector2 ToVec2(this VmdMotionImporter.Vector2 v)
        {
            return new OpenTK.Vector2(v.X, v.Y);
        }

        public static OpenTK.Vector3 ToVec3(this VmdMotionImporter.Vector3 v, bool flipZ = false)
        {
            return new OpenTK.Vector3(v.X, v.Y, flipZ ? -v.Z : v.Z);
        }

        public static OpenTK.Vector4 ToVec4(this VmdMotionImporter.Vector4 v, bool flipZ = false)
        {
            return new OpenTK.Vector4(v.X, v.Y, flipZ ? -v.Z : v.Z, v.W);
        }

        public static OpenTK.Quaternion ToQuaternion(this VmdMotionImporter.Vector4 v, bool flipZ = false)
        {
            return new OpenTK.Quaternion(-v.X, -v.Y, flipZ ? -v.Z : v.Z, v.W);
        }
    }
}
