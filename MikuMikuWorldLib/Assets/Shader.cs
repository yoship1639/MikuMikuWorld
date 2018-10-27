//
// Miku Miku World License
//
// Copyright (c) 2017 Miku Miku World.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//

using MikuMikuWorld.GameComponents;
using MikuMikuWorld.GameComponents.Lights;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorld.Assets
{
    public abstract class Shader : IAsset
    {
        public bool Loaded { get; protected set; } = false;

        /// <summary>
        /// アセット名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// シェーダコードの種類
        /// </summary>
        public string ShaderCodeType { get; private set; }

        /// <summary>
        /// シェーダのコンパイルに失敗したときに出力されるメッセージ
        /// </summary>
        public string CompileErrorMessage { get; protected set; }

        protected internal Dictionary<string, ShaderParam> shaderParams = new Dictionary<string, ShaderParam>();
        protected internal Dictionary<string, ShaderParam> shaderParamsSemantic = new Dictionary<string, ShaderParam>();

        public Shader(string name, string codeType)
        {
            Name = name;
            ShaderCodeType = codeType;
        }

        protected abstract Result RegistShaderParam<T>(string name);
        protected abstract Result RegistShaderParam(string name, TextureUnit unit);
        protected abstract Result RegistShaderParam(string name, object value, float min, float max, float freqency);
        protected abstract Result RegistShaderParam<T>(string name, string semantic);
        protected abstract Result RegistShaderParam(string name, string semantic, TextureUnit unit);
        protected abstract Result RegistShaderParam(string name, string semantic, object value, float min, float max, float freqency);

        public virtual void SetUniqueParameter(ShaderUniqueParameter param, bool global) { }

        public abstract void SetParameter(int location, int value);
        public abstract void SetParameter(int location, float value);
        public abstract void SetParameter(int location, double value);
        public abstract void SetParameter(int location, Vector2 value);
        public abstract void SetParameter(int location, ref Vector2 value);
        public abstract void SetParameter(int location, Vector3 value);
        public abstract void SetParameter(int location, ref Vector3 value);
        public abstract void SetParameter(int location, Vector4 value);
        public abstract void SetParameter(int location, ref Vector4 value);
        public abstract void SetParameter(int location, Color4 value);
        public abstract void SetParameter(int location, ref Color4 value);
        public abstract void SetParameter(int location, int[] value);
        public abstract void SetParameter(int location, float[] value);
        public abstract void SetParameter(int location, Matrix4 value, bool transpose);
        public abstract void SetParameter(int location, ref Matrix4 value, bool transpose);
        public abstract void SetParameter(TextureUnit unit, Texture2D tex);
        public abstract void SetParameter(TextureUnit unit, TextureCube cube);

        public abstract void SetParameterByName(string name, int value);
        public abstract void SetParameterByName(string name, float value);
        public abstract void SetParameterByName(string name, double value);
        public abstract void SetParameterByName(string name, Vector2 value);
        public abstract void SetParameterByName(string name, ref Vector2 value);
        public abstract void SetParameterByName(string name, Vector3 value);
        public abstract void SetParameterByName(string name, ref Vector3 value);
        public abstract void SetParameterByName(string name, Vector4 value);
        public abstract void SetParameterByName(string name, ref Vector4 value);
        public abstract void SetParameterByName(string name, Color4 value);
        public abstract void SetParameterByName(string name, ref Color4 value);
        public abstract void SetParameterByName(string name, int[] value);
        public abstract void SetParameterByName(string name, float[] value);
        public abstract void SetParameterByName(string name, Matrix4 value, bool transpose);
        public abstract void SetParameterByName(string name, ref Matrix4 value, bool transpose);
        public abstract void SetParameterByName(string name, Texture2D tex);
        public abstract void SetParameterByName(string name, TextureCube cube);

        public abstract void SetParameterBySemantic(string semantic, int value);
        public abstract void SetParameterBySemantic(string semantic, float value);
        public abstract void SetParameterBySemantic(string semantic, double value);
        public abstract void SetParameterBySemantic(string semantic, Vector2 value);
        public abstract void SetParameterBySemantic(string semantic, ref Vector2 value);
        public abstract void SetParameterBySemantic(string semantic, Vector3 value);
        public abstract void SetParameterBySemantic(string semantic, ref Vector3 value);
        public abstract void SetParameterBySemantic(string semantic, Vector4 value);
        public abstract void SetParameterBySemantic(string semantic, ref Vector4 value);
        public abstract void SetParameterBySemantic(string semantic, Color4 value);
        public abstract void SetParameterBySemantic(string semantic, ref Color4 value);
        public abstract void SetParameterBySemantic(string semantic, int[] value);
        public abstract void SetParameterBySemantic(string semantic, float[] value);
        public abstract void SetParameterBySemantic(string semantic, Matrix4 value, bool transpose);
        public abstract void SetParameterBySemantic(string semantic, ref Matrix4 value, bool transpose);
        public abstract void SetParameterBySemantic(string semantic, Texture2D tex);
        public abstract void SetParameterBySemantic(string semantic, TextureCube cube);

        public abstract void UseShader(string technique = null, string pass = null);
        public abstract void UnuseShader();

        internal virtual void InitMaterialParameter(Material mat) { }

        public abstract Result Load();
        public abstract Result Unload();
    }

    public class ShaderParam
    {
        public string name;
        public string semantic;
        public TextureUnit unit = TextureUnit.Texture31;
        public int location = -1;
        public float min;
        public float max;
        public float frequency;
        public object value;
        public Type type;
    }

    public class ShaderUniqueParameter
    {
        public Camera camera;

        public double deltaTime;

        public Vector2 resolution;

        public Vector3 cameraDir;
        public Vector3 cameraPos;

        public DirectionalLight dirLight;
        public List<PointLight> pointLights = new List<PointLight>();
        public List<SpotLight> spotLights = new List<SpotLight>();

        public Matrix4 world;
        public Matrix4 worldInv;
        public Matrix4 view;
        public Matrix4 viewInverse;
        public Matrix4 proj;
        public Matrix4 projInverse;
        public Matrix4 viewProj;
        public Matrix4 viewProjInverse;
        public Matrix4 oldWorld;
        public Matrix4 oldViewProj;
        public Matrix4 ortho;

        public Matrix4 shadowDepthBias1;
        public Matrix4 shadowDepthBias2;
        public Matrix4 shadowDepthBias3;
        public Texture2D shadowDepthMap1;
        public Texture2D shadowDepthMap2;
        public Texture2D shadowDepthMap3;
        public Color4 uniqueColor;

        public TextureCube environmentMap;

        public Texture2D deferredAlbedoMap;
        public Texture2D deferredWorldPosMap;
        public Texture2D deferredWorldNormalMap;
        public Texture2D deferredPhysicalParamsMap;
        public Texture2D deferredF0Map;
        public Texture2D deferredDepthMap;
        public Texture2D deferredShadowMap;
        public Texture2D deferredVelocityMap;

        public bool skinning;
        public bool morphing;
    }
}
