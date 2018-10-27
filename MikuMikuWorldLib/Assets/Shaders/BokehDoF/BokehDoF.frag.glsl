#version 420

const float NORMALIZE = 1.0 / 41.0;

uniform vec2 resolutionInverse = vec2(1.0 / 1280.0, 1.0 / 720.0);
uniform float focus = 0.2;
uniform vec2 bias = vec2(1.0, 0.0);
uniform vec2 blurMax = vec2(12.0, 8.0);
uniform vec2 nearFar = vec2(0.1, 1000.0);


layout (binding = 0) uniform sampler2D samplerSrc;
layout (binding = 1) uniform sampler2D samplerDepth;

layout (location = 0) out vec4 FragColor;

float linearize(float depth, float n, float f)
{
	return (2.0 * n * f) / (f + n - depth * (f - n));
}

float nonlinearize(float depth, float n, float f)
{
	return (f + n - (2.0 * n * f / depth)) / (f - n);
}

vec4 depthsample(vec4 base, float baseDepth, sampler2D sampSrc, sampler2D sampDepth, vec2 sampCoord)
{
	float d = texture(sampDepth, sampCoord).x;
	if (baseDepth > d + 0.01) return base;
	return texture(sampSrc, sampCoord);
}



void main()
{
	vec2 aspect = vec2(1.0, resolutionInverse.y / resolutionInverse.x);
	aspect = resolutionInverse;
	vec2 coord = gl_FragCoord.xy * resolutionInverse;
	float depth = texture(samplerDepth, coord).x;
	//if (depth == 0.0)
	//{
	//	FragColor = texture(samplerSrc, coord);
	//	return;
	//}
	float factor = linearize(depth, nearFar.x, nearFar.y) - focus;
	float b = bias.y;
	if (factor < 0.0) b = bias.x;
	vec2 dofblur = vec2(clamp(factor * b, -blurMax.x, blurMax.y));
	//vec2 dofblur = vec2(clamp(max(factor * b, max(bias.x, bias.y)), -blurMax.x, blurMax.y));
	//vec2 dofblur = vec2(max(blurMax.x, blurMax.y));

	vec4 col = vec4(0.0);

	/*
	vec4 base = texture(samplerSrc, coord);
	col += texture(samplerSrc, coord);
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2( 0.00, 0.40) * aspect) * dofblur);
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2( 0.15, 0.37) * aspect) * dofblur);
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2( 0.29, 0.29) * aspect) * dofblur);
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2(-0.37, 0.15) * aspect) * dofblur);       
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2( 0.40, 0.00) * aspect) * dofblur);   
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2( 0.37,-0.15) * aspect) * dofblur);       
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2( 0.29,-0.29) * aspect) * dofblur);       
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2(-0.15,-0.37) * aspect) * dofblur);
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2( 0.00,-0.40) * aspect) * dofblur); 
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2(-0.15, 0.37) * aspect) * dofblur);
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2(-0.29, 0.29) * aspect) * dofblur);
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2( 0.37, 0.15) * aspect) * dofblur); 
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2(-0.40, 0.00) * aspect) * dofblur); 
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2(-0.37,-0.15) * aspect) * dofblur);       
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2(-0.29,-0.29) * aspect) * dofblur);       
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2( 0.15,-0.37) * aspect) * dofblur);

    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2( 0.15, 0.37) * aspect) * dofblur * 0.9);
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2(-0.37, 0.15) * aspect) * dofblur * 0.9);           
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2( 0.37,-0.15) * aspect) * dofblur * 0.9);           
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2(-0.15,-0.37) * aspect) * dofblur * 0.9);
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2(-0.15, 0.37) * aspect) * dofblur * 0.9);
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2( 0.37, 0.15) * aspect) * dofblur * 0.9);            
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2(-0.37,-0.15) * aspect) * dofblur * 0.9);   
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2( 0.15,-0.37) * aspect) * dofblur * 0.9);   

    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2( 0.29, 0.29) * aspect) * dofblur * 0.7);
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2( 0.40, 0.00) * aspect) * dofblur * 0.7);       
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2( 0.29,-0.29) * aspect) * dofblur * 0.7);   
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2( 0.00,-0.40) * aspect) * dofblur * 0.7);     
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2(-0.29, 0.29) * aspect) * dofblur * 0.7);
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2(-0.40, 0.00) * aspect) * dofblur * 0.7);     
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2(-0.29,-0.29) * aspect) * dofblur * 0.7);   
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2( 0.00, 0.40) * aspect) * dofblur * 0.7);

    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2( 0.29, 0.29) * aspect) * dofblur * 0.4);
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2( 0.40, 0.00) * aspect) * dofblur * 0.4);       
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2( 0.29,-0.29) * aspect) * dofblur * 0.4);   
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2( 0.00,-0.40) * aspect) * dofblur * 0.4);     
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2(-0.29, 0.29) * aspect) * dofblur * 0.4);
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2(-0.40, 0.00) * aspect) * dofblur * 0.4);     
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2(-0.29,-0.29) * aspect) * dofblur * 0.4);   
    col += depthsample(base, depth, samplerSrc, samplerDepth, coord + (vec2( 0.00, 0.40) * aspect) * dofblur * 0.4);
	*/
	
	
    col += texture(samplerSrc, coord);
    col += texture(samplerSrc, coord + (vec2( 0.00, 0.40) * aspect) * dofblur);
    col += texture(samplerSrc, coord + (vec2( 0.15, 0.37) * aspect) * dofblur);
    col += texture(samplerSrc, coord + (vec2( 0.29, 0.29) * aspect) * dofblur);
    col += texture(samplerSrc, coord + (vec2(-0.37, 0.15) * aspect) * dofblur);       
    col += texture(samplerSrc, coord + (vec2( 0.40, 0.00) * aspect) * dofblur);   
    col += texture(samplerSrc, coord + (vec2( 0.37,-0.15) * aspect) * dofblur);       
    col += texture(samplerSrc, coord + (vec2( 0.29,-0.29) * aspect) * dofblur);       
    col += texture(samplerSrc, coord + (vec2(-0.15,-0.37) * aspect) * dofblur);
    col += texture(samplerSrc, coord + (vec2( 0.00,-0.40) * aspect) * dofblur); 
    col += texture(samplerSrc, coord + (vec2(-0.15, 0.37) * aspect) * dofblur);
    col += texture(samplerSrc, coord + (vec2(-0.29, 0.29) * aspect) * dofblur);
    col += texture(samplerSrc, coord + (vec2( 0.37, 0.15) * aspect) * dofblur); 
    col += texture(samplerSrc, coord + (vec2(-0.40, 0.00) * aspect) * dofblur); 
    col += texture(samplerSrc, coord + (vec2(-0.37,-0.15) * aspect) * dofblur);       
    col += texture(samplerSrc, coord + (vec2(-0.29,-0.29) * aspect) * dofblur);       
    col += texture(samplerSrc, coord + (vec2( 0.15,-0.37) * aspect) * dofblur);

    col += texture(samplerSrc, coord + (vec2( 0.15, 0.37) * aspect) * dofblur * 0.9);
    col += texture(samplerSrc, coord + (vec2(-0.37, 0.15) * aspect) * dofblur * 0.9);           
    col += texture(samplerSrc, coord + (vec2( 0.37,-0.15) * aspect) * dofblur * 0.9);           
    col += texture(samplerSrc, coord + (vec2(-0.15,-0.37) * aspect) * dofblur * 0.9);
    col += texture(samplerSrc, coord + (vec2(-0.15, 0.37) * aspect) * dofblur * 0.9);
    col += texture(samplerSrc, coord + (vec2( 0.37, 0.15) * aspect) * dofblur * 0.9);            
    col += texture(samplerSrc, coord + (vec2(-0.37,-0.15) * aspect) * dofblur * 0.9);   
    col += texture(samplerSrc, coord + (vec2( 0.15,-0.37) * aspect) * dofblur * 0.9);   

    col += texture(samplerSrc, coord + (vec2( 0.29, 0.29) * aspect) * dofblur * 0.7);
    col += texture(samplerSrc, coord + (vec2( 0.40, 0.00) * aspect) * dofblur * 0.7);       
    col += texture(samplerSrc, coord + (vec2( 0.29,-0.29) * aspect) * dofblur * 0.7);   
    col += texture(samplerSrc, coord + (vec2( 0.00,-0.40) * aspect) * dofblur * 0.7);     
    col += texture(samplerSrc, coord + (vec2(-0.29, 0.29) * aspect) * dofblur * 0.7);
    col += texture(samplerSrc, coord + (vec2(-0.40, 0.00) * aspect) * dofblur * 0.7);     
    col += texture(samplerSrc, coord + (vec2(-0.29,-0.29) * aspect) * dofblur * 0.7);   
    col += texture(samplerSrc, coord + (vec2( 0.00, 0.40) * aspect) * dofblur * 0.7);

    col += texture(samplerSrc, coord + (vec2( 0.29, 0.29) * aspect) * dofblur * 0.4);
    col += texture(samplerSrc, coord + (vec2( 0.40, 0.00) * aspect) * dofblur * 0.4);       
    col += texture(samplerSrc, coord + (vec2( 0.29,-0.29) * aspect) * dofblur * 0.4);   
    col += texture(samplerSrc, coord + (vec2( 0.00,-0.40) * aspect) * dofblur * 0.4);     
    col += texture(samplerSrc, coord + (vec2(-0.29, 0.29) * aspect) * dofblur * 0.4);
    col += texture(samplerSrc, coord + (vec2(-0.40, 0.00) * aspect) * dofblur * 0.4);     
    col += texture(samplerSrc, coord + (vec2(-0.29,-0.29) * aspect) * dofblur * 0.4);   
    col += texture(samplerSrc, coord + (vec2( 0.00, 0.40) * aspect) * dofblur * 0.4);
	

	col *= NORMALIZE;
	col.a = 1.0;

	FragColor = col;
	//FragColor.rgb = vec3(depth);
}