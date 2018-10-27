#version 450

uniform mat4 MVP;
uniform mat4 OldMVP;
uniform mat4 MIT;
uniform float deltaTime;

layout (std430, binding = 1) readonly buffer Trans {
  mat4 data[];
} gTrans;

layout (std430, binding = 2) readonly buffer OldTrans {
  mat4 data[];
} gOldTrans;

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;
layout (location = 9) in vec4 boneIndex;
layout (location = 10) in vec4 boneWeight;

layout (location = 0) out vec4 outDir;

void main()
{
	vec4 pos = vec4(position, 1.0);

	// now trans
	vec4 p = vec4(0);
	//vec3 n = vec3(0);
	for (int i = 0; i < 4; i++)
	{
		mat4 m = gTrans.data[int(boneIndex[i])];
		p += (m * pos) * boneWeight[i];
		//n += (mat3(m) * normal) * boneWeight[i];
	}

	// old trans
	vec4 oldp = vec4(0);
	for (int i = 0; i < 4; i++)
	{
		mat4 m = gOldTrans.data[int(boneIndex[i])];
		oldp += (m * pos) * boneWeight[i];
	}

	vec4 nowPos = MVP * p;
	vec4 oldPos = OldMVP * oldp;

	vec3 dir = nowPos.xyz - oldPos.xyz;

	vec4 outPos = nowPos;

	outDir.xy = (nowPos.xy - oldPos.xy);
	outDir.y *= -1.0;
	outDir.zw = outPos.zw;

    gl_Position = outPos;
}