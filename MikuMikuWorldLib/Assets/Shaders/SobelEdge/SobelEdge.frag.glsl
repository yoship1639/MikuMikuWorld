#version 420

require(functions)

const float COEF_H[9] = { 1.0, 0.0, -1.0, 2.0, 0.0, -2.0, 1.0, 0.0, -1.0 };
const float COEF_V[9] = { 1.0, 2.0, 1.0, 0.0, 0.0, 0.0, -1.0, -2.0, -1.0 };
const vec2 OFFSET[9] = { vec2(-1.0, -1.0), vec2(0.0, -1.0), vec2(1.0, -1.0), vec2(-1.0, 0.0), vec2(0.0, 0.0), vec2(1.0, 0.0), vec2(-1.0, 1.0), vec2(0.0, 1.0), vec2(1.0, 1.0) };

uniform vec2 resolutionInverse = vec2(1.0 / 1280.0, 1.0 / 720.0);
uniform float edgeWidth = 2.0;
uniform float threshold = 0.001;
uniform float edgeStrength = 0.6;

layout (binding = 0) uniform sampler2D samplerSrc;
layout (binding = 1) uniform sampler2D samplerColor;

layout (location = 0) out vec4 FragColor;

vec4 sobel_edge(vec2 tex, sampler2D sampSrc, sampler2D sampColor)
{
	vec2 div = resolutionInverse * edgeWidth * 0.5;
	vec3 hColor = vec3(0.0);
	vec3 vColor = vec3(0.0);

	hColor += texture(sampColor, tex + OFFSET[0] * div).rgb * COEF_H[0];
	hColor += texture(sampColor, tex + OFFSET[1] * div).rgb * COEF_H[1];
	hColor += texture(sampColor, tex + OFFSET[2] * div).rgb * COEF_H[2];
	hColor += texture(sampColor, tex + OFFSET[3] * div).rgb * COEF_H[3];
	hColor += texture(sampColor, tex + OFFSET[4] * div).rgb * COEF_H[4];
	hColor += texture(sampColor, tex + OFFSET[5] * div).rgb * COEF_H[5];
	hColor += texture(sampColor, tex + OFFSET[6] * div).rgb * COEF_H[6];
	hColor += texture(sampColor, tex + OFFSET[7] * div).rgb * COEF_H[7];
	hColor += texture(sampColor, tex + OFFSET[8] * div).rgb * COEF_H[8];

	vColor += texture(sampColor, tex + OFFSET[0] * div).rgb * COEF_V[0];
	vColor += texture(sampColor, tex + OFFSET[1] * div).rgb * COEF_V[1];
	vColor += texture(sampColor, tex + OFFSET[2] * div).rgb * COEF_V[2];
	vColor += texture(sampColor, tex + OFFSET[3] * div).rgb * COEF_V[3];
	vColor += texture(sampColor, tex + OFFSET[4] * div).rgb * COEF_V[4];
	vColor += texture(sampColor, tex + OFFSET[5] * div).rgb * COEF_V[5];
	vColor += texture(sampColor, tex + OFFSET[6] * div).rgb * COEF_V[6];
	vColor += texture(sampColor, tex + OFFSET[7] * div).rgb * COEF_V[7];
	vColor += texture(sampColor, tex + OFFSET[8] * div).rgb * COEF_V[8];

	vec3 color = sqrt(hColor * hColor + vColor * vColor);
	float bright = dot(color, vec3(0.299, 0.587, 0.114));
	vec4 srcColor = texture(sampSrc, tex);
	color = mix(srcColor.rgb, vec3(1.0), step(bright, threshold));
	return srcColor * mix(vec4(1.0), vec4(color, 1.0), edgeStrength);
}

void main()
{
	vec2 uv = gl_FragCoord.xy * resolutionInverse;
    FragColor = sobel_edge(uv, samplerSrc, samplerColor);
}