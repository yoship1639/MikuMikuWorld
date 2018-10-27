#version 420

require(functions)

struct Surface
{
    vec3 wPos;
	vec3 wNormal;
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
uniform vec4 specular = vec4(0.5);
uniform float shininess = 5.0;

uniform vec3 wCamDir = vec3(0.0, 0.0, 1.0);
uniform vec3 wCamPos = vec3(0.0);

uniform DirectionalLight wDirLight;

uniform vec4 gAmbient = vec4(0.7, 0.7, 0.7, 0.0);
uniform float contrast = 1.0;
uniform float saturation = 1.0;
uniform float brightness = 1.0;

layout (binding = 0) uniform sampler2D albedoMap;
layout (binding = 1) uniform sampler2D toonMap;

in Surface In;
layout( location = 0 ) out vec4 FragColor;

void main()
{
	vec3 normal = normalize(In.wNormal);
	vec3 surfToEye = normalize(wCamPos - In.wPos);
	vec4 surfColor = In.color * diffuse * texture2D(albedoMap, In.uv);

	vec3 dif = vec3(0);
	vec3 spe = vec3(0);
	vec3 amb = vec3(0);

	// directional light
	{
		vec3 surfToLight = normalize(-wDirLight.dir);

		// diffuse
		float NdotL = dot(normal, surfToLight);
		float toonCoord = 1.0 - ((NdotL + 1.0) * 0.5);
		dif = surfColor.rgb * texture2D(toonMap, vec2(toonCoord)).rgb * _OVERPI;

		// specular
		spe = vec3(1.0 - step(_specular(surfToEye, normal, surfToLight, shininess), 0.7));
		spe *= specular.rgb * _OVERPI;

		// ambient
		amb += gAmbient.rgb * surfColor.rgb * _OVERPI * 1.42;

		float atten = wDirLight.intensity * 0.7;

		dif *= atten;
		spe *= atten;
	}

	vec3 color = saturate(dif + spe + amb);
	color = pow(color, vec3(_GAMMA));
	color = _csb(color, 2.4 * contrast, 1.5 * saturation, 1.1 * brightness);
	FragColor = vec4(color, surfColor.a);
}