#version 420

layout (binding = 0) uniform sampler2D albedoMap;

layout (location = 0) in vec4 color;
layout (location = 1) in vec2 uv;
layout( location = 0 ) out vec4 FragColor;
 
void main()
{
    FragColor = color * texture2D(albedoMap, uv);
}