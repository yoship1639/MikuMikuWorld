#version 450

require(functions)

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

uniform Light wDirLight;

uniform vec2 resolution = vec2(1280.0, 720.0);
uniform vec2 resolutionInverse = vec2(1.0 / 1280.0, 1.0 / 720.0);
uniform vec2 nearFar = vec2(0.1, 1000.0);
uniform vec3 wCamDir;
uniform vec3 wCamPos;
uniform vec4 gAmbient = vec4(0.3, 0.3, 0.3, 0.0);
uniform float iblIntensity = 1.0;
uniform vec4 fogColor = vec4(52.0 / 255.0, 76.0 / 255.0, 110.0 / 255.0, 1.0);
uniform float fogIntensity = 1.0;

layout (std430, binding = 3) volatile buffer LightCount {
  int data[];
} gLightCount;

layout (std430, binding = 4) volatile buffer LightIndex {
  int data[];
} gLightIndex;

struct BufferLight
{
	vec4 pos;
	vec4 color;
	vec4 dir;
	float intensity;
	float radius;
	float specCoeff;
	float innerDot;
	float outerDot;
	vec3 temp;
};

layout (std430, binding = 5) readonly buffer Lights {
  BufferLight data[];
} gLights;

layout (binding = 0) uniform sampler2D samplerAlbedo;
layout (binding = 1) uniform sampler2D samplerWorldPos;
layout (binding = 2) uniform sampler2D samplerWorldNormal;
layout (binding = 3) uniform sampler2D samplerPhysicalParams;
layout (binding = 4) uniform sampler2D samplerF0;
layout (binding = 5) uniform sampler2D samplerDepth;
layout (binding = 6) uniform sampler2D samplerShadow;
//layout (binding = 7) uniform sampler2D samplerVelocity;
layout (binding = 8) uniform samplerCube samplerEnv;

layout (location = 0) out vec4 FragColor;

void main()
{
	vec2 coord = gl_FragCoord.xy * resolutionInverse;
	vec4 surfColor = texture(samplerAlbedo, coord);
	if (surfColor.a == 0.0) discard;
	float depth = texture(samplerDepth, coord).x;
	if (depth >= 1.0)
	{
		FragColor = surfColor;
		return;
	}
	surfColor.rgb = pow(surfColor.rgb, vec3(2.2));

	vec3 pos = texture(samplerWorldPos, coord).xyz;
	vec3 n = normalize((texture(samplerWorldNormal, coord).xyz - vec3(0.5)) * 2.0);
	vec2 pp = texture(samplerPhysicalParams, coord).xy;
	vec3 f0 = texture(samplerF0, coord).rgb;
	float shadow = texture(samplerShadow, coord).x;
	float roughness = clamp(pp.x, 0.0, 1.0);
	float metallic = pp.y;
	vec3 v = normalize(wCamPos - pos);

	vec3 diff = vec3(0);
	vec3 spec = vec3(0);
	vec3 amb = gAmbient.rgb * surfColor.rgb;

	vec3 diffuse = mix(surfColor.rgb, vec3(0.0), metallic);
	vec3 specular = mix(vec3(0.04), surfColor.rgb, metallic);

	// directional light
	{
		vec3 l = normalize(-wDirLight.dir);
		float NdotL = dot(n, l);
		
		float nl = clamp(NdotL, 0.0, 1.0);
		nl = NdotL * 0.5 + 0.5;

		// diffuse
		vec3 d = _diffuseBRDF(diffuse);

		// specular
		vec3 s = _specularBRDF2(specular, roughness, v, n, l);
		//s = pow(s, vec3(2.2));

		vec3 lightAtten = wDirLight.color.rgb * wDirLight.intensity * nl * (shadow * 0.75 + 0.25);

		diff += d * lightAtten;
		spec += s * lightAtten;
	}

	vec2 indices = gl_FragCoord.xy / 16.0;
	int sx = (int(resolution.x) / 16) + 1;
	int sy = (int(resolution.y) / 16) + 1;
	int x = int(indices.x);
	int y = int(indices.y);

	// point light
	int count = gLightCount.data[y * sx + x];
	for (int i = 0; i < 64; i++)
	{
		if (i == count) break;

		int idx = gLightIndex.data[(y * sx + x) * 64 + i];
		BufferLight light = gLights.data[idx];

		vec3 l = normalize(light.pos.xyz - pos);
		float NdotL = dot(n, l);
		float nl = clamp(NdotL * 0.5 + 0.5, 0.0, 1.0);

		// diffuse
		vec3 d = _diffuseBRDF(diffuse);

		// specular
		vec3 s = vec3(0);
		if (NdotL > 0.0)
		{
			s = _specularBRDF2(specular, roughness, v, n, l) * light.specCoeff;
			//s = pow(s, vec3(2.2));
		}
		//vec3 

		float dist = length(pos - light.pos.xyz);
		vec3 lightAtten = light.color.rgb * _physicalLightAtten5(dist, light.radius, light.intensity) * nl;

		diff += d * lightAtten;
		spec += s * lightAtten;
	}

	// IBL
	vec3 ibl = vec3(0);
	if (iblIntensity > 0) 
	{
		// diffuse
		ibl += _diffuseBRDF(surfColor.rgb) * _diffuseIBL(roughness, n, v, samplerEnv) * f0.rgb * iblIntensity;

		// specular
		ibl += _specularIBL(f0.rgb, roughness, n, v, samplerEnv) * f0.rgb * iblIntensity;
	}

	vec3 color = diff + spec + amb + ibl;
	color = pow(color, vec3(_GAMMA));
	color = max(color, vec3(0.0));

	float lineardepth = _linearize(depth, nearFar.x, nearFar.y);
	float fogRate = clamp(lineardepth * fogIntensity / 500.0, 0.0, 1.0);
	//vec3 env = texture(samplerEnv, vec3(v.x, -v.y, -v.z), 6.0).rgb;
	vec3 env = fogColor.rgb;

    FragColor = vec4(mix(color, env * 0.75, fogRate), 1.0);
}