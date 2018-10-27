#version 420

const float NORMALIZE = 1.0 / (1.0 + 2.0 * ( 0.93 + 0.8 + 0.7 + 0.6 + 0.5 + 0.4 + 0.3 + 0.2 + 0.1 ));

uniform vec2 resolutionInverse = vec2(1.0 / 1280.0, 1.0 / 720.0);
uniform float radius = 5.0;
layout (binding = 0) uniform sampler2D sampler0;

layout (location = 0) out vec4 FragColor;

vec4 gaussianblur(vec2 tex, vec2 dir, sampler2D src)
{
	vec4 color = texture(src, tex);

	color += (texture(src, tex + dir * 1.0) + texture(src, tex - dir * 1.0)) * 0.93;
	color += (texture(src, tex + dir * 2.0) + texture(src, tex - dir * 2.0)) * 0.8;
	color += (texture(src, tex + dir * 3.0) + texture(src, tex - dir * 3.0)) * 0.7;
	color += (texture(src, tex + dir * 4.0) + texture(src, tex - dir * 4.0)) * 0.6;
	color += (texture(src, tex + dir * 5.0) + texture(src, tex - dir * 5.0)) * 0.5;
	color += (texture(src, tex + dir * 6.0) + texture(src, tex - dir * 6.0)) * 0.4;
	color += (texture(src, tex + dir * 7.0) + texture(src, tex - dir * 7.0)) * 0.3;
	color += (texture(src, tex + dir * 8.0) + texture(src, tex - dir * 8.0)) * 0.2;
	color += (texture(src, tex + dir * 9.0) + texture(src, tex - dir * 9.0)) * 0.1;

	return color * NORMALIZE;
}

void main()
{
    FragColor = gaussianblur(gl_FragCoord.xy * resolutionInverse, vec2(radius * resolutionInverse.x / 9.0, 0.0), sampler0);
}