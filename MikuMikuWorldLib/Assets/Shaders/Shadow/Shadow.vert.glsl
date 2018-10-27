#version 420

struct Surface
{
    vec3 wPos;
	vec4 shadowCoord1;
	vec4 shadowCoord2;
	vec4 shadowCoord3;
};

uniform mat4 M;
uniform mat4 MVP;
uniform mat4 shadowMV1;
uniform mat4 shadowMV2;
uniform mat4 shadowMV3;

layout (location = 0) in vec3 position;

out Surface In;
 
void main()
{
	gl_Position = MVP * vec4(position, 1.0);
	In.wPos = (M * vec4(position, 1.0)).xyz;
	In.shadowCoord1 = shadowMV1 * vec4(position, 1.0);
	In.shadowCoord2 = shadowMV2 * vec4(position, 1.0);
	In.shadowCoord3 = shadowMV3 * vec4(position, 1.0);
}