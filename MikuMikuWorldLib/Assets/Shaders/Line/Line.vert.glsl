#version 400

uniform mat4 MVP;
uniform vec4 color;

layout (location = 0) in vec3 position;
layout (location = 3) in vec4 inColor;

out vec4 Color;
 
void main()
{
    gl_Position = MVP * vec4(position, 1.0);
	Color = inColor * color;
}