#version 450

const vec3 RGB2Y   = vec3( 0.299,  0.587,  0.114);
const vec3 RGB2Cb  = vec3(-0.168, -0.331,  0.500);
const vec3 RGB2Cr  = vec3( 0.500, -0.418, -0.081);
const vec3 YCbCr2R = vec3( 1.000,  0.000,  1.402);
const vec3 YCbCr2G = vec3( 1.000, -0.344, -0.714);
const vec3 YCbCr2B = vec3( 1.000,  1.772,  0.000);

uniform vec2 resolutionInverse = vec2(1.0 / 1280.0, 1.0 / 720.0);
uniform float intensity = 0.5;
uniform float rate;

layout (std430, binding = 6) buffer Lum {
  float data[];
} gLum;

layout (binding = 0) uniform sampler2D samplerSrc;
layout (binding = 1) uniform sampler2D samplerInfo;

layout (location = 0) out vec4 FragColor;

vec3 saturation(vec3 color, float sat)
{
	const vec3 LumCoeff = vec3(0.2125, 0.7154, 0.0721);
	float intensity = dot(color, LumCoeff);
	return max(mix(vec3(intensity), color, sat), vec3(0));
}

void main()
{
	vec2 uv = gl_FragCoord.xy * resolutionInverse;

	vec4 info = texture(samplerInfo, vec2(0.5, 0.5));
	info = max(info, vec4(0.0));
	vec4 texel = texture(samplerSrc, uv);

	info.r = mix(gLum.data[2], gLum.data[0], clamp(rate, 0.0, 1.0));
	info.g = mix(gLum.data[3], gLum.data[1], clamp(rate, 0.0, 1.0));

	float coeff = 0.5 * exp(-info.g);
    float l_max = coeff * info.r;

	// YCbCr系に変換
	vec3 YCbCr;
	YCbCr.y = dot(RGB2Cb, texel.rgb);
	YCbCr.z = dot(RGB2Cr, texel.rgb);
	
	// 色の強さは補正
	float lum = coeff * dot(RGB2Y, texel.rgb);
	YCbCr.x = lum * (1.0 + lum / (l_max * l_max)) / (1.0 + lum);
	
	// RGB系にして出力
	vec4 color;
	color.r = dot(YCbCr2R, YCbCr);
	color.g = dot(YCbCr2G, YCbCr);
	color.b = dot(YCbCr2B, YCbCr);
	color.a = 1.0;

	float rate = min(max(info.r - info.g - 0.5, 0.0), 1.0);
	color.rgb = mix(texel.rgb, color.rgb, rate);
	color.rgb = saturation(color.rgb, max(1.15 - pow(rate, 3.0), 0.7));

	FragColor.rgb = mix(texel.rgb, color.rgb, vec3(intensity));
	FragColor.a = 1.0;
}