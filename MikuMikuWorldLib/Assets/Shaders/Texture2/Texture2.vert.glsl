#version 400

#define LAYER_NUM	65536.0

uniform float layer;
uniform float flipY = 0.0;

uniform mat4 MVP;

layout (location = 0) in vec3 position;
layout (location = 4) in vec2 uv;

out vec2 texcoord;
 
void main()
{
    gl_Position = MVP * vec4(position.xy, (layer / LAYER_NUM), 1.0);
	float v = mix(uv.y, 1.0 - uv.y, flipY);
	texcoord = vec2(uv.x, v);
}