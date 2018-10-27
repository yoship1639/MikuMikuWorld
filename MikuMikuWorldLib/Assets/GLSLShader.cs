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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Graphics;
using MikuMikuWorld.Properties;
using System.Diagnostics;

namespace MikuMikuWorld.Assets
{
    public class GLSLShader : Shader
    {
        internal int program = -1;

        public string VertexCode { get; set; }
        public string FragmentCode { get; set; }
        public string GeometryCode { get; set; }
        public string TesseControlCode { get; set; }
        public string TessEvaluationCode { get; set; }
        public string ComputeCode { get; set; }

        public GLSLShader(string name) : 
            base(name, "GLSL")
        { }

        override protected Result RegistShaderParam<T>(string name)
        {
            if (shaderParams.ContainsKey(name)) return Result.AlreadyRegistered;
            shaderParams.Add(name, new ShaderParam()
            {
                name = name,
                type = typeof(T),
            });
            return Result.Success;
        }
        override protected Result RegistShaderParam(string name, TextureUnit unit)
        {
            if (shaderParams.ContainsKey(name)) return Result.AlreadyRegistered;
            shaderParams.Add(name, new ShaderParam() { name = name, unit = unit, type = typeof(Texture2D) });
            return Result.Success;
        }
        override protected Result RegistShaderParam(string name, object value, float min, float max, float freqency)
        {
            if (shaderParams.ContainsKey(name)) return Result.AlreadyRegistered;
            shaderParams.Add(name, new ShaderParam()
            {
                name = name,
                value = value,
                min = min,
                max = max,
                frequency = freqency,
                type = value.GetType(),
            });
            return Result.Success;
        }
        override protected Result RegistShaderParam<T>(string name, string semantic)
        {
            if (shaderParams.ContainsKey(name)) return Result.AlreadyRegistered;
            var sp = new ShaderParam() { name = name, semantic = semantic, type = typeof(T) };
            shaderParams.Add(name, sp);
            shaderParamsSemantic.Add(semantic, sp);
            return Result.Success;
        }
        override protected Result RegistShaderParam(string name, string semantic, TextureUnit unit)
        {
            if (shaderParams.ContainsKey(name)) return Result.AlreadyRegistered;
            var sp = new ShaderParam() { name = name, semantic = semantic, unit = unit, type = typeof(Texture2D) };
            shaderParams.Add(name, sp);
            shaderParamsSemantic.Add(semantic, sp);
            return Result.Success;
        }
        override protected Result RegistShaderParam(string name, string semantic, object value, float min, float max, float freqency)
        {
            if (shaderParams.ContainsKey(name)) return Result.AlreadyRegistered;
            var sp = new ShaderParam()
            {
                name = name,
                semantic = semantic,
                value = value,
                min = min,
                max = max,
                frequency = freqency,
                type = value.GetType(),
            };
            shaderParams.Add(name, sp);
            shaderParamsSemantic.Add(semantic, sp);
            return Result.Success;
        }

        override public void SetParameter(int location, int value)
        {
            GL.ProgramUniform1(program, location, value);
        }
        override public void SetParameter(int location, float value)
        {
            GL.ProgramUniform1(program, location, value);
        }
        override public void SetParameter(int location, double value)
        {
            GL.ProgramUniform1(program, location, value);
        }
        override public void SetParameter(int location, Vector2 value)
        {
            GL.ProgramUniform2(program, location, ref value);
        }
        override public void SetParameter(int location, ref Vector2 value)
        {
            GL.ProgramUniform2(program, location, ref value);
        }
        override public void SetParameter(int location, Vector3 value)
        {
            GL.ProgramUniform3(program, location, ref value);
        }
        override public void SetParameter(int location, ref Vector3 value)
        {
            GL.ProgramUniform3(program, location, ref value);
        }
        override public void SetParameter(int location, Vector4 value)
        {
            GL.ProgramUniform4(program, location, ref value);
        }
        override public void SetParameter(int location, ref Vector4 value)
        {
            GL.ProgramUniform4(program, location, ref value);
        }
        override public void SetParameter(int location, Color4 value)
        {
            GL.ProgramUniform4(program, location, value);
        }
        override public void SetParameter(int location, ref Color4 value)
        {
            GL.ProgramUniform4(program, location, value);
        }
        override public void SetParameter(int location, int[] value)
        {
            GL.ProgramUniform4(program, location, value.Length, value);
        }
        override public void SetParameter(int location, float[] value)
        {
            GL.ProgramUniform4(program, location, value.Length, value);
        }
        override public void SetParameter(int location, Matrix4 value, bool transpose)
        {
            GL.ProgramUniformMatrix4(program, location, transpose, ref value);
        }
        override public void SetParameter(int location, ref Matrix4 value, bool transpose)
        {
            GL.ProgramUniformMatrix4(program, location, transpose, ref value);
        }
        override public void SetParameter(TextureUnit unit, Texture2D tex)
        {
            if (tex == null) return;
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, tex.texture);
        }
        override public void SetParameter(TextureUnit unit, TextureCube cube)
        {
            if (cube == null) return;
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.TextureCubeMap, cube.texture);
        }

        override public void SetParameterByName(string name, int value)
        {
            if (program == -1) return;
            var param = GetParamByName(name);
            if (param == null || param.location == -1) return;
            GL.ProgramUniform1(program, param.location, value);
        }
        override public void SetParameterByName(string name, float value)
        {
            if (program == -1) return;
            var param = GetParamByName(name);
            if (param == null || param.location == -1) return;
            GL.ProgramUniform1(program, param.location, value);
        }
        override public void SetParameterByName(string name, double value)
        {
            if (program == -1) return;
            var param = GetParamByName(name);
            if (param == null || param.location == -1) return;
            GL.ProgramUniform1(program, param.location, value);
        }
        override public void SetParameterByName(string name, Vector2 value)
        {
            if (program == -1) return;
            var param = GetParamByName(name);
            if (param == null || param.location == -1) return;
            GL.ProgramUniform2(program, param.location, ref value);
        }
        override public void SetParameterByName(string name, ref Vector2 value)
        {
            if (program == -1) return;
            var param = GetParamByName(name);
            if (param == null || param.location == -1) return;
            GL.ProgramUniform2(program, param.location, ref value);
        }
        override public void SetParameterByName(string name, Vector3 value)
        {
            if (program == -1) return;
            var param = GetParamByName(name);
            if (param == null || param.location == -1) return;
            GL.ProgramUniform3(program, param.location, ref value);
        }
        override public void SetParameterByName(string name, ref Vector3 value)
        {
            if (program == -1) return;
            var param = GetParamByName(name);
            if (param == null || param.location == -1) return;
            GL.ProgramUniform3(program, param.location, ref value);
        }
        override public void SetParameterByName(string name, Vector4 value)
        {
            if (program == -1) return;
            var param = GetParamByName(name);
            if (param == null || param.location == -1) return;
            GL.ProgramUniform4(program, param.location, ref value);
        }
        override public void SetParameterByName(string name, ref Vector4 value)
        {
            if (program == -1) return;
            var param = GetParamByName(name);
            if (param == null || param.location == -1) return;
            GL.ProgramUniform4(program, param.location, ref value);
        }
        override public void SetParameterByName(string name, Color4 value)
        {
            if (program == -1) return;
            var param = GetParamByName(name);
            if (param == null || param.location == -1) return;
            GL.ProgramUniform4(program, param.location, value);
        }
        override public void SetParameterByName(string name, ref Color4 value)
        {
            if (program == -1) return;
            var param = GetParamByName(name);
            if (param == null || param.location == -1) return;
            GL.ProgramUniform4(program, param.location, value);
        }
        override public void SetParameterByName(string name, int[] value)
        {
            if (program == -1) return;
            var param = GetParamByName(name);
            if (param == null || param.location == -1) return;
            GL.ProgramUniform4(program, param.location, value.Length, value);
        }
        override public void SetParameterByName(string name, float[] value)
        {
            if (program == -1) return;
            var param = GetParamByName(name);
            if (param == null || param.location == -1) return;
            GL.ProgramUniform4(program, param.location, value.Length, value);
        }
        override public void SetParameterByName(string name, Matrix4 value, bool transpose)
        {
            if (program == -1) return;
            var param = GetParamByName(name);
            if (param == null || param.location == -1) return;
            GL.ProgramUniformMatrix4(program, param.location, transpose, ref value);
        }
        override public void SetParameterByName(string name, ref Matrix4 value, bool transpose)
        {
            if (program == -1) return;
            var param = GetParamByName(name);
            if (param == null || param.location == -1) return;
            GL.ProgramUniformMatrix4(program, param.location, transpose, ref value);
        }
        override public void SetParameterByName(string name, Texture2D tex)
        {
            if (program == -1 || tex == null || !tex.Loaded) return;
            var param = GetParamByName(name);
            if (param == null) return;
            GL.ActiveTexture(param.unit);
            GL.BindTexture(TextureTarget.Texture2D, tex.texture);
        }
        override public void SetParameterByName(string name, TextureCube cube)
        {
            if (program == -1 || cube == null || !cube.Loaded) return;
            var param = GetParamByName(name);
            if (param == null) return;
            GL.ActiveTexture(param.unit);
            GL.BindTexture(TextureTarget.TextureCubeMap, cube.texture);
        }

        override public void SetParameterBySemantic(string semantic, int value)
        {
            if (program == -1) return;
            var param = GetParamBySemantic(semantic);
            if (param == null || param.location == -1) return;
            GL.ProgramUniform1(program, param.location, value);
        }
        override public void SetParameterBySemantic(string semantic, float value)
        {
            if (program == -1) return;
            var param = GetParamBySemantic(semantic);
            if (param == null || param.location == -1) return;
            GL.ProgramUniform1(program, param.location, value);
        }
        override public void SetParameterBySemantic(string semantic, double value)
        {
            if (program == -1) return;
            var param = GetParamBySemantic(semantic);
            if (param == null || param.location == -1) return;
            GL.ProgramUniform1(program, param.location, value);
        }
        override public void SetParameterBySemantic(string semantic, Vector2 value)
        {
            if (program == -1) return;
            var param = GetParamBySemantic(semantic);
            if (param == null || param.location == -1) return;
            GL.ProgramUniform2(program, param.location, ref value);
        }
        override public void SetParameterBySemantic(string semantic, ref Vector2 value)
        {
            if (program == -1) return;
            var param = GetParamBySemantic(semantic);
            if (param == null || param.location == -1) return;
            GL.ProgramUniform2(program, param.location, ref value);
        }
        override public void SetParameterBySemantic(string semantic, Vector3 value)
        {
            if (program == -1) return;
            var param = GetParamBySemantic(semantic);
            if (param == null || param.location == -1) return;
            GL.ProgramUniform3(program, param.location, ref value);
        }
        override public void SetParameterBySemantic(string semantic, ref Vector3 value)
        {
            if (program == -1) return;
            var param = GetParamBySemantic(semantic);
            if (param == null || param.location == -1) return;
            GL.ProgramUniform3(program, param.location, ref value);
        }
        override public void SetParameterBySemantic(string semantic, Vector4 value)
        {
            if (program == -1) return;
            var param = GetParamBySemantic(semantic);
            if (param == null || param.location == -1) return;
            GL.ProgramUniform4(program, param.location, ref value);
        }
        override public void SetParameterBySemantic(string semantic, ref Vector4 value)
        {
            if (program == -1) return;
            var param = GetParamBySemantic(semantic);
            if (param == null || param.location == -1) return;
            GL.ProgramUniform4(program, param.location, ref value);
        }
        override public void SetParameterBySemantic(string semantic, Color4 value)
        {
            if (program == -1) return;
            var param = GetParamBySemantic(semantic);
            if (param == null || param.location == -1) return;
            GL.ProgramUniform4(program, param.location, value);
        }
        override public void SetParameterBySemantic(string semantic, ref Color4 value)
        {
            if (program == -1) return;
            var param = GetParamBySemantic(semantic);
            if (param == null || param.location == -1) return;
            GL.ProgramUniform4(program, param.location, value);
        }
        override public void SetParameterBySemantic(string semantic, int[] value)
        {
            if (program == -1) return;
            var param = GetParamBySemantic(semantic);
            if (param == null || param.location == -1) return;
            GL.ProgramUniform4(program, param.location, value.Length, value);
        }
        override public void SetParameterBySemantic(string semantic, float[] value)
        {
            if (program == -1) return;
            var param = GetParamBySemantic(semantic);
            if (param == null || param.location == -1) return;
            GL.ProgramUniform4(program, param.location, value.Length, value);
        }
        override public void SetParameterBySemantic(string semantic, Matrix4 value, bool transpose)
        {
            if (program == -1) return;
            var param = GetParamBySemantic(semantic);
            if (param == null || param.location == -1) return;
            GL.ProgramUniformMatrix4(program, param.location, transpose, ref value);
        }
        override public void SetParameterBySemantic(string semantic, ref Matrix4 value, bool transpose)
        {
            if (program == -1) return;
            var param = GetParamBySemantic(semantic);
            if (param == null || param.location == -1) return;
            GL.ProgramUniformMatrix4(program, param.location, transpose, ref value);
        }
        override public void SetParameterBySemantic(string semantic, Texture2D tex)
        {
            if (program == -1 || tex == null || !tex.Loaded) return;
            var param = GetParamBySemantic(semantic);
            if (param == null) return;
            GL.ActiveTexture(param.unit);
            GL.BindTexture(TextureTarget.Texture2D, tex.texture);
        }
        override public void SetParameterBySemantic(string semantic, TextureCube cube)
        {
            if (program == -1 || cube == null || !cube.Loaded) return;
            var param = GetParamBySemantic(semantic);
            if (param == null) return;
            GL.ActiveTexture(param.unit);
            GL.BindTexture(TextureTarget.TextureCubeMap, cube.texture);
        }

        public int GetUniformLocation(string name)
        {
            if (program == -1) return -1;
            return GL.GetUniformLocation(program, name);
        }
        public TextureUnit GetTextureUnit(string name)
        {
            if (!shaderParams.ContainsKey(name)) return TextureUnit.Texture31;
            return shaderParams[name].unit;
        }
        private ShaderParam GetParamByName(string name)
        {
            if (!shaderParams.ContainsKey(name)) return null;
            var param = shaderParams[name];
            if (param.location == -1) param.location = GetUniformLocation(name);
            return param;
        }
        private ShaderParam GetParamBySemantic(string semantic)
        {
            if (!shaderParamsSemantic.ContainsKey(semantic)) return null;
            var param = shaderParamsSemantic[semantic];
            if (param.location == -1) param.location = GetUniformLocation(param.name);
            return param;
        }

        override public void UseShader(string technique = null, string pass = null)
        {
            if (program != -1) GL.UseProgram(program);
        }
        override public void UnuseShader()
        {
            GL.UseProgram(0);
        }

        private class ShaderLoadData
        {
            public int shader;
            public ShaderType type;
            public string code;

            public ShaderLoadData(ShaderType type, string code)
            {
                this.shader = -1;
                this.type = type;
                this.code = code;
            }
        }

        private int LoadShader(ShaderType type, string code)
        {
            var shader = GL.CreateShader(type);
            GL.ShaderSource(shader, code);
            GL.CompileShader(shader);
            int status;
            GL.GetShader(shader, ShaderParameter.CompileStatus, out status);
            if (status == 0)
            {
                CompileErrorMessage = GL.GetShaderInfoLog(shader);
                GL.DeleteShader(shader);
                return -1;
            }
            return shader;
        }

        override public Result Load()
        {
            Unload();

            string add = "";
            if (MMW.Configuration.IBLQuality == MMWConfiguration.IBLQualityType.VeryHigh)
            {
                add += "#define IBL_DIFF_SAMPLE_NUM  128\n";
                add += "#define IBL_SPEC_SAMPLE_NUM  64\n";
            }
            else if (MMW.Configuration.IBLQuality == MMWConfiguration.IBLQualityType.High)
            {
                add += "#define IBL_DIFF_SAMPLE_NUM  48\n";
                add += "#define IBL_SPEC_SAMPLE_NUM  24\n";
            }
            else if (MMW.Configuration.IBLQuality == MMWConfiguration.IBLQualityType.Default)
            {
                add += "#define IBL_DIFF_SAMPLE_NUM  24\n";
                add += "#define IBL_SPEC_SAMPLE_NUM  12\n";
            }
            else if (MMW.Configuration.IBLQuality == MMWConfiguration.IBLQualityType.Low)
            {
                add += "#define IBL_DIFF_SAMPLE_NUM  12\n";
                add += "#define IBL_SPEC_SAMPLE_NUM  8\n";
            }

            if (VertexCode != null) VertexCode = VertexCode.Replace("require(functions)", add + Resources.Functions);
            if (FragmentCode != null) FragmentCode = FragmentCode.Replace("require(functions)", add + Resources.Functions);

            List<ShaderLoadData> shaders = new List<ShaderLoadData>();
            if (!string.IsNullOrEmpty(VertexCode)) shaders.Add(new ShaderLoadData(ShaderType.VertexShader, VertexCode));
            if (!string.IsNullOrEmpty(FragmentCode)) shaders.Add(new ShaderLoadData(ShaderType.FragmentShader, FragmentCode));
            if (!string.IsNullOrEmpty(GeometryCode)) shaders.Add(new ShaderLoadData(ShaderType.GeometryShader, GeometryCode));
            if (!string.IsNullOrEmpty(TesseControlCode)) shaders.Add(new ShaderLoadData(ShaderType.TessControlShader, TesseControlCode));
            if (!string.IsNullOrEmpty(TessEvaluationCode)) shaders.Add(new ShaderLoadData(ShaderType.TessEvaluationShader, TessEvaluationCode));
            if (!string.IsNullOrEmpty(ComputeCode)) shaders.Add(new ShaderLoadData(ShaderType.ComputeShader, ComputeCode));

            foreach (var data in shaders)
            {
                data.shader = LoadShader(data.type, data.code);
                if (data.shader == -1)
                {
                    Debug.WriteLine(Name + ": " + CompileErrorMessage);
                    //Console.WriteLine(CompileErrorMessage);
                    return Result.CompileError;
                }
            }

            program = GL.CreateProgram();

            foreach (var data in shaders)
            {
                GL.AttachShader(program, data.shader);
                GL.DeleteShader(data.shader);
            }

            int status;
            GL.LinkProgram(program);
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out status);
            if (status == 0)
            {
                CompileErrorMessage = GL.GetProgramInfoLog(program);
                Debug.WriteLine(CompileErrorMessage);
                GL.DeleteProgram(program);
                program = -1;
                return Result.CompileError;
            }

            foreach (var p in shaderParams) p.Value.location = GetUniformLocation(p.Value.name);

            Loaded = true;
            return Result.Success;
        }
        override public Result Unload()
        {
            if (!Loaded) return Result.NotLoaded;

            GL.DeleteProgram(program);
            program = -1;
            Loaded = false;
            return Result.Success;
        }
    }
}
