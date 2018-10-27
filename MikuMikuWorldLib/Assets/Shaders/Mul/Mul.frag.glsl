#version 420

uniform vec2 resolutionInverse = vec2(1.0 / 1280.0, 1.0 / 720);
layout (binding = 0) uniform sampler2D sampler0;
layout (binding = 1) uniform sampler2D sampler1;

layout (location = 0) out vec4 FragColor;

void main()
{
	vec2 coord = gl_FragCoord.xy * resolutionInverse.xy;
    FragColor = texture(sampler0, coord) * texture(sampler1, coord);
}