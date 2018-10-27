#version 400

#define LAYER_NUM	65536.0

uniform vec4 srcRect = vec4(0, 0, 1.0, 1.0);
uniform vec4 dstRect = vec4(0, 0, 1.0, 1.0);
uniform float layer;
uniform float flipY = 0.0;

uniform mat4 MVP;

layout (location = 0) in vec3 position;
layout (location = 4) in vec2 uv;

out vec2 texcoord;

vec2 fit_tex(vec2 v, vec4 rect)
{
	v *= rect.zw;
	v += rect.xy;
	v.y += 1.0 - rect.w;
	v.y = mix(v.y, 1.0 - v.y, flipY);
	return v;
}

vec2 fit_pos(vec2 v, vec4 rect)
{
	v = (v + 1.0) * 0.5;
	v *= rect.zw;
	v.x += rect.x;
	v.y += 1.0 - (rect.y + rect.w);
	return (v * 2.0) - 1.0;
}

mat4 rotationMatrix(vec3 axis, float angle)
{
    axis = normalize(axis);
    float s = sin(angle);
    float c = cos(angle);
    float oc = 1.0 - c;
    
    return mat4(oc * axis.x * axis.x + c,           oc * axis.x * axis.y - axis.z * s,  oc * axis.z * axis.x + axis.y * s,  0.0,
                oc * axis.x * axis.y + axis.z * s,  oc * axis.y * axis.y + c,           oc * axis.y * axis.z - axis.x * s,  0.0,
                oc * axis.z * axis.x - axis.y * s,  oc * axis.y * axis.z + axis.x * s,  oc * axis.z * axis.z + c,           0.0,
                0.0,                                0.0,                                0.0,                                1.0);
}

vec2 rotateVec(vec2 v, float a) {
	float s = sin(a);
	float c = cos(a);
	mat2 m = mat2(c, -s, s, c);
	return m * v;
}
 
void main()
{
    gl_Position = MVP * vec4(fit_pos(position.xy, dstRect), (layer / LAYER_NUM), 1.0);
	texcoord = fit_tex(uv, srcRect);
}