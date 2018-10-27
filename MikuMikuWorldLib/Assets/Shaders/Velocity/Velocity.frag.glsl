#version 420

layout (location = 0) in vec4 dir;

layout (location = 0) out vec4 FragColor;

void main()
{
	vec4 color;
	color.xy = dir.xy;
	color.z = 1.0;
	color.w = dir.z / dir.w;
	
	FragColor = color;
}