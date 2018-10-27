#version 450
#extension GL_EXT_draw_buffers : enable

#define VELOCITYMAP
#define SHADOWMAP

struct Surface
{
    vec3 wPos;
	vec4 pos;
	vec4 dir;
	vec3 normal;
	vec3 tangent;
	vec3 binormal;
	vec2 uv;
	vec4 color;
	vec4 shadowCoord1;
	vec4 shadowCoord2;
	vec4 shadowCoord3;
};

uniform mat4 MIT;

uniform vec4 albedo = vec4(1.0);
uniform float metallic = 0.0;
uniform float roughness = 0.7;
uniform vec4 f0 = vec4(1.022, 0.782, 0.344, 1.0);
uniform vec4 uniqueColor = vec4(0.5, 0.5, 0.5, 1.0);
uniform vec4 emissive = vec4(0.0);
uniform vec4 multColor = vec4(1.0);

//uniform vec3 wCamDir = vec3(0.0, 0.0, 1.0);
uniform vec3 wCamPos = vec3(0.0);
//uniform vec2 nearFar = vec2(0.1, 1000.0);
uniform vec2 resolutionInverse = vec2(1.0 / 1280.0, 1.0 / 720);

uniform float shadowBias1 = 0.0004;
uniform float shadowBias2 = 0.0005;
uniform float shadowBias3 = 0.0004;
uniform float shadowAtten = 0.0;

layout (binding = 0) uniform sampler2D albedoMap;
layout (binding = 1) uniform sampler2D normalMap;
layout (binding = 3) uniform sampler2D shadowMap1;
layout (binding = 4) uniform sampler2D shadowMap2;
layout (binding = 5) uniform sampler2D shadowMap3;

in Surface In;

layout (location = 0) out vec4 Albedo;
layout (location = 1) out vec4 WorldPos;
layout (location = 2) out vec4 WorldNormal;
layout (location = 3) out vec4 PhysicalParams;
layout (location = 4) out vec4 F0;
layout (location = 5) out vec4 Depth;
layout (location = 6) out vec4 Shadow;
layout (location = 7) out vec4 Velocity;


//
// 高品質ソフトシャドー
//
vec2 poissonDisk[4] = vec2[](
  vec2( -0.94201624, -0.39906216 ),
  vec2( 0.94558609, -0.76890725 ),
  vec2( -0.094184101, -0.92938870 ),
  vec2( 0.34495938, 0.29387760 )
);

vec2 poissonDisk2[12] = vec2[](
  vec2(-.326,-.406),
  vec2(-.696, .457),
  vec2( .962,-.195),
  vec2( .519, .767),
  vec2(-.840,-.074),
  vec2(-.203, .621),
  vec2( .473,-.480),
  vec2( .185,-.893),
  vec2( .507, .064),
  vec2( .896, .412),
  vec2(-.322,-.933),
  vec2(-.792,-.598)
);

float calcNearShadow()
{
	if (In.shadowCoord3.x > 0.0 && In.shadowCoord3.x < 1.0 && In.shadowCoord3.y > 0.0 && In.shadowCoord3.y < 1.0)
	{
		float rate = 0.0;
		for (int i = 0; i < 12; i++)
		{
			vec2 coord = In.shadowCoord3.xy + poissonDisk2[i] * resolutionInverse;
			float r = step(texture(shadowMap3, coord).r, In.shadowCoord3.z - shadowBias3);
			rate += mix(1.0, shadowAtten, r);
		}
		rate /= 12.0;
		return rate;
	}
	return 1.0;
}

float calcMiddleShadow()
{
	if (In.shadowCoord2.x > 0.0 && In.shadowCoord2.x < 1.0 && In.shadowCoord2.y > 0.0 && In.shadowCoord2.y < 1.0)
	{
		float rate = 0.0;
		for (int i = 0; i < 4; i++)
		{
			vec2 coord = In.shadowCoord2.xy + poissonDisk[i] * resolutionInverse * 0.5;
			float r = step(texture(shadowMap2, coord).r, In.shadowCoord2.z - shadowBias2);
			rate += mix(1.0, shadowAtten, r);
		}
		rate *= 0.25;
		return rate;
	}
	return 1.0;
}

float calcSoftShadow2()
{
	float dist = length(wCamPos - In.wPos);
	if (dist < 5)
	{
		return calcNearShadow();
	}
	else if (dist < 16)
	{
		return calcMiddleShadow();
	}
	else if (dist < 20)
	{
		return mix(calcMiddleShadow(), 1.0, 1.0 - ((20 - dist) * 0.25));
	}
	return 1.0;
}

void main()
{
	vec3 nMap = texture(normalMap, In.uv).rgb * 2.0 - vec3(1.0);
	mat3 normalMat = mat3(normalize(In.tangent), normalize(In.binormal), normalize(In.normal));
	vec3 nMapO = normalize(normalMat * nMap);
	vec3 n = normalize(mat3(MIT) * nMapO);

	vec4 surfColor = In.color * albedo * texture(albedoMap, In.uv) + emissive;
	float alpha = surfColor.a;

	float gray = length(surfColor.rgb);
	float dd = fwidth(gray);

	Albedo = max(surfColor, vec4(0.0));
	WorldPos = vec4(In.wPos, alpha);
	vec3 wn = n * 0.5 + vec3(0.5);
	wn = clamp(wn, vec3(0.0), vec3(1.0));
	if (length(wn) == 0.0) wn = vec3(0.5, 0.5, 1.0);
	WorldNormal = vec4(wn, alpha);
	PhysicalParams = vec4(roughness, metallic, 0.0, 1.0);
	F0 = vec4(f0.rgb, alpha);
	Depth = vec4(gl_FragCoord.z, 0.0, 0.0, alpha);

#ifdef SHADOWMAP
	Shadow = vec4(calcSoftShadow2(), 0.0, 0.0, alpha);
#endif

#ifdef VELOCITYMAP
	float l = length(In.dir.xy);
	l /= In.dir.z;
	vec2 dir =clamp(normalize(In.dir.xy) * 0.5 + vec2(0.5), 0.0, 1.0);
	Velocity = vec4(dir, l, In.dir.z / In.dir.w);
#endif
}