#version 420

uniform vec4 color = vec4(1.0);

layout (location = 0) out vec4 FragColor0;

void main()
{
	FragColor0 = color;
}