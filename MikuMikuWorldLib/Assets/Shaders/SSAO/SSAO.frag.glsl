#version 420

#define SSAO_SAMPLES	32

uniform vec2 resolutionInverse = vec2(1.0 / 1280.0, 1.0 / 720);
uniform float radius = 8.0;
uniform float ignoreDist = 0.3;
uniform float attenPower = 3.0;

layout (binding = 0) uniform sampler2D samplerSrc;
layout (binding = 1) uniform sampler2D samplerDepth;

layout (location = 0) out vec4 FragColor;

float radicalInverse_VdC(uint bits)
{
    bits = (bits << 16u) | (bits >> 16u);
    bits = ((bits & 0x55555555u) << 1u) | ((bits & 0xAAAAAAAAu) >> 1u);
    bits = ((bits & 0x33333333u) << 2u) | ((bits & 0xCCCCCCCCu) >> 2u);
    bits = ((bits & 0x0F0F0F0Fu) << 4u) | ((bits & 0xF0F0F0F0u) >> 4u);
    bits = ((bits & 0x00FF00FFu) << 8u) | ((bits & 0xFF00FF00u) >> 8u);
    return float(bits) * 2.3283064365386963e-10;
}

vec2 hammersley2d(uint i, uint N)
{
    return vec2(float(i) / float(N), radicalInverse_VdC(i));
}

float rand(vec2 co)
{
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}

float _linearize(float depth, float n, float f)
{
	return (2.0 * n * f) / (f + n - depth * (f - n));
}

void main()
{
	vec2 uv = gl_FragCoord.xy * resolutionInverse;
	vec4 src = texture(samplerSrc, uv);
	float srcDepth = texture(samplerDepth, uv).r;
	srcDepth = _linearize(srcDepth, 0.1, 1000.0);
	
	float sum = 0.0;
	for (uint i = 0; i < SSAO_SAMPLES; i++)
	{
		vec2 xi = hammersley2d(i, SSAO_SAMPLES);
		float w = radicalInverse_VdC(i);
		vec2 sampleUV = (xi - vec2(0.5)) * w * 2.0 * resolutionInverse * radius / max(srcDepth * 0.05, 1.0);

		//vec2 sampleUV = (vec2(rand(vec2(gl_FragCoord.x, i)), rand(vec2(i, gl_FragCoord.y))) - 0.5) * resolutionInverse * radius / max(srcDepth * 0.05, 1.0);

		float sampleDepth = texture(samplerDepth, uv + sampleUV).r;
		sampleDepth = _linearize(sampleDepth, 0.1, 1000.0);
		if (abs(sampleDepth - srcDepth) > ignoreDist) sum -= abs(sampleDepth - srcDepth);
		sum += srcDepth - sampleDepth;
	}

	sum /= SSAO_SAMPLES;
	sum -= 0.01;
	sum = max(sum, 0.0);

	float ao = 1.0 - (sum * attenPower);
	ao = max(ao, 0.3);
	//FragColor = max(vec4(vec3(ao), src.a), vec4(0.0));

	vec3 color = src.rgb * ao;
	FragColor = max(vec4(color, src.a), vec4(0.0));

	/*
	float atten = 1.0;
	float sub = 1.0 / SSAO_SAMPLES;
    for (uint i = 0; i < SSAO_SAMPLES; i++)
	{
		vec2 xi = hammersley2d(i, SSAO_SAMPLES);
		float w = radicalInverse_VdC(i);
		vec2 sampleUV = (xi - vec2(0.5)) * w * 2.0 * resolutionInverse * radius;

		float sampleDepth = texture(samplerDepth, uv + sampleUV).r;
		sampleDepth = _linearize(sampleDepth, 0.1, 1000.0);
		if (abs(sampleDepth - srcDepth) > ignoreDist) continue;
		if (sampleDepth < srcDepth) atten -= sub;
	}
	atten *= 2.4;
	atten = pow(min(atten, 1.0), attenPower);
	
	vec3 attenColor = pow(src.rgb, vec3(1.0 - atten));

	vec3 color = src.rgb * attenColor;
	FragColor = max(vec4(color, src.a), vec4(0.0));
	*/


}