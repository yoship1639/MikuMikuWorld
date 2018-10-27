using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Network
{
    public class NwMaterial
    {
        public string Name { get; set; }

        public Dictionary<string, float> Floats = new Dictionary<string, float>();
        public Dictionary<string, Vector2f> Vector2s = new Dictionary<string, Vector2f>();
        public Dictionary<string, Vector3f> Vector3s = new Dictionary<string, Vector3f>();
        public Dictionary<string, Vector4f> Vector4s = new Dictionary<string, Vector4f>();
        public Dictionary<string, Color4f> Color4s = new Dictionary<string, Color4f>();
        public Dictionary<string, Matrix4f> Matrix4s = new Dictionary<string, Matrix4f>();
        public Dictionary<string, string> Texture2Ds = new Dictionary<string, string>();
        public Dictionary<string, string> TextureCubes = new Dictionary<string, string>();
    }
}
