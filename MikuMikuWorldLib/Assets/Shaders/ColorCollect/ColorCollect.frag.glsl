#version 420

require(functions)

uniform float hue = 0.0f;
uniform float brightness = 1.0f;
uniform float saturation = 1.0f;
uniform float contrast = 1.0f;
uniform vec2 resolutionInverse = vec2(1.0 / 1280.0, 1.0 / 720);

layout (binding = 0) uniform sampler2D sampler0;

layout (location = 0) out vec4 FragColor;

vec3 csb(vec3 color) 
{ 
    // Increase or decrease theese values to adjust r, g and b color channels seperately 
    const float AvgLumR = 0.5; 
    const float AvgLumG = 0.5; 
    const float AvgLumB = 0.5; 
    
    const vec3 LumCoeff = vec3(0.2125, 0.7154, 0.0721);

	vec3 col = _rgb2hsb(color);
	col.r += hue;
	color = _hsb2rgb(col);

    vec3 AvgLumin = vec3(AvgLumR, AvgLumG, AvgLumB); 
    vec3 brtColor = color * brightness; 
    float intensityf = dot(brtColor, LumCoeff); 
    vec3 intensity = vec3(intensityf); 
    vec3 satColor = mix(intensity, brtColor, saturation); 
    vec3 conColor = max(mix(AvgLumin, satColor, contrast), vec3(0)); 
    return conColor; 
}

void main()
{
    FragColor.rgb = csb(texture2D(sampler0, gl_FragCoord.xy * resolutionInverse).rgb);
	FragColor.a = 1.0;
}