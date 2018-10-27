#version 450

require(functions)

replace(POINT_LIGHT_NUM)
replace(SPOT_LIGHT_NUM)

struct Surface
{
    vec3 wPos;
	vec3 wNormal;
	vec3 normal;
	vec3 tangent;
	vec3 binormal;
	vec2 uv;
	vec4 color;
	vec4 shadowCoord1;
	vec4 shadowCoord2;
	vec4 shadowCoord3;
};

struct Light
{
	vec3 pos;
	vec3 dir;
	vec4 color;
	float radius;
	float intensity;
	vec3 min;
	vec3 max;
	float innerAngle;
	float outerAngle;
};

uniform mat4 MIT;

uniform vec4 albedo = vec4(1.0);
uniform vec4 emissive = vec4(0.0);
uniform float metallic = 0.0;
uniform float roughness = 0.7;
uniform vec4 f0 = vec4(1.022, 0.782, 0.344, 1.0);

uniform vec3 wCamDir = vec3(0.0, 0.0, 1.0);
uniform vec3 wCamPos = vec3(0.0);
uniform vec2 nearFar = vec2(0.1, 1000.0);
uniform vec2 resolutionInverse = vec2(1.0 / 1280.0, 1.0 / 720);

uniform Light wDirLight;
uniform Light wPointLights[POINT_LIGHT_NUM];
uniform Light wSpotLights[SPOT_LIGHT_NUM];

uniform vec4 gAmbient = vec4(0.3, 0.3, 0.3, 0.0);
uniform float iblIntensity = 1.0;
uniform float brightness = 1.0;
uniform float saturation = 1.0;
uniform float contrast = 1.0;

uniform float shadowBias1 = 0.0004;
uniform float shadowBias2 = 0.0005;
uniform float shadowBias3 = 0.0003;
uniform float shadowAtten = 0.0;

layout (binding = 0) uniform sampler2D albedoMap;
layout (binding = 1) uniform sampler2D normalMap;
layout (binding = 2) uniform samplerCube environmentMap;
layout (binding = 3) uniform sampler2D shadowMap1;
layout (binding = 4) uniform sampler2D shadowMap2;
layout (binding = 5) uniform sampler2D shadowMap3;

in Surface In;
layout( location = 0 ) out vec4 FragColor;

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
	//vec3 n = normalize(In.wNormal);

	vec3 v = normalize(wCamPos - In.wPos);
	vec4 surfColor = In.color * albedo * texture(albedoMap, In.uv);
	surfColor.rgb = pow(surfColor.rgb, vec3(2.2));

	vec3 diff = vec3(0);
	vec3 spec = vec3(0);
	vec3 amb = gAmbient.rgb * surfColor.rgb + emissive.rgb;

	vec3 diffuse = mix(surfColor.rgb, vec3(0.0), metallic);
	vec3 specular = mix(vec3(0.04), surfColor.rgb, metallic);

	// directional light
	{
		vec3 l = normalize(-wDirLight.dir);
		float NdotL = dot(n, l);
		
		float nl = clamp(NdotL, 0.0, 1.0);

		// diffuse
		vec3 d = _diffuseBRDF(diffuse);

		// specular
		vec3 s = _specularBRDF2(specular, roughness, v, n, l);
		//s = pow(s, vec3(2.2));

		vec3 lightAtten = wDirLight.color.rgb * wDirLight.intensity * nl;// * calcSoftShadow2();

		diff += d * lightAtten;
		spec += s * lightAtten;
	}
	
	/*
	// point lights
	for (int i = 0; i < POINT_LIGHT_NUM; i++)
	{
		if (wPointLights[i].intensity <= 0.0) break;
		if (_clip(In.wPos, wPointLights[i].min, wPointLights[i].max) == 0.0) continue;

		vec3 l = normalize(wPointLights[i].pos - In.wPos);
		float NdotL = max(dot(n, l), 0.0);
		if (NdotL == 0.0) continue;
		// diffuse
		vec3 d = _diffuseBRDF(surfColor.rgb);

		// specular
		vec3 s = _specularBRDF2(f0.rgb, roughness, v, n, l);

		float dist = length(In.wPos - wPointLights[i].pos);
		vec3 lightAtten = wPointLights[i].color.rgb;
		lightAtten = lightAtten * _physicalLightAtten5(dist, wPointLights[i].radius, wPointLights[i].intensity);
		lightAtten = pow(lightAtten, vec3(_GAMMA));
		lightAtten *= NdotL;

		diff += d * lightAtten;
		spec += s * lightAtten;
	}
	
	// spot lights
	for (int i = 0; i < SPOT_LIGHT_NUM; i++)
	{
		if (wSpotLights[i].intensity <= 0.0) break;
		if (_clip(In.wPos, wSpotLights[i].min, wSpotLights[i].max) == 0.0) continue;

		vec3 l = normalize(wSpotLights[i].dir);
		vec3 p = normalize(In.wPos - wSpotLights[i].pos);
		float NdotL = max(dot(n, l), 0.0);
		if (NdotL == 0.0) continue;

		// diffuse
		vec3 d = _diffuseBRDF(surfColor.rgb);

		// specular
		vec3 s = _specularBRDF2(f0.rgb, roughness, v, n, l);

		float dist = length(In.wPos - wSpotLights[i].pos);
		vec3 lightAtten = wSpotLights[i].color.rgb;
		lightAtten = lightAtten * _physicalLightAtten5(dist, wSpotLights[i].radius, wSpotLights[i].intensity);
		lightAtten = pow(lightAtten, vec3(_GAMMA));
		lightAtten *= NdotL;

		diff += d * lightAtten;
		spec += s * lightAtten;
	}
	*/

	// IBL
	vec3 ibl = vec3(0);
	if (iblIntensity > 0) 
	{
		// diffuse
		ibl += _diffuseBRDF(surfColor.rgb) * _diffuseIBL(roughness, n, v, environmentMap) * iblIntensity;

		// specular
		ibl += _specularIBL(f0.rgb, roughness, n, v, environmentMap) * iblIntensity;
	}
	vec3 color = diff + spec + amb + ibl;
	color = pow(color, vec3(_GAMMA));
	color = max(color, vec3(0.0));

	//color = _csb(color, contrast, saturation, brightness);
	FragColor = vec4(color, surfColor.a);
}