#version 420

struct Surface
{
    vec3 wPos;
	vec3 wNormal;
	vec3 normal;
	vec3 tangent;
	vec3 binormal;
	vec3 tanLightDir;
	vec2 uv;
	vec4 color;
};

struct DirectionalLight
{
	vec3 dir;
	vec4 color;
	float intensity;
};

uniform mat4 M;
uniform mat4 MVP;
uniform mat4 MIT;

uniform DirectionalLight wDirLight;

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec4 tangent;
layout (location = 3) in vec4 color;
layout (location = 4) in vec2 uv;

out Surface In;
 
void main()
{
	gl_Position = MVP * vec4(position, 1.0);

	In.wPos = vec3(M * vec4(position, 1.0));
	In.wNormal = normalize(mat3(MIT) * normal);
	//In.normal = normalize(normal);
	//In.tangent = normalize(tangent);
	//In.binormal = normalize(cross(normal, tangent));
	//mat4 invTanMat = mat4(vec4(In.normal, 0.0), vec4(In.tangent), 0.0, vec4(In.binormal, 0.0), vec4(0.0, 0.0, 0.0, 1.0));
	//vec3 lightDir = normalize(vec3(MIT * vec4(wDirLight.dir, 0.0)));
	//In.tanLightDir = -lightDir * invTanMat;
	In.uv = uv;
	In.color = color;
}