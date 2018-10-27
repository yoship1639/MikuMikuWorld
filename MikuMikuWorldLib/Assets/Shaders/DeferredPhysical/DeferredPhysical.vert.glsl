#version 450

#define VELOCITYMAP
#define SHADOWMAP

struct Surface
{
    vec3 wPos;
	vec4 pos;
	vec4 dir;
	vec3 normal;
	vec3 tangent;
	vec3 binormal;
	vec2 uv;
	vec4 color;
	vec4 shadowCoord1;
	vec4 shadowCoord2;
	vec4 shadowCoord3;
};

uniform mat4 M;
uniform mat4 MVP;
uniform mat4 MIT;
uniform mat4 OldMVP;
uniform mat4 shadowMV1;
uniform mat4 shadowMV2;
uniform mat4 shadowMV3;
uniform float deltaTime;

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec3 tangent;
layout (location = 3) in vec4 color;
layout (location = 4) in vec2 uv;

out Surface In;
 
void main()
{
	vec4 pos = vec4(position, 1.0);
	vec4 nowPos =  MVP * pos;

	gl_Position = nowPos;
	In.wPos = vec3(M * pos);
	In.pos = nowPos;

	In.normal = normalize(normal);
	In.tangent = normalize(tangent);
	In.binormal = normalize(cross(normal, tangent));

	In.uv = uv;
	In.color = color;

#ifdef SHADOWMAP
	In.shadowCoord1 = shadowMV1 * pos;
	In.shadowCoord2 = shadowMV2 * pos;
	In.shadowCoord3 = shadowMV3 * pos;
#endif

#ifdef VELOCITYMAP
	vec4 oldPos = OldMVP * pos;
	In.dir.xy = (nowPos.xy - oldPos.xy);
	In.dir.y *= -1.0;
	In.dir.zw = nowPos.zw;
#endif
}