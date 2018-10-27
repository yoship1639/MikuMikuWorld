#version 420

uniform vec2 resolutionInverse = vec2(1.0 / 1280.0, 1.0 / 720.0);
uniform float length = 8.0;
layout (binding = 0) uniform sampler2D samplerSrc;
layout (binding = 1) uniform sampler2D samplerVelocity;
layout (binding = 2) uniform sampler2D samplerDepth;

layout (location = 0) out vec4 FragColor;

void main()
{
	vec2 uv = gl_FragCoord.xy * resolutionInverse;
	vec4 dir = texture(samplerVelocity, uv);
	if (dir.z == 0.0)
	{
		FragColor = texture(samplerSrc, uv);
		return;
	}
	dir.xy = (dir.xy - vec2(0.5)) * 2.0 * dir.z * length;
	dir.x *= -1.0;
	dir.xy *= resolutionInverse;

	vec4 color = texture(samplerSrc, uv);

	int count = 1;
	for (int i = count; i < 24; i++)
	{
		vec4 vc = texture(samplerSrc, uv + dir.xy * float(i));
		float d = texture(samplerDepth, uv + dir.xy * float(i)).x;

		if (dir.w < d + 0.02)
		{
			count++;
			color += vc;
		}
	}

	color /= float(count);
    
	FragColor = color;
}