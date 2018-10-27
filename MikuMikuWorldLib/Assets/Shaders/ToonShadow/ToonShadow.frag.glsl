#version 420

require(functions)

struct Surface
{
    vec3 wPos;
	vec3 wNormal;
	vec2 uv;
	vec4 color;
	vec4 shadowCoord1;
	vec4 shadowCoord2;
	vec4 shadowCoord3;
};

struct DirectionalLight
{
	vec3 dir;
	vec4 color;
	float intensity;
};

uniform vec4 diffuse = vec4(1.0);
uniform vec4 specular = vec4(0.5);
uniform float shininess = 5.0;
uniform vec4 uniqueColor = vec4(1.0);

uniform vec3 wCamDir = vec3(0.0, 0.0, 1.0);
uniform vec3 wCamPos = vec3(0.0);

uniform DirectionalLight wDirLight;

uniform vec4 gAmbient = vec4(0.7, 0.7, 0.7, 0.0);
uniform float contrast = 1.0;
uniform float saturation = 1.0;
uniform float brightness = 1.0;
uniform float limCoeff = 0.3;

uniform float shadowBias1 = 0.0004;
uniform float shadowBias2 = 0.0005;
uniform float shadowBias3 = 0.0006;
uniform float shadowAtten = 0.0;

layout (binding = 0) uniform sampler2D albedoMap;
layout (binding = 1) uniform sampler2D toonMap;
layout (binding = 2) uniform sampler2D shadowMap1;
layout (binding = 3) uniform sampler2D shadowMap2;
layout (binding = 4) uniform sampler2D shadowMap3;

in Surface In;
layout (location = 0) out vec4 FragColor0;
//layout (location = 1) out vec4 FragColor1;
//layout (location = 2) out vec4 FragColor2;

float calcShadow()
{
	float dist = length(wCamPos - In.wPos);
	if (dist < 5)
	{
		if (In.shadowCoord3.x > 0.0 && In.shadowCoord3.x < 1.0 && In.shadowCoord3.y > 0.0 && In.shadowCoord3.y < 1.0)
		{
			float rate = step(texture2D(shadowMap3, In.shadowCoord3.xy).r, In.shadowCoord3.z - shadowBias3);
			return mix(1.0, shadowAtten, rate);
		}
	}
	else if (dist < 20)
	{
		if (In.shadowCoord2.x > 0.0 && In.shadowCoord2.x < 1.0 && In.shadowCoord2.y > 0.0 && In.shadowCoord2.y < 1.0)
		{
			float rate = step(texture2D(shadowMap2, In.shadowCoord2.xy).r, In.shadowCoord2.z - shadowBias2);
			return mix(1.0, shadowAtten, rate);
		}
	}
	else
	{
		float rate = step(texture2D(shadowMap1, In.shadowCoord1.xy).r, In.shadowCoord1.z - shadowBias1);
		return mix(1.0, shadowAtten, rate);
	}
	return 1.0;
}

void main()
{
	vec3 normal = normalize(In.wNormal);
	vec3 surfToEye = normalize(wCamPos - In.wPos);
	vec4 surfColor = In.color * diffuse * texture(albedoMap, In.uv);

	vec3 dif = vec3(0);
	vec3 spe = vec3(0);
	vec3 amb = vec3(0);

	float NdotV = max(dot(surfToEye, normal), 0.0);

	// ambient
	amb += gAmbient.rgb * surfColor.rgb;

	// directional light
	{
		vec3 surfToLight = normalize(-wDirLight.dir);

		float shadow = calcShadow();

		// diffuse
		float NdotL = dot(normal, surfToLight);
		float toonCoord = 1.0 - ((NdotL + 1.0) * 0.5);
		vec3 toon = texture2D(toonMap, vec2(toonCoord)).rgb;
		vec3 atten = min(vec3(shadow), toon);
		dif = pow(surfColor.rgb, vec3(2.0) - atten);
		dif += pow(1.0 - NdotV, 2.5) * limCoeff * 0.0;

		// specular
		spe = vec3(1.0 - step(_specular(surfToEye, normal, surfToLight, shininess), 0.7));
		spe *= specular.rgb * 0.25;

		vec3 lightAtten = wDirLight.color.rgb * wDirLight.intensity * 0.1;

		dif *= lightAtten;
		spe *= lightAtten;
	}

	vec3 color = dif + spe + amb;
	color = _csb(color, contrast, saturation, brightness);
	FragColor0 = vec4(color, surfColor.a);
	//FragColor1 = vec4(vec3(gl_FragCoord.z), 1.0);
	//FragColor2 = vec4(uniqueColor.rgb, 1.0);
}