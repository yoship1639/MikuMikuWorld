#version 420

struct Surface
{
    vec3 wPos;
	vec4 shadowCoord1;
	vec4 shadowCoord2;
	vec4 shadowCoord3;
};

uniform vec3 wCamPos = vec3(0.0);

uniform float shadowBias1 = 0.001;
uniform float shadowBias2 = 0.001;
uniform float shadowBias3 = 0.0004;
uniform float shadowAtten = 0.25;

layout (binding = 0) uniform sampler2D shadowMap1;
layout (binding = 1) uniform sampler2D shadowMap2;
layout (binding = 2) uniform sampler2D shadowMap3;

in Surface In;
layout( location = 0 ) out vec4 FragColor;

void main()
{
	float dist = length(wCamPos - In.wPos);
	float atten = 1.0;
	if (dist < 5)
	{
		if (In.shadowCoord3.x > 0.0 && In.shadowCoord3.x < 1.0 && In.shadowCoord3.y > 0.0 && In.shadowCoord3.y < 1.0)
		{
			float rate = step(texture2D(shadowMap3, In.shadowCoord3.xy).r, In.shadowCoord3.z - shadowBias3);
			atten *= mix(1.0, shadowAtten, rate);
		}
	}
	else if (dist < 20)
	{
		if (In.shadowCoord2.x > 0.0 && In.shadowCoord2.x < 1.0 && In.shadowCoord2.y > 0.0 && In.shadowCoord2.y < 1.0)
		{
			float rate = step(texture2D(shadowMap2, In.shadowCoord2.xy).r, In.shadowCoord2.z - shadowBias2);
			atten *= mix(1.0, shadowAtten, rate);
		}
	}
	else
	{
		float rate = step(texture2D(shadowMap1, In.shadowCoord1.xy).r, In.shadowCoord1.z - shadowBias1);
		atten *= mix(1.0, shadowAtten, rate);
	}
	FragColor = vec4(vec3(atten), 1.0);
}