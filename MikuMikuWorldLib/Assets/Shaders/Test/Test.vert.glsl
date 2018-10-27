#version 420

struct BoneWeight
{
	float boneIndex0;
	float boneIndex1;
	float boneIndex2;
	float boneIndex3;
	float weight0;
	float weight1;
	float weight2;
	float weight3;
};
 
uniform mat4 MIT;
uniform mat4 MVP;
uniform vec4 diffuse = vec4(1.0);
uniform vec4 ambient = vec4(0.0);

uniform vec3 lightDir = vec3(-1, -1, 1);
 
layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec4 tangent;
layout (location = 3) in vec4 color;
layout (location = 4) in vec2 uv;
layout (location = 5) in vec4 uv1;
layout (location = 6) in vec4 uv2;
layout (location = 7) in vec4 uv3;
layout (location = 8) in vec4 uv4;
layout (location = 9) in BoneWeight boneWeight;
layout (location = 10) in float index;

layout (location = 0) out vec4 Color;
layout (location = 1) out vec2 UV;
 
void main()
{
	vec3 n = normalize(mat3(MIT) * normal);
    Color = color * (diffuse + ambient);
	Color.rgb *= clamp(dot(n, normalize(-lightDir)), 0.0, 1.0);
	Color.a = color.a * diffuse.a;
	UV = uv;
    gl_Position = MVP * vec4(position, 1.0);
}