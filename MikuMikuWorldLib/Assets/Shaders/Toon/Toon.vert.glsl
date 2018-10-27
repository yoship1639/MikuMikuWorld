#version 420

struct Surface
{
    vec3 wPos;
	vec3 wNormal;
	vec2 uv;
	vec4 color;
};

uniform mat4 M;
uniform mat4 MVP;
uniform mat4 MIT;

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;
layout (location = 3) in vec4 color;
layout (location = 4) in vec2 uv;

out Surface In;
 
void main()
{
	gl_Position = MVP * vec4(position, 1.0);

	In.wPos = vec3(M * vec4(position, 1.0));
	In.wNormal = normalize(mat3(MIT) * normal);
	In.uv = uv;
	In.color = color;
}