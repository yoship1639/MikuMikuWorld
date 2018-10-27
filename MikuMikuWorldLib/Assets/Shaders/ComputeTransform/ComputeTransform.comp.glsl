#version 450

#define IKLINK_NUM	8

layout (std430, binding = 0) writeonly buffer DstTrans {
  mat4 mat[];
} gDstTrans;

layout (std430, binding = 1) readonly buffer Bone {
  vec4 posAndpParent[];
} gBone;

layout (std430, binding = 2) readonly buffer Trans {
  mat4 mat[];
} gTrans;

layout (std430, binding = 3) readonly buffer Rot {
  vec4 ikRot[];
};

layout (local_size_x = 16, local_size_y = 1, local_size_z = 1) in;

mat4 createRotate(in vec3 rot)
{
	float cosx = cos(rot.x);
    float cosy = cos(-rot.y);
    float cosz = cos(rot.z);
    float sinx = sin(rot.x);
    float siny = sin(-rot.y);
    float sinz = sin(rot.z);

	mat4 m = mat4(
        cosz * cosy + sinx * siny * sinz,
        sinz * cosx,
        cosz * -siny + sinz * sinx * cosy,
        0.0,
        -sinz * cosy + cosz * sinx * siny,
        cosz * cosx,
        -sinz * -siny + cosz * sinx * cosy,
        0.0,
        cosx * siny,
        -sinx,
        cosx * cosy,
        0.0,
        0.0,
        0.0,
        0.0,
        1.0
    );
	
	return m;
}

mat4 createTrans(in vec3 pos, in vec3 rot, in vec3 scale)
{
	mat4 m = createRotate(rot);

    m[0] *= scale.x;
    m[1] *= scale.y;
    m[2] *= scale.z;
    m[3] = vec4(pos, 1.0);

    return m;
}

mat4 createOffset(in uint boneIndex)
{
	mat4 m = mat4(1.0);
	m[3].xyz = -gBone.posAndpParent[boneIndex].xyz;
	return m;
}

mat4 createInvOffset(in uint boneIndex)
{
	mat4 m = mat4(1.0);
	m[3].xyz = gBone.posAndpParent[boneIndex].xyz;

	int pi = int(gBone.posAndpParent[boneIndex].w);
	if (pi >= 0)
	{
		m[3].xyz -= gBone.posAndpParent[pi].xyz;
	}

	return m;
}

void main()
{
    uint index = gl_WorkGroupSize.x * gl_NumWorkGroups.x * gl_GlobalInvocationID.y + gl_GlobalInvocationID.x;

	mat4 inv = createInvOffset(index);
	mat4 off = createOffset(index);
	mat4 res = inv * gTrans.mat[index] * off;

	int pIndex = int(gBone.posAndpParent[index].w);
	while (pIndex >= 0)
	{
		res = createInvOffset(pIndex) * gTrans.mat[pIndex] * res;
		pIndex = int(gBone.posAndpParent[pIndex].w);
	}

	gDstTrans.mat[index] = res;
}