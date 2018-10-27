#version 420

const float COEF[9] = { 1.0, 1.0, 1.0, 1.0, -8.0, 1.0, 1.0, 1.0, 1.0};
const vec2 OFFSET[9] = { vec2(-1.0, -1.0), vec2(0.0, -1.0), vec2(1.0, -1.0), vec2(-1.0, 0.0), vec2(0.0, 0.0), vec2(1.0, 0.0), vec2(-1.0, 1.0), vec2(0.0, 1.0), vec2(1.0, 1.0) };

uniform vec2 resolutionInverse;
uniform float edgeWidth = 2.0;
uniform float threshold = 0.6;

layout (binding = 0) uniform sampler2D samplerSrc;

layout (location = 0) out vec4 FragColor;

vec4 laplacian(vec2 tex, sampler2D src)
{
	vec2 div = resolutionInverse * edgeWidth;
	vec3 dstColor = vec3(0.0);

	dstColor += texture(src, tex + OFFSET[0] * div).rgb * COEF[0];
	dstColor += texture(src, tex + OFFSET[1] * div).rgb * COEF[1];
	dstColor += texture(src, tex + OFFSET[2] * div).rgb * COEF[2];
	dstColor += texture(src, tex + OFFSET[3] * div).rgb * COEF[3];
	dstColor += texture(src, tex + OFFSET[4] * div).rgb * COEF[4];
	dstColor += texture(src, tex + OFFSET[5] * div).rgb * COEF[5];
	dstColor += texture(src, tex + OFFSET[6] * div).rgb * COEF[6];
	dstColor += texture(src, tex + OFFSET[7] * div).rgb * COEF[7];
	dstColor += texture(src, tex + OFFSET[8] * div).rgb * COEF[8];

	float bright = dot(dstColor, vec3(0.299, 0.587, 0.114));
	dstColor = vec3(1.0 - step(bright, threshold));
	return vec4(dstColor, 1.0);
}


void main()
{
	vec2 uv = gl_FragCoord.xy * resolutionInverse;
    FragColor = laplacian(uv, samplerSrc);
}