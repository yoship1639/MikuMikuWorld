#version 420

require(functions)

uniform vec2 resolutionInverse = vec2(1.0 / 1280.0, 1.0 / 720.0);
uniform vec2 nearFar = vec2(0.1, 1000.0);
uniform float baseDepth = 5.0;
uniform float startDist = 0.0;
uniform float transDist = 5.0;

layout (binding = 0) uniform sampler2D samplerSrc;
layout (binding = 1) uniform sampler2D samplerBlur;
layout (binding = 2) uniform sampler2D samplerDepth;

layout (location = 0) out vec4 FragColor;

void main()
{
	vec2 coord = gl_FragCoord.xy * resolutionInverse;
	float depth = _linearize(texture(samplerDepth, coord).r, nearFar.x, nearFar.y);

	float absDist = abs(depth - baseDepth);
	float sDist = absDist - startDist;
	float rate = clamp(sDist / transDist, 0.0, 1.0);

	vec4 src = texture(samplerSrc, coord);
	vec4 blur = texture(samplerBlur, coord);

    FragColor = mix(src, blur, rate);
}