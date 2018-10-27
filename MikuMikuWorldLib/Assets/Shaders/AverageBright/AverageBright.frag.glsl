#version 420

const vec3 RGB2Y = vec3(0.299, 0.587, 0.114);
const float EPSILON = 0.00001;

uniform vec2 resolutionInverse = vec2(1.0 / 1280.0, 1.0 / 720.0);
layout (binding = 0) uniform sampler2D samplerSrc;

layout (location = 0) out vec4 FragColor;

void main()
{
	vec2 uv0 = gl_FragCoord.xy * resolutionInverse;
	vec2 uv1 = uv0 + vec2(resolutionInverse.x, 0.0);
	vec2 uv2 = uv0 + vec2(0.0, resolutionInverse.y);
	vec2 uv3 = uv0 + resolutionInverse;

	float l0 = dot(RGB2Y, texture(samplerSrc, uv0).rgb);
	float l1 = dot(RGB2Y, texture(samplerSrc, uv1).rgb);
	float l2 = dot(RGB2Y, texture(samplerSrc, uv2).rgb);
	float l3 = dot(RGB2Y, texture(samplerSrc, uv3).rgb);

	float l_max = max(max(l0, l1), max(l2, l3));
	float total = log(l0 + EPSILON) + log(l1 + EPSILON) + log(l2 + EPSILON) + log(l3 + EPSILON);

    FragColor = vec4(l_max, vec2(total) * 0.25, 1.0);
}