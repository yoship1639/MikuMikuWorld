#version 420

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

uniform vec2 resolutionInverse = vec2(1.0 / 1280.0, 1.0 / 720.0);
uniform vec2 nearFar = vec2(0.1, 1000.0);
uniform vec3 wCamDir;
uniform vec3 wCamPos;
uniform vec4 gAmbient = vec4(0.3, 0.3, 0.3, 0.0);
uniform float iblIntensity = 1.0;

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
	surfColor.rgb = pow(surfColor.rgb, vec3(2.2));

	vec3 pos = texture(samplerWorldPos, coord).xyz;
	vec3 n = normalize(texture(samplerWorldNormal, coord).xyz - vec3(1.0));
	vec2 pp = texture(samplerPhysicalParams, coord).xy;
	vec3 f0 = texture(samplerF0, coord).rgb;
	float shadow = texture(samplerShadow, coord).x;
	float roughness = pp.x;
	float metallic = pp.y;
	vec3 v = normalize(wCamPos - pos);

	vec3 diff = vec3(0);
	vec3 spec = vec3(0);
	vec3 amb = gAmbient.rgb * surfColor.rgb;

	// directional light
	{
		vec3 l = normalize(-wDirLight.dir);
		float NdotL = max(dot(n, l), 0.0);
		NdotL = dot(n, l);
		
		// diffuse
		vec3 d = _diffuseBRDF(surfColor.rgb);

		// specular
		float sp = _specular(v, n, l, 10.0);
		sp = 1.0 - step(sp, 0.5);
		vec3 s = vec3(sp) * f0;
		s = pow(s, vec3(2.2));

		float nl = 1.0;
		if (NdotL < 0.0) nl = 0.5;

		float sha = 1.0;
		if (shadow == 0.0 && nl == 1.0) sha = 0.5;

		vec3 lightAtten = wDirLight.color.rgb * wDirLight.intensity * nl * sha;

		diff += d * lightAtten;
		spec += s * lightAtten;
	}

	vec3 color = diff + spec + amb;
	//color *= (shadow * 0.5 + 0.5);
	color = pow(color, vec3(_GAMMA));
	color = max(color, vec3(0.0));
    FragColor = vec4(color, surfColor.a);
}