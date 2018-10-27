#version 420

uniform vec2 resolutionInverse = vec2(1.0 / 1280.0, 1.0 / 720.0);
uniform float threshold = 1.0;
layout (binding = 0) uniform sampler2D samplerSrc;

layout (location = 0) out vec4 FragColor;

void main()
{
	vec2 uv = gl_FragCoord.xy * resolutionInverse;
	vec4 color = texture(samplerSrc, uv);

	const vec3 LumCoeff = vec3(0.2125, 0.7154, 0.0721);
	//float m = clamp(dot(color.rgb, LumCoeff) - threshold, 0.0, 1.0);
	float m = clamp(max(color.r, max(color.g, color.b)) - threshold, 0.0, 1.0);
	color.rgb = normalize(color.rgb) * m;
	color.rgb = max(color.rgb, vec3(0));
	//color.rgb = max(color.rgb - vec3(threshold), vec3(0));
    FragColor = color;
}