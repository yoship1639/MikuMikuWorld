#version 420

uniform vec2 resolutionInverse = vec2(1.0 / 1280.0, 1.0 / 720.0);
uniform float intensity = 1.0;
layout (binding = 0) uniform sampler2D samplerSrc;
layout (binding = 1) uniform sampler2D samplerBlur1;
layout (binding = 2) uniform sampler2D samplerBlur2;
layout (binding = 3) uniform sampler2D samplerBlur3;
layout (binding = 4) uniform sampler2D samplerBlur4;
layout (binding = 5) uniform sampler2D samplerBlur5;
layout (binding = 6) uniform sampler2D samplerBlur6;
layout (binding = 7) uniform sampler2D samplerBlur7;
layout (binding = 8) uniform sampler2D samplerBlur8;
layout (binding = 9) uniform sampler2D samplerBlur9;

layout (location = 0) out vec4 FragColor;

void main()
{
	vec2 uv = gl_FragCoord.xy * resolutionInverse;
	vec4 color = vec4(texture(samplerSrc, uv).rgb, 1.0);
	//color = vec4(vec3(0), 1.0);

	vec3 line = vec3(0);
	line += texture(samplerBlur1, uv).rgb * intensity;
	line += texture(samplerBlur2, uv).rgb * intensity * 2.0;
	line += texture(samplerBlur3, uv).rgb * intensity * 6.0;

	vec3 plus = vec3(0);
	plus += texture(samplerBlur4, uv).rgb * intensity;
	plus += texture(samplerBlur5, uv).rgb * intensity * 2.0;
	plus += texture(samplerBlur6, uv).rgb * intensity * 6.0;

	vec3 star = vec3(0);
	star += texture(samplerBlur7, uv).rgb * intensity;
	star += texture(samplerBlur8, uv).rgb * intensity * 2.0;
	star += texture(samplerBlur9, uv).rgb * intensity * 6.0;

	color.rgb += max(line, max(plus, star));
    FragColor = color;
}