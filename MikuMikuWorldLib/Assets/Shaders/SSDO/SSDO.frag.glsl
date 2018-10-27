#version 420

uniform vec2 resolution = vec2(1280.0, 720.0);
uniform vec2 resolutionInverse = vec2(1.0 / 1280.0, 1.0 / 720.0);
uniform vec2 nearFar = vec2(0.1, 1000.0);
uniform float mxlength = 100.0;
uniform float radius = 1.0;
uniform float raylength = 1.0;
uniform float aoscatter = 1.0;
uniform float cdm = 1.0;
uniform float strength = 1.0;
uniform float coeff = 0.85;
uniform mat4 V;

layout (binding = 0) uniform sampler2D samplerSrc;
layout (binding = 1) uniform sampler2D samplerDepth;
layout (binding = 2) uniform sampler2D samplerPosition;
layout (binding = 3) uniform sampler2D samplerNormal;
layout (binding = 6) uniform sampler2D samplerShadow;

layout (location = 0) out vec4 FragColor;
//layout (location = 6) out vec4 FragShadow;

float linearize(float depth)
{
	float n = nearFar.x;
	float f = nearFar.y;
	return (2.0 * n * f) / (f + n - depth * (f - n));
}

vec3 coord2position( in vec2 coord, in float z )
{
	vec3 pos;
	vec2 hbuffersize = resolution * 0.5;
	pos.z = z;
	pos.x = ((((coord.x+0.5)/hbuffersize.x)-0.5) * 2.0)*(pos.z / radius);
	pos.y = ((((-coord.y+0.5)/hbuffersize.y)+0.5) * 2.0 / (hbuffersize.x/hbuffersize.y))*(-pos.z / radius);
	return pos;
}

vec2 screen2coord(in vec3 pos)
{
	vec2 coord;
	vec2 hbuffersize = resolution * 0.5;
	pos.x /= (pos.z / radius);
	coord.x = (pos.x / 2.0 + 0.5);

	pos.y /= (-pos.z / radius) / (hbuffersize.x / hbuffersize.y);
	coord.y = -(pos.y / 2.0 - 0.5);

	return coord;
}

float rand(vec2 co)
{
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}

float randsum(in mat4 m)
{
	return m[0][0] + m[0][1] + m[0][2]
	     + m[1][0] + m[1][1] + m[1][2]
		 + m[2][0] + m[2][1] + m[2][2]
		 + m[3][0] + m[3][1] + m[3][2];
}

float readZ(in vec2 texcoord)
{
	return linearize( texture( samplerDepth, texcoord + 0.25 / resolution ).x );
}

mat3 vec3tomat3(in vec3 z)
{
	mat3 mat;
	mat[2] = z;
	vec3 v = vec3(z.z, z.x, -z.y);
	mat[0] = cross(z, v);
	mat[1] = cross(mat[0], z);
	return mat;
}

float compareDepths( in float depth1, in float depth2, in float m )
{
	float mindiff = 0.05 * m;
	float middiff = 0.25 * m;
	float maxdiff = 1.30 * m;
	float enddiff = 1.50 * m;

	float diff = (depth1-depth2);
	if (diff < mindiff) {
		return 1.0;
	}
	if (diff < middiff) {
		diff -= mindiff;
		return 1.0-diff/(middiff-mindiff);
	}
	if (diff < maxdiff) {
		return 0.0;
	}
	if (diff < enddiff) {
		diff -= maxdiff;
		return diff/(enddiff-maxdiff);
	}
	return 1.0;	
}

void main()
{
	vec2 texcoord = gl_FragCoord.xy * resolutionInverse;
	vec4 color = texture(samplerSrc, texcoord);
	float depth = texture(samplerDepth, texcoord).x;

	if (depth > 0.0)
	{
		float lineardepth = linearize(depth);
		vec3 normal = texture(samplerNormal, texcoord).rgb * 2.0 - 1.0;
		normal = mat3(transpose(inverse(V))) * normal;
		normal = normalize(normal);
		normal.z = -normal.z;

		vec3 p0 = coord2position(vec2(0.0, 0.5), lineardepth);
		vec3 p1 = coord2position(vec2(1.0, 0.5), lineardepth);
		float dist = abs(p1.x - p0.x);

		vec3 screencoord = vec3((((gl_FragCoord.x+0.5)/resolution.x)-0.5) * 2.0, (((-gl_FragCoord.y+0.5)/resolution.y)+0.5) * 2.0 / (resolution.x/resolution.y), lineardepth);
		screencoord.x *= screencoord.z / radius;
		screencoord.y *= -screencoord.z / radius;
		float rs = randsum(V);

		#define raycasts 4
		#define raysegments 6

		mat3 mat = vec3tomat3(normal);
		float a = rand(rs + texcoord);

		float ao = 0.0;
		vec3 col = vec3(0.0);
		float mx = 1.0 - (clamp(lineardepth - mxlength, 0.0, mxlength) / mxlength);

		if (mx > 0.0)
		{
			for (int i = 0; i < raycasts; i++)
			{
				for (int m = 0; m < raysegments; m++)
				{
					vec3 offset;
					offset.x = cos(a + float(i) / float(raycasts) * 3.14 * 4.0) * aoscatter;
					offset.y = sin(a + float(i) / float(raycasts) * 3.14 * 4.0) * aoscatter;
					offset.z = 1.0;
					offset = normalize(offset);

					vec3 raynormal = mat * offset;

					vec3 newpoint = screencoord + raynormal * (raylength / raysegments) * float(m + 1);
					float wheredepthshouldbe = newpoint.z;

					vec2 coord = screen2coord(newpoint);
					float newdepth = readZ(coord);

					float cd = compareDepths(wheredepthshouldbe, newdepth, cdm);
					ao += cd;

					col += texture(samplerSrc, coord).rgb;
				}
			}
		}

		ao /= (raycasts * raysegments);
		ao = ao * mx + (1.0 - mx);
		ao = 1.0 - ao;
		ao *= strength;
		ao = 1.0 - ao;

		//FragColor = vec4(vec3(ao), 1.0);

		col /= (raycasts * raysegments);
		col *= 1.0 - ao;

		//col = vec3(1.0) - col;

		vec4 res = vec4(vec3(ao) + col, 1.0);
		FragColor = mix(vec4(vec3(ao), 1.0), res, 0.5);

		//FragColor = vec4(vec3(ao), 1.0);
	}
	else
	{
		FragColor = vec4(1.0);
	}
}