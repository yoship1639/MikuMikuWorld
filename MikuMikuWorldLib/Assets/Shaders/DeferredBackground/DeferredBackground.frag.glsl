#version 450
#extension GL_EXT_draw_buffers : enable

#define VELOCITYMAP

struct Surface
{
	vec3 pos;
	vec3 wPos;
	vec4 dir;
};

uniform vec4 albedo = vec4(1.0);
uniform vec4 fogColor = vec4(52.0 / 255.0, 76.0 / 255.0, 110.0 / 255.0, 1.0);
uniform float fogStrength = 1.0;

layout (binding = 0) uniform samplerCube envMap;

in Surface In;

layout (location = 0) out vec4 Albedo;
layout (location = 1) out vec4 WorldPos;
layout (location = 2) out vec4 WorldNormal;
layout (location = 3) out vec4 PhysicalParams;
layout (location = 4) out vec4 F0;
layout (location = 5) out vec4 Depth;
layout (location = 6) out vec4 Shadow;
layout (location = 7) out vec4 Velocity;


void main()
{
	vec4 env = albedo * texture(envMap, In.pos * vec3(-1.0, 1.0, 1.0), clamp(fogStrength * 0.5, 0.0, 5.0));
	env = mix(env, fogColor, clamp(fogStrength * 0.1, 0.0, 1.0));
	vec4 surfColor = env;
	float alpha = surfColor.a;

	Albedo = max(surfColor, vec4(0.0));
	WorldPos = vec4(In.wPos, alpha);
	WorldNormal = vec4(vec3(0, 0, 1.0), alpha);
	PhysicalParams = vec4(0.0, 0.0, 0.0, alpha);
	F0 = vec4(vec3(0.0, 0.0, 0.0), alpha);
	Depth = vec4(1.0, 0.0, 0.0, alpha);

#ifdef SHADOWMAP
	Shadow = vec4(calcSoftShadow2() * 0.75 + 0.25, 0.0, 0.0, alpha);
#endif

#ifdef VELOCITYMAP
	float l = length(In.dir.xy);
	l /= In.dir.z;
	vec2 dir = clamp(normalize(In.dir.xy) * 0.5 + vec2(0.5), 0.0, 1.0);
	Velocity = vec4(dir, l, In.dir.z / In.dir.w);
#endif
}