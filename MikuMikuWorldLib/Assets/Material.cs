using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Assets
{
    public class Material : IAsset
    {
        public bool Loaded { get; private set; } = true;
        public string Name { get; set; }
        public Result Load() => Result.Success;
        public Result Unload() => Result.Success;

        internal Material srcMat;

        private Shader shader;
        public Shader Shader
        {
            get { return shader; }
            set
            {
                shader = value;
                if (shader != null)
                {
                    shader.InitMaterialParameter(this);
                    if (shader is GLSLShader) SetParameterLocation();
                } 
            }
        }

        private void SetParameterLocation()
        {
            var sh = (GLSLShader)shader;

            foreach (var p in floatParams.Values)
            {
                p.location = sh.GetUniformLocation(p.name);
            }
            foreach (var p in vec2Params.Values)
            {
                p.location = sh.GetUniformLocation(p.name);
            }
            foreach (var p in vec3Params.Values)
            {
                p.location = sh.GetUniformLocation(p.name);
            }
            foreach (var p in vec4Params.Values)
            {
                p.location = sh.GetUniformLocation(p.name);
            }
            foreach (var p in colorParams.Values)
            {
                p.location = sh.GetUniformLocation(p.name);
            }
            foreach (var p in mat4Params.Values)
            {
                p.location = sh.GetUniformLocation(p.name);
            }
            foreach (var p in tex2DParams.Values)
            {
                p.unit = sh.GetTextureUnit(p.name);
            }
        }

        internal Dictionary<string, MaterialParam<float>> floatParams = new Dictionary<string, MaterialParam<float>>();
        internal Dictionary<string, MaterialParam<Vector2>> vec2Params = new Dictionary<string, MaterialParam<Vector2>>();
        internal Dictionary<string, MaterialParam<Vector3>> vec3Params = new Dictionary<string, MaterialParam<Vector3>>();
        internal Dictionary<string, MaterialParam<Vector4>> vec4Params = new Dictionary<string, MaterialParam<Vector4>>();
        internal Dictionary<string, MaterialParam<Color4>> colorParams = new Dictionary<string, MaterialParam<Color4>>();
        internal Dictionary<string, MaterialParam<Matrix4>> mat4Params = new Dictionary<string, MaterialParam<Matrix4>>();
        internal Dictionary<string, MaterialParam<Texture2D>> tex2DParams = new Dictionary<string, MaterialParam<Texture2D>>();

        public Material() : this("Material", null) { }
        public Material(string name) : this(name, null) { }
        public Material(string name, Shader shader)
        {
            Name = name;
            Shader = shader;
        }

        public void AddParam<T>(string name, T value, object tag = null)
        {
            object obj = value;
            var type = typeof(T);
            int loc = -1;
            if (type != typeof(Texture2D) && shader != null && shader is GLSLShader) loc = ((GLSLShader)shader).GetUniformLocation(name);
            if (type == typeof(float)) floatParams.Add(name, new MaterialParam<float>() { name = name, value = (float)obj, location = loc });
            else if (type == typeof(Vector2)) vec2Params.Add(name, new MaterialParam<Vector2>() { name = name, value = (Vector2)obj, location = loc });
            else if (type == typeof(Vector3)) vec3Params.Add(name, new MaterialParam<Vector3>() { name = name, value = (Vector3)obj, location = loc });
            else if (type == typeof(Vector4)) vec4Params.Add(name, new MaterialParam<Vector4>() { name = name, value = (Vector4)obj, location = loc });
            else if (type == typeof(Color4)) colorParams.Add(name, new MaterialParam<Color4>() { name = name, value = (Color4)obj, location = loc });
            else if (type == typeof(Matrix4)) mat4Params.Add(name, new MaterialParam<Matrix4>() { name = name, value = (Matrix4)obj, location = loc });
            else if (type == typeof(Texture2D))
            {
                TextureUnit unit = TextureUnit.Texture31;
                if (shader != null && shader is GLSLShader) unit = ((GLSLShader)shader).GetTextureUnit(name);
                tex2DParams.Add(name, new MaterialParam<Texture2D>() { name = name, value = (Texture2D)obj, unit = unit, tag = tag });
            } 
        }
        public void AddParam<T>(string name, string semantic, T value, object tag = null)
        {
            object obj = value;
            int loc = -1;
            var type = typeof(T);
            if (type != typeof(Texture2D) && shader != null && shader is GLSLShader) loc = ((GLSLShader)shader).GetUniformLocation(name);
            if (type == typeof(float)) floatParams.Add(name, new MaterialParam<float>() { name = name, value = (float)obj, semantic = semantic, location = loc });
            else if (type == typeof(Vector2)) vec2Params.Add(name, new MaterialParam<Vector2>() { name = name, value = (Vector2)obj, semantic = semantic, location = loc });
            else if (type == typeof(Vector3)) vec3Params.Add(name, new MaterialParam<Vector3>() { name = name, value = (Vector3)obj, semantic = semantic, location = loc });
            else if (type == typeof(Vector4)) vec4Params.Add(name, new MaterialParam<Vector4>() { name = name, value = (Vector4)obj, semantic = semantic, location = loc });
            else if (type == typeof(Color4)) colorParams.Add(name, new MaterialParam<Color4>() { name = name, value = (Color4)obj, semantic = semantic, location = loc });
            else if (type == typeof(Matrix4)) mat4Params.Add(name, new MaterialParam<Matrix4>() { name = name, value = (Matrix4)obj, semantic = semantic, location = loc });
            else if (type == typeof(Texture2D))
            {
                TextureUnit unit = TextureUnit.Texture31;
                if (shader != null && shader is GLSLShader) unit = ((GLSLShader)shader).GetTextureUnit(name);
                tex2DParams.Add(name, new MaterialParam<Texture2D>() { name = name, value = (Texture2D)obj, semantic = semantic, unit = unit, tag = tag });
            }
        }
        public void SetParam<T>(string name, T value)
        {
            object obj = value;
            if (value is float) floatParams[name].value = (float)obj;
            else if (value is Vector2) vec2Params[name].value = (Vector2)obj;
            else if (value is Vector3) vec3Params[name].value = (Vector3)obj;
            else if (value is Vector4) vec4Params[name].value = (Vector4)obj;
            else if (value is Color4) colorParams[name].value = (Color4)obj;
            else if (value is Matrix4) mat4Params[name].value = (Matrix4)obj;
            else if (value is Texture2D) tex2DParams[name].value = (Texture2D)obj;
        }
        public bool TrySetParam<T>(string name, T value)
        {
            if (!HasParam<T>(name)) return false;
            object obj = value;
            if (value is float) floatParams[name].value = (float)obj;
            else if (value is Vector2) vec2Params[name].value = (Vector2)obj;
            else if (value is Vector3) vec3Params[name].value = (Vector3)obj;
            else if (value is Vector4) vec4Params[name].value = (Vector4)obj;
            else if (value is Color4) colorParams[name].value = (Color4)obj;
            else if (value is Matrix4) mat4Params[name].value = (Matrix4)obj;
            else if (value is Texture2D) tex2DParams[name].value = (Texture2D)obj;
            return true;
        }
        public T GetParam<T>(string name)
        {
            var type = typeof(T);
            if (type == typeof(float) && floatParams.ContainsKey(name)) return (T)(object)floatParams[name].value;
            else if (type == typeof(Vector2) && vec2Params.ContainsKey(name)) return (T)(object)vec2Params[name].value;
            else if (type == typeof(Vector3) && vec3Params.ContainsKey(name)) return (T)(object)vec3Params[name].value;
            else if (type == typeof(Vector4) && vec4Params.ContainsKey(name)) return (T)(object)vec4Params[name].value;
            else if (type == typeof(Color4) && colorParams.ContainsKey(name)) return (T)(object)colorParams[name].value;
            else if (type == typeof(Matrix4) && mat4Params.ContainsKey(name)) return (T)(object)mat4Params[name].value;
            else if (type == typeof(Texture2D) && tex2DParams.ContainsKey(name)) return (T)(object)tex2DParams[name].value;
            return default(T);
        }
        public bool HasParam<T>(string name)
        {
            var type = typeof(T);
            if (type == typeof(float)) return floatParams.ContainsKey(name);
            else if (type == typeof(Vector2)) return vec2Params.ContainsKey(name);
            else if (type == typeof(Vector3)) return vec3Params.ContainsKey(name);
            else if (type == typeof(Vector4)) return vec4Params.ContainsKey(name);
            else if (type == typeof(Color4)) return colorParams.ContainsKey(name);
            else if (type == typeof(Matrix4)) return mat4Params.ContainsKey(name);
            else if (type == typeof(Texture2D)) return tex2DParams.ContainsKey(name);
            return false;
        }

        public void ApplyShaderParam()
        {
            if (shader == null) return;

            foreach (var p in floatParams.Values)
            {
                if (p.location != -1) shader.SetParameter(p.location, p.value);
                else if (p.semantic != null) shader.SetParameterBySemantic(p.semantic, p.value);
                else shader.SetParameterByName(p.name, p.value);
            }
            foreach (var p in vec2Params.Values)
            {
                if (p.location != -1) shader.SetParameter(p.location, p.value);
                else if (p.semantic != null) shader.SetParameterBySemantic(p.semantic, p.value);
                else shader.SetParameterByName(p.name, p.value);
            }
            foreach (var p in vec3Params.Values)
            {
                if (p.location != -1) shader.SetParameter(p.location, p.value);
                else if (p.semantic != null) shader.SetParameterBySemantic(p.semantic, p.value);
                else shader.SetParameterByName(p.name, p.value);
            }
            foreach (var p in vec4Params.Values)
            {
                if (p.location != -1) shader.SetParameter(p.location, p.value);
                else if (p.semantic != null) shader.SetParameterBySemantic(p.semantic, p.value);
                else shader.SetParameterByName(p.name, p.value);
            }
            foreach (var p in colorParams.Values)
            {
                if (p.location != -1) shader.SetParameter(p.location, p.value);
                else if (p.semantic != null) shader.SetParameterBySemantic(p.semantic, p.value);
                else shader.SetParameterByName(p.name, p.value);
            }
            foreach (var p in mat4Params.Values)
            {
                if (p.location != -1) shader.SetParameter(p.location, p.value, false);
                else if (p.semantic != null) shader.SetParameterBySemantic(p.semantic, p.value, false);
                else shader.SetParameterByName(p.name, p.value, false);
            }
            foreach (var p in tex2DParams.Values)
            {
                if (p.unit != TextureUnit.Texture31) shader.SetParameter(p.unit, p.value);
                else if (p.semantic != null) shader.SetParameterBySemantic(p.semantic, p.value);
                else shader.SetParameterByName(p.name, p.value);
            }
        }

        public Material Clone()
        {
            var mat = new Material(Name);
            mat.shader = shader;
            mat.srcMat = this;
            foreach (var p in floatParams)
            {
                if (!mat.floatParams.ContainsKey(p.Key)) mat.floatParams.Add(p.Key, p.Value.Clone());
                else mat.floatParams[p.Key] = p.Value;
            } 
            foreach (var p in vec2Params)
            {
                if (!mat.vec2Params.ContainsKey(p.Key)) mat.vec2Params.Add(p.Key, p.Value.Clone());
                else mat.vec2Params[p.Key] = p.Value;
            }
            foreach (var p in vec3Params)
            {
                if (!mat.vec3Params.ContainsKey(p.Key)) mat.vec3Params.Add(p.Key, p.Value.Clone());
                else mat.vec3Params[p.Key] = p.Value;
            }
            foreach (var p in vec4Params)
            {
                if (!mat.vec4Params.ContainsKey(p.Key)) mat.vec4Params.Add(p.Key, p.Value.Clone());
                else mat.vec4Params[p.Key] = p.Value;
            }
            foreach (var p in colorParams)
            {
                if (!mat.colorParams.ContainsKey(p.Key)) mat.colorParams.Add(p.Key, p.Value.Clone());
                else mat.colorParams[p.Key] = p.Value;
            }
            foreach (var p in mat4Params)
            {
                if (!mat.mat4Params.ContainsKey(p.Key)) mat.mat4Params.Add(p.Key, p.Value.Clone());
                else mat.mat4Params[p.Key] = p.Value;
            }
            foreach (var p in tex2DParams)
            {
                if (!mat.tex2DParams.ContainsKey(p.Key)) mat.tex2DParams.Add(p.Key, p.Value.Clone());
                else mat.tex2DParams[p.Key] = p.Value;
            }
            return mat;
        }

        public override string ToString()
        {
            return Name;
        }

        public static Color4 IronColor = new Color4(0.56f, 0.57f, 0.58f, 1.0f);
        public static Color4 SilverColor = new Color4(0.972f, 0.96f, 0.915f, 1.0f);
        public static Color4 GoldColor = new Color4(1.0f, 0.766f, 0.336f, 1.0f);
        public static Color4 CopperColor = new Color4(0.955f, 0.637f, 0.538f, 1.0f);
        public static Color4 AluminumColor = new Color4(0.913f, 0.921f, 0.925f, 1.0f);
        public static Color4 ChromiumColor = new Color4(0.550f, 0.556f, 0.554f, 1.0f);
        public static Color4 NickelColor = new Color4(0.660f, 0.609f, 0.526f, 1.0f);
        public static Color4 TitaniumColor = new Color4(0.542f, 0.497f, 0.449f, 1.0f);
        public static Color4 CobaltColor = new Color4(0.662f, 0.655f, 0.634f, 1.0f);
        public static Color4 PlatinumColor = new Color4(0.672f, 0.637f, 0.585f, 1.0f);

        public static Color4 WaterF0Color = new Color4(0.15f, 0.15f, 0.15f, 1.0f);
        public static Color4 PlasticF0Color = new Color4(0.21f, 0.21f, 0.21f, 1.0f);
        public static Color4 PlasticHighF0Color = new Color4(0.24f, 0.24f, 0.24f, 1.0f);
        public static Color4 RubyF0Color = new Color4(0.31f, 0.31f, 0.31f, 1.0f);
        public static Color4 DiamondF0Color = new Color4(0.45f, 0.45f, 0.45f, 1.0f);

        public static Color4 IronF0Color = new Color4(0.77f, 0.78f, 0.78f, 1.0f);
        public static Color4 SilverF0Color = new Color4(0.98f, 0.97f, 0.95f, 1.0f);
        public static Color4 GoldF0Color = new Color4(1.0f, 0.86f, 0.57f, 1.0f);
        public static Color4 CopperF0Color = new Color4(0.98f, 0.82f, 0.76f, 1.0f);
        public static Color4 AluminumF0Color = new Color4(0.96f, 0.96f, 0.97f, 1.0f);

        [DataContract]
        [KnownType(typeof(float[]))]
        class JsonParam
        {
            public enum ValueType
            {
                Float,
                Vector2,
                Vector3,
                Vector4,
                Color4,
                Matrix4,
                Texture2D,
            }

            [DataMember(EmitDefaultValue = false, Order = 0)]
            public string name;

            [DataMember(EmitDefaultValue = false, Order = 1)]
            public string semantic;

            [DataMember(EmitDefaultValue = false, Order = 2)]
            public ValueType type;

            [DataMember(EmitDefaultValue = false, Order = 3)]
            public float[] value;

            public JsonParam(string name, string semantic, ValueType type, float[] value)
            {
                this.name = name;
                this.semantic = semantic;
                this.type = type;
                this.value = value;
            }
        }

        [DataMember(EmitDefaultValue = false, Order = 1)]
        private string sh
        {
            get { if (shader == null) return null; return shader.Name; }
            set { shader = MMW.GetAsset<Shader>(value); }
        }

        [DataMember(EmitDefaultValue = false, Order = 2)]
        private JsonParam[] parameters
        {
            get
            {
                var ps = new List<JsonParam>();
                foreach (var p in floatParams.Values) ps.Add(new JsonParam(p.name, p.semantic, JsonParam.ValueType.Float, new float[] { p.value }));
                foreach (var p in vec2Params.Values) ps.Add(new JsonParam(p.name, p.semantic, JsonParam.ValueType.Vector2, p.value.ToFloats()));
                foreach (var p in vec3Params.Values) ps.Add(new JsonParam(p.name, p.semantic, JsonParam.ValueType.Vector3, p.value.ToFloats()));
                foreach (var p in vec4Params.Values) ps.Add(new JsonParam(p.name, p.semantic, JsonParam.ValueType.Vector4, p.value.ToFloats()));
                foreach (var p in colorParams.Values) ps.Add(new JsonParam(p.name, p.semantic, JsonParam.ValueType.Color4, p.value.ToFloats()));
                foreach (var p in mat4Params.Values) ps.Add(new JsonParam(p.name, p.semantic, JsonParam.ValueType.Matrix4, p.value.ToFloats()));
                foreach (var p in tex2DParams.Values) ps.Add(new JsonParam(p.name, p.semantic, JsonParam.ValueType.Texture2D, p.value != null ? new float[] { p.value.Index } : new float[] { -1 }));
                return ps.ToArray();
            }
            set
            {
                if (value == null || value.Length == 0) return;

                floatParams = new Dictionary<string, MaterialParam<float>>();
                vec2Params = new Dictionary<string, MaterialParam<Vector2>>();
                vec3Params = new Dictionary<string, MaterialParam<Vector3>>();
                vec4Params = new Dictionary<string, MaterialParam<Vector4>>();
                colorParams = new Dictionary<string, MaterialParam<Color4>>();
                mat4Params = new Dictionary<string, MaterialParam<Matrix4>>();
                tex2DParams = new Dictionary<string, MaterialParam<Texture2D>>();

                foreach (var v in value)
                {
                    if (v.type == JsonParam.ValueType.Float) AddParam<float>(v.name, v.semantic, v.value[0]);
                    if (v.type == JsonParam.ValueType.Vector2) AddParam<Vector2>(v.name, v.semantic, v.value.ToVector2());
                    if (v.type == JsonParam.ValueType.Vector3) AddParam<Vector3>(v.name, v.semantic, v.value.ToVector3());
                    if (v.type == JsonParam.ValueType.Vector4) AddParam<Vector4>(v.name, v.semantic, v.value.ToVector4());
                    if (v.type == JsonParam.ValueType.Color4) AddParam<Color4>(v.name, v.semantic, v.value.ToColor4());
                    if (v.type == JsonParam.ValueType.Matrix4) AddParam<Matrix4>(v.name, v.semantic, v.value.ToMatrix4());
                    if (v.type == JsonParam.ValueType.Texture2D) AddParam<Texture2D>(v.name, v.semantic, null, (int)v.value[0]);
                }
            }
        }
    }

    

    [DataContract]
    class MaterialParam<T>
    {
        [DataMember(Name = "name", EmitDefaultValue = false, Order = 0)]
        public string name;

        [DataMember(Name = "semantic", EmitDefaultValue = false, Order = 1)]
        public string semantic;

        [DataMember(Name = "value", EmitDefaultValue = false, Order = 2)]
        public T value;
        public int location = -1;
        public TextureUnit unit = TextureUnit.Texture31;

        public object tag;

        public MaterialParam<T> Clone()
        {
            return new MaterialParam<T>()
            {
                name = name,
                semantic = semantic,
                value = value,
                location = location,
                unit = unit,
                tag = tag,
            };
        }
    }
}
