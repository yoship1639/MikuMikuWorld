#version 420

require(functions)

struct Surface
{
    vec3 wPos;
	vec3 wNormal;
	vec3 normal;
	vec3 tangent;
	vec3 binormal;
	vec3 tanLightDir;
	vec2 uv;
	vec4 color;
};

struct DirectionalLight
{
	vec3 dir;
	vec4 color;
	float intensity;
};

uniform vec4 diffuse = vec4(1.0);
uniform float metallic = 0.0;
uniform float roughness = 0.7;
uniform vec3 f0 = vec3(1.022, 0.782, 0.344);

uniform vec3 wCamDir = vec3(0.0, 0.0, 1.0);
uniform vec3 wCamPos = vec3(0.0);

uniform DirectionalLight wDirLight;

uniform vec4 gAmbient = vec4(0.3, 0.3, 0.3, 0.0);
uniform float brightness = 1.0f;
uniform float saturation = 1.0f;
uniform float contrast = 1.0f;

layout (binding = 0) uniform sampler2D albedoMap;
layout (binding = 1) uniform sampler2D normalMap;
layout (binding = 2) uniform sampler2D environmentMap;
layout (binding = 3) uniform sampler2D environmentMapDiffuse;

in Surface In;
layout( location = 0 ) out vec4 FragColor;
 
void main()
{
	vec3 normal = normalize(In.wNormal);
	vec3 surfToEye = normalize(wCamPos - In.wPos);
	vec4 surfColor = diffuse * texture2D(albedoMap, In.uv);

	vec3 dif = vec3(0);
	vec3 spe = vec3(0);
	vec3 amb = vec3(0);

	// directional light
	{
		vec3 surfToLight = normalize(-wDirLight.dir);
		vec3 f = wDirLight.color.rgb;

		// diffuse
		float NdotL = max(0.0, dot(normal, surfToLight));
		vec3 nonMetalDif = surfColor.rgb * NdotL * _OVERPI;
		vec3 d = mix(nonMetalDif, vec3(0), metallic);

		// specular
		vec3 metalSpe = _specularBRDF(f, surfToEye, normal, surfToLight, roughness);
		vec3 nonMetalSpe = _specularBRDF(vec3(0.04), surfToEye, normal, surfToLight, roughness);
		vec3 s = max(vec3(0), mix(nonMetalSpe, metalSpe, metallic));

		// ambient
		vec3 difAmb = surfColor.rgb * gAmbient.rgb * _OVERPI;
		vec3 speAmb = f * gAmbient.rgb * _OVERPI;
		vec3 a = mix(difAmb, speAmb, metallic);

		vec3 atten = wDirLight.color.rgb * wDirLight.intensity;

		dif += d * atten;
		spe += s * atten;
		amb += a;
	}

	// IBL
	vec3 ibl = vec3(0);
	{
		vec3 d = mix(surfColor.rgb, vec3(0), metallic);
		vec3 s = mix(vec3(0), f0, metallic);

		vec3 refVec = reflect(-surfToEye, normal);

		ibl = ImageBasedLighting(environmentMap, environmentMapDiffuse, refVec, normal, surfToEye, d, s, roughness, 1.0, 5.0);
	}
	vec3 color = saturate(dif + spe + amb + ibl);

	color = pow(color, vec3(_GAMMA));
	color = _csb(color, 2.4, saturation, brightness);
	FragColor = vec4(color, surfColor.a);
}