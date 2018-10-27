#version 420

uniform vec2 nearFar = vec2(0.1, 1000.0);

in vec4 pos;
layout( location = 0 ) out vec4 FragColor;
 
float _linearize(float depth, float n, float f)
{
	return (2.0 * n) / (f + n - depth * (f - n));
}

void main()
{
	//float depth = _linearize(gl_FragCoord.z, nearFar.x, nearFar.y);

	float depth = pos.z / pos.w;
	depth = depth * 0.5 + 0.5;
	float depth2 = depth * depth;
	float dx = dFdx(depth);
	float dy = dFdy(depth);
	depth2 += 0.25 * (dx * dx + dy * dy);

    FragColor.rgb = vec3(gl_FragCoord.z, depth, depth2);
	FragColor.a = 1.0;
}