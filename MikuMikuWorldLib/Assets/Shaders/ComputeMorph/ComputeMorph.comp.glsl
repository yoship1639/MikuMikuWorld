#version 450

layout (std430, binding = 0) writeonly buffer DstMorph {
  vec4 dst[];
};

layout (std430, binding = 1) readonly buffer Morph0 {
  vec4 pos[];
} gMorph0;

layout (std430, binding = 2) readonly buffer Morph1 {
  vec4 pos[];
} gMorph1;

layout (std430, binding = 3) readonly buffer Morph2 {
  vec4 pos[];
} gMorph2;

layout (std430, binding = 4) readonly buffer Morph3 {
  vec4 pos[];
} gMorph3;

//uniform highp ivec4 offset;
//uniform highp ivec4 size;
uniform highp vec4 weight;

layout (local_size_x = 64, local_size_y = 1, local_size_z = 1) in;

void main()
{
	highp int index = int(gl_WorkGroupSize.x * gl_NumWorkGroups.x * gl_GlobalInvocationID.y + gl_GlobalInvocationID.x);

	vec4 add = vec4(0);

	add += gMorph0.pos[index] * weight[0];
	add += gMorph1.pos[index] * weight[1];
	add += gMorph2.pos[index] * weight[2];
	add += gMorph3.pos[index] * weight[3];

	dst[index] = add;
}