#version 450

uniform int index = -1;
uniform vec2 resolution = vec2(1280.0 / 16.0, 768.0 / 16.0);

layout (std430, binding = 3) buffer LightCount {
  int data[];
} gLightCount;

layout (std430, binding = 4) buffer LightIndex {
  int data[];
} gLightIndex;


layout (location = 0) out vec4 FragColor;
 
void main()
{
	vec2 indices = gl_FragCoord.xy;
	int sx = int(resolution.x);
	int sy = int(resolution.y);
	int x = int(indices.x);
	int y = int(indices.y);

	int idx = (y * sx) + x;
	int count = gLightCount.data[idx];
	if (count < 64)
	{
		gLightCount.data[idx] = count + 1;
	
		int idx2 = idx * 64 + count;
		gLightIndex.data[idx2] = index;
	} 

    FragColor = vec4(gl_FragCoord.x / resolution.x, gl_FragCoord.y / resolution.y, 0.0, 0.2);
}