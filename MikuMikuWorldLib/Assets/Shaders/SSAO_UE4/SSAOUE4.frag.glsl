#version 420

#define SSAO_SAMPLES 6
#define OVERPI 0.31830988

uniform vec2 resolutionInverse = vec2(1.0 / 1280.0, 1.0 / 720);
uniform float radius = 2.0;
uniform float depthBias = 0.08;
uniform float strength = 1.0;
uniform vec2 nearFar = vec2(0.1, 1000.0);

layout (binding = 0) uniform sampler2D samplerSrc;
layout (binding = 1) uniform sampler2D samplerDepth;

layout (location = 0) out vec4 FragColor;

vec2 poissonDisk2[12] = vec2[](
  vec2(-.326,-.406),
  vec2(-.840,-.074),
  vec2(-.696, .457),
  vec2(-.203, .621),
  vec2( .962,-.195),
  vec2( .473,-.480),
  vec2( .519, .767),
  vec2( .185,-.893),
  vec2( .507, .064),
  vec2( .896, .412),
  vec2(-.322,-.933),
  vec2(-.792,-.598)
);

const vec2 sampCoord[6] =
{
	vec2(1.0, 0.0),
	vec2(0.866, 0.5),
	vec2(0.5, 0.866),
	vec2(0.0, 1.0),
	vec2(-0.5, 0.866),
	vec2(-0.866, 0.5)
};

float tangent(vec3 p, vec3 s)
{  
    return (p.z - s.z) / length(s.xy - p.xy);  
}

vec3 contrast(vec3 color, float con)
{
	const vec3 AvgLum = vec3(0.5);
	return max(mix(AvgLum, color, con), vec3(0));
}

float linearize(float depth, float n, float f)
{
	return (2.0 * n * f) / (f + n - depth * (f - n));
}

void main()
{
	vec2 uv = gl_FragCoord.xy * resolutionInverse;
	
	float srcDepth = linearize(texture(samplerDepth, uv).r, nearFar.x, nearFar.y);
	vec3 pos = vec3(uv, srcDepth);

	float atten = 0.0;

	for (int i = 0; i < 6; i++)
	{
		vec2 samp = sampCoord[i] * resolutionInverse * radius / clamp(srcDepth, 1.0, 4.0);
		//vec2 samp = poissonDisk2[i] * resolutionInverse * radius * 0.5;

		// left
		vec2 sl = uv + samp;
		float dl = linearize(texture(samplerDepth, sl).r, nearFar.x, nearFar.y);
		vec3 pl = vec3(sl, dl);
		if (pl.z < pos.z - depthBias) continue;
		float tl = atan(tangent(pos, pl));

		// right
		vec2 sr = uv - samp;
		float dr = linearize(texture(samplerDepth, sr).r, nearFar.x, nearFar.y);
		vec3 pr = vec3(sr, dr);
		if (pr.z < pos.z - depthBias) continue;
		float tr = atan(tangent(pos, pr));

		atten += clamp((tl + tr) * OVERPI * strength, 0.0, 1.0);
	}

	atten = clamp(atten * 0.16666666, 0.0, 1.0);

	vec4 src = texture(samplerSrc, uv);
	//src = vec4(1.0);
	FragColor = vec4(src.rgb * (1.0 - atten), src.a);
	//FragColor = vec4(pow(src.rgb, vec3(1.0 + atten)), src.a);
	//FragColor = vec4(contrast(src.rgb, 1.0 + atten), src.a);
}