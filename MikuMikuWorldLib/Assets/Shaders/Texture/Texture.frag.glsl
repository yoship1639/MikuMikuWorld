#version 420

uniform vec4 color = vec4(1.0);

layout( binding = 0) uniform sampler2D tex0;

in vec2 texcoord;
layout( location = 0 ) out vec4 FragColor;
 
void main()
{
    FragColor = texture(tex0, texcoord) * clamp(color, vec4(0), vec4(1));
}