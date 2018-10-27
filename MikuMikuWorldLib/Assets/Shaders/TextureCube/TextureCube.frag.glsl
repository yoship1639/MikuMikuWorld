#version 420

require(functions)

uniform vec4 color = vec4(1.0);
uniform float contrast = 1.0;
uniform float saturation = 1.0;
uniform float brightness = 1.0;

layout (binding = 0) uniform samplerCube tex0;

in vec3 pos;

layout (location = 0) out vec4 FragColor;
 
void main()
{
	vec4 col = texture(tex0, pos * vec3(-1.0, 1.0, 1.0)) * color;
	col.rgb = _csb(col.rgb, contrast, saturation, brightness);
	FragColor = col;
}