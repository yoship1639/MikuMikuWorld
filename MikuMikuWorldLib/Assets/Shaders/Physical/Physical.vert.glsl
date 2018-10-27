#version 450

struct Surface
{
    vec3 wPos;
	vec3 wNormal;
	vec3 normal;
	vec3 tangent;
	vec3 binormal;
	vec2 uv;
	vec4 color;
	vec4 shadowCoord1;
	vec4 shadowCoord2;
	vec4 shadowCoord3;
};

uniform int skinning = 0;
uniform int morphing = 0;

uniform mat4 M;
uniform mat4 MVP;
uniform mat4 MIT;
uniform mat4 shadowMV1;
uniform mat4 shadowMV2;
uniform mat4 shadowMV3;

layout (std430, binding = 0) readonly buffer Morph {
  vec4 data[];
} gMorph;

layout (std430, binding = 1) readonly buffer Trans {
  mat4 data[];
} gTrans;

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec3 tangent;
layout (location = 3) in vec4 color;
layout (location = 4) in vec2 uv;
layout (location = 9) in vec4 boneIndex;
layout (location = 10) in vec4 boneWeight;
layout (location = 11) in highp float index;

out Surface In;
 
void main()
{
	vec4 pos = vec4(0);
	// morphing
	if (morphing == 1)
	{
		pos = vec4(position + gMorph.data[int(index)].xyz, 1.0);
	}
	else
	{
		pos = vec4(position, 1.0);
	}

	// skinmesh
	vec4 p = vec4(0);
	vec3 n = vec3(0);
	vec3 t = vec3(0);
	if (skinning == 1)
	{
		for (int i = 0; i < 4; i++)
		{
			mat4 m = gTrans.data[int(boneIndex[i])];
			p += (m * pos) * boneWeight[i];
			n += (mat3(m) * normal) * boneWeight[i];
			t += (mat3(m) * tangent) * boneWeight[i];
		}
		p /= p.w;
	}
	else
	{
		p = pos;
		n = normal;
		t = tangent;
	}
	
	gl_Position = MVP * p;

	In.wPos = vec3(M * p);
	In.wNormal = vec3(mat3(MIT) * n);

	In.normal = normalize(n);
	In.tangent = normalize(t);
	In.binormal = normalize(cross(n, t));

	In.uv = uv;
	In.color = color;
	In.shadowCoord1 = shadowMV1 * p;
	In.shadowCoord2 = shadowMV2 * p;
	In.shadowCoord3 = shadowMV3 * p;
}