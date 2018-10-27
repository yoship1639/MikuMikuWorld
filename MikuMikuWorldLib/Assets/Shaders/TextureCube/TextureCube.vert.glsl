#version 420

uniform mat4 MVP;

layout (location = 0) in vec3 position;

out vec3 pos;
 
void main()
{
    gl_Position = MVP * vec4(position, 1);
	pos = position;
}