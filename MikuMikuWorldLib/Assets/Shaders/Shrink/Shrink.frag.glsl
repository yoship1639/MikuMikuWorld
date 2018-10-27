#version 450

uniform vec2 resolutionInverse = vec2(1.0 / 320.0, 1.0 / 180.0);
uniform vec2 offset = vec2(1.0 / 1280.0, 1.0 / 720.0);

layout (std430, binding = 6) buffer Lum {
  float data[];
} gLum;

layout (binding = 0) uniform sampler2D samplerSrc;

layout (location = 0) out vec4 FragColor;

void main()
{
	vec2 uv = gl_FragCoord.xy * resolutionInverse;

	vec4 t0 = texture(samplerSrc, uv + offset * vec2(0.0, 0.0));
	vec4 t1 = texture(samplerSrc, uv + offset * vec2(1.0, 0.0));
	vec4 t2 = texture(samplerSrc, uv + offset * vec2(2.0, 0.0));
	vec4 t3 = texture(samplerSrc, uv + offset * vec2(3.0, 0.0));
	vec4 t4 = texture(samplerSrc, uv + offset * vec2(0.0, 1.0));
	vec4 t5 = texture(samplerSrc, uv + offset * vec2(1.0, 1.0));
	vec4 t6 = texture(samplerSrc, uv + offset * vec2(2.0, 1.0));
	vec4 t7 = texture(samplerSrc, uv + offset * vec2(3.0, 1.0));
	vec4 t8 = texture(samplerSrc, uv + offset * vec2(0.0, 2.0));
	vec4 t9 = texture(samplerSrc, uv + offset * vec2(1.0, 2.0));
	vec4 ta = texture(samplerSrc, uv + offset * vec2(2.0, 2.0));
	vec4 tb = texture(samplerSrc, uv + offset * vec2(3.0, 2.0));
	vec4 tc = texture(samplerSrc, uv + offset * vec2(0.0, 3.0));
	vec4 td = texture(samplerSrc, uv + offset * vec2(1.0, 3.0));
	vec4 te = texture(samplerSrc, uv + offset * vec2(2.0, 3.0));
	vec4 tf = texture(samplerSrc, uv + offset * vec2(3.0, 3.0));

	float l_max = max(max(max(max(t0.r, t1.r), max(t2.r, t3.r)), max(max(t4.r, t5.r), max(t6.r, t7.r))),
                      max(max(max(t8.r, t9.r), max(ta.r, tb.r)), max(max(tc.r, td.r), max(te.r, tf.r))));

	FragColor.r = l_max;

	FragColor.gb = 0.0625 * (t0.gb + t1.gb + t2.gb + t3.gb
                            + t4.gb + t5.gb + t6.gb + t7.gb
                            + t8.gb + t9.gb + ta.gb + tb.gb
                            + tc.gb + td.gb + te.gb + tf.gb);
	FragColor.a = 1.0;

	gLum.data[2] = gLum.data[0];
	gLum.data[3] = gLum.data[1];
	gLum.data[0] = l_max;
	gLum.data[1] = FragColor.g;
}