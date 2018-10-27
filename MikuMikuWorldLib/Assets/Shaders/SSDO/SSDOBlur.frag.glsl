#version 420

const float NORMALIZE = 1.0 / 41.0;

uniform vec2 resolutionInverse = vec2(1.0 / 1280.0, 1.0 / 720.0);
uniform vec2 nearFar = vec2(0.1, 1000.0);
uniform float strength = 1.0;

layout (binding = 0) uniform sampler2D samplerSrc;
layout (binding = 1) uniform sampler2D samplerAO;

layout (location = 0) out vec4 FragColor;

void main()
{
	vec2 coord = gl_FragCoord.xy * resolutionInverse;
	vec2 aspect = resolutionInverse;

	vec4 col = vec4(0.0);
	col += texture(samplerAO, coord);
    col += texture(samplerAO, coord + (vec2( 0.00, 0.40) * aspect) * strength);
    col += texture(samplerAO, coord + (vec2( 0.15, 0.37) * aspect) * strength);
    col += texture(samplerAO, coord + (vec2( 0.29, 0.29) * aspect) * strength);
    col += texture(samplerAO, coord + (vec2(-0.37, 0.15) * aspect) * strength);       
    col += texture(samplerAO, coord + (vec2( 0.40, 0.00) * aspect) * strength);   
    col += texture(samplerAO, coord + (vec2( 0.37,-0.15) * aspect) * strength);       
    col += texture(samplerAO, coord + (vec2( 0.29,-0.29) * aspect) * strength);       
    col += texture(samplerAO, coord + (vec2(-0.15,-0.37) * aspect) * strength);
    col += texture(samplerAO, coord + (vec2( 0.00,-0.40) * aspect) * strength); 
    col += texture(samplerAO, coord + (vec2(-0.15, 0.37) * aspect) * strength);
    col += texture(samplerAO, coord + (vec2(-0.29, 0.29) * aspect) * strength);
    col += texture(samplerAO, coord + (vec2( 0.37, 0.15) * aspect) * strength); 
    col += texture(samplerAO, coord + (vec2(-0.40, 0.00) * aspect) * strength); 
    col += texture(samplerAO, coord + (vec2(-0.37,-0.15) * aspect) * strength);       
    col += texture(samplerAO, coord + (vec2(-0.29,-0.29) * aspect) * strength);       
    col += texture(samplerAO, coord + (vec2( 0.15,-0.37) * aspect) * strength);

    col += texture(samplerAO, coord + (vec2( 0.15, 0.37) * aspect) * strength * 0.9);
    col += texture(samplerAO, coord + (vec2(-0.37, 0.15) * aspect) * strength * 0.9);           
    col += texture(samplerAO, coord + (vec2( 0.37,-0.15) * aspect) * strength * 0.9);           
    col += texture(samplerAO, coord + (vec2(-0.15,-0.37) * aspect) * strength * 0.9);
    col += texture(samplerAO, coord + (vec2(-0.15, 0.37) * aspect) * strength * 0.9);
    col += texture(samplerAO, coord + (vec2( 0.37, 0.15) * aspect) * strength * 0.9);            
    col += texture(samplerAO, coord + (vec2(-0.37,-0.15) * aspect) * strength * 0.9);   
    col += texture(samplerAO, coord + (vec2( 0.15,-0.37) * aspect) * strength * 0.9);   

    col += texture(samplerAO, coord + (vec2( 0.29, 0.29) * aspect) * strength * 0.7);
    col += texture(samplerAO, coord + (vec2( 0.40, 0.00) * aspect) * strength * 0.7);       
    col += texture(samplerAO, coord + (vec2( 0.29,-0.29) * aspect) * strength * 0.7);   
    col += texture(samplerAO, coord + (vec2( 0.00,-0.40) * aspect) * strength * 0.7);     
    col += texture(samplerAO, coord + (vec2(-0.29, 0.29) * aspect) * strength * 0.7);
    col += texture(samplerAO, coord + (vec2(-0.40, 0.00) * aspect) * strength * 0.7);     
    col += texture(samplerAO, coord + (vec2(-0.29,-0.29) * aspect) * strength * 0.7);   
    col += texture(samplerAO, coord + (vec2( 0.00, 0.40) * aspect) * strength * 0.7);

    col += texture(samplerAO, coord + (vec2( 0.29, 0.29) * aspect) * strength * 0.4);
    col += texture(samplerAO, coord + (vec2( 0.40, 0.00) * aspect) * strength * 0.4);       
    col += texture(samplerAO, coord + (vec2( 0.29,-0.29) * aspect) * strength * 0.4);   
    col += texture(samplerAO, coord + (vec2( 0.00,-0.40) * aspect) * strength * 0.4);     
    col += texture(samplerAO, coord + (vec2(-0.29, 0.29) * aspect) * strength * 0.4);
    col += texture(samplerAO, coord + (vec2(-0.40, 0.00) * aspect) * strength * 0.4);     
    col += texture(samplerAO, coord + (vec2(-0.29,-0.29) * aspect) * strength * 0.4);   
    col += texture(samplerAO, coord + (vec2( 0.00, 0.40) * aspect) * strength * 0.4);

	col *= NORMALIZE;

	FragColor = texture(samplerSrc, coord) * col;
	//FragColor = col;
}