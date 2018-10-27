#version 420

uniform vec2 resolutionInverse = vec2(1.0 / 1280.0, 1.0 / 720.0);
uniform float intensity = 1.0;
layout (binding = 0) uniform sampler2D samplerSrc;
layout (binding = 1) uniform sampler2D samplerBlur1;
layout (binding = 2) uniform sampler2D samplerBlur2;
layout (binding = 3) uniform sampler2D samplerBlur3;
layout (binding = 4) uniform sampler2D samplerBlur4;
layout (binding = 5) uniform sampler2D samplerBlur5;

layout (location = 0) out vec4 FragColor;

void main()
{
	vec2 uv = gl_FragCoord.xy * resolutionInverse;
	vec4 color = vec4(texture(samplerSrc, uv).rgb, 1.0);
	//color = vec4(vec3(0), 1.0);
	color.rgb += texture(samplerBlur1, uv).rgb;// * intensity * 0.25;
	color.rgb += texture(samplerBlur2, uv).rgb;// * intensity * 0.5;
	color.rgb += texture(samplerBlur3, uv).rgb * intensity;
	color.rgb += texture(samplerBlur4, uv).rgb * intensity * 2.0;
	color.rgb += texture(samplerBlur5, uv).rgb * intensity * 4.0;
	color.rgb = max(color.rgb, vec3(0.0));
    FragColor = color;
}