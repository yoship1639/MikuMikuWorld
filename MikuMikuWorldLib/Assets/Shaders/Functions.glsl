
#pragma optionNV(inline all)
#pragma optionNV(fastmath on)
//#pragma optionNV(fastprecision on)
//#pragma optionNV(ifcvt none)
//#pragma optionNV(strict on)
//#pragma optionNV(unroll all)

#define _PI			3.141592653589793
#define _PI2		6.283185307179586
#define _PIOVER2	1.570796326794895
#define _PIOVER3	1.047197551196597
#define _PIOVER4	0.785398163397447
#define _PIOVER6	0.523598775598298
#define _OVERPI		0.318309886183791
#define _GAMMA		0.454545454545454

float saturate(float value) { return clamp(value, 0.0, 1.0); }
vec2 saturate(vec2 value) { return clamp(value, vec2(0.0), vec2(1.0)); }
vec3 saturate(vec3 value) { return clamp(value, vec3(0.0), vec3(1.0)); }
vec4 saturate(vec4 value) { return clamp(value, vec4(0.0), vec4(1.0)); }

vec3 _rgb2hsb(in vec3 c)
{
    vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
    vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));
    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

vec3 _hsb2rgb(in vec3 c )
{
    vec3 rgb = clamp(abs(mod(c.x*6.0+vec3(0.0,4.0,2.0),6.0)-3.0)-1.0, 0.0, 1.0);
    rgb = rgb*rgb*(3.0-2.0*rgb);
    return c.z * mix(vec3(1.0), rgb, c.y);
}

// 非線形深度を線形深度に変換
// IN: depth 非線形深度
// IN: n ニアクリップ
// IN: f ファークリップ
float _linearize(float depth, float n, float f)
{
	return (2.0 * n * f) / (f + n - depth * (f - n));
}

// 線形深度を非線形深度に変換
// IN: depth 線形深度
// IN: n ニアクリップ
// IN: f ファークリップ
float _nonlinearize(float depth, float n, float f)
{
	return (f + n - (2.0 * n * f / depth)) / (f - n);
}

// 座標がクリップ領域内にあるか、ある場合は1.0 ない場合は0.0
float _clip(vec3 pos, vec3 minClip, vec3 maxClip)
{
	if (
		pos.x >= minClip.x && pos.x <= maxClip.x &&
		pos.y >= minClip.y && pos.y <= maxClip.y &&
		pos.z >= minClip.z && pos.z <= maxClip.z
	) return 1.0;
	else return 0.0;
}

vec3 _contrast(vec3 color, float con)
{
	const vec3 AvgLum = vec3(0.5);
	return max(mix(AvgLum, color, con), vec3(0));
}

vec3 _saturation(vec3 color, float sat)
{
	const vec3 LumCoeff = vec3(0.2125, 0.7154, 0.0721);
	float intensity = dot(color, LumCoeff);
	return max(mix(vec3(intensity), color, sat), vec3(0));
}

vec3 _csb(vec3 color, float con, float sat, float brt) 
{ 
    // Increase or decrease theese values to adjust r, g and b color channels seperately 
    const float AvgLumR = 0.5; 
    const float AvgLumG = 0.5; 
    const float AvgLumB = 0.5; 
    
    const vec3 LumCoeff = vec3(0.2125, 0.7154, 0.0721); 
    
    vec3 AvgLumin = vec3(AvgLumR, AvgLumG, AvgLumB); 
    vec3 brtColor = color * brt; 
    float intensityf = dot(brtColor, LumCoeff); 
    vec3 intensity = vec3(intensityf); 
    vec3 satColor = mix(intensity, brtColor, sat); 
    vec3 conColor = max(mix(AvgLumin, satColor, con), vec3(0)); 
    return conColor; 
}

// 法線マップ、法線、接線から法線マッピングを施す
// IN: normMap 法線マップ
// IN: uv UV値
// IN: normal 法線
// IN: tangent 接線
// OUT: 法線マッピングを施した法線
vec3 _normalMap(sampler2D normMap, vec2 uv, vec3 tanLightDir)
{
    vec3 normColor = texture2D(normMap, uv).rgb;
    vec3 normVec = 2.0 * normColor - 1.0;
    normVec = normalize(normVec);

	//return dot(tanLightDir, normVec);
	return normVec;
}

// 法線からスフィアマップのUV値を取得
// IN: N 法線
// OUT: vec2 スフィアマップのUV値
vec2 _sphereMap(vec3 N)
{
	return vec2(0.5 - atan(N.z, N.x) / _PI2, acos(-N.y) / _PI);
}


// 一般的なライト減衰を取得
// IN: surfPos 頂点位置
// IN: lightPos ライトの位置
// OUT: ライト減衰
float _lightAtten1(vec3 surfPos, vec3 lightPos)
{
	float dist = length(lightPos - surfPos);
	return 1.0 / (1.0 + 0.1 * dist + 0.01 * dist * dist);
}


// 一般的なライト減衰を取得
// IN: surfPos 頂点位置
// IN: lightPos ライトの位置
// IN: atten1 減衰係数1
// IN: atten2 減衰係数2
// OUT: ライト減衰
float _lightAtten1(vec3 surfPos, vec3 lightPos, float atten1, float atten2)
{
	float dist = length(lightPos - surfPos);
	return 1.0 / (1.0 + atten1 * dist + atten2 * dist * dist);
}


// 通常のライト減衰を取得
// IN: surfPos 頂点位置
// IN: lightPos ライトの位置
// IN: atten 減衰係数
// OUT: ライト減衰
float _lightAtten2(vec3 surfPos, vec3 lightPos, float atten)
{
	float dist = length(lightPos - surfPos);
    return 1.0 / (1.0 + atten * dist * dist);
}


// 物理ライト減衰を取得
// IN: surfPos 頂点位置
// IN: lightPos ライトの位置
// OUT: ライト減衰
float _physicalLightAtten1(vec3 surfPos, vec3 lightPos)
{
	float denom = length(lightPos - surfPos) + 1;
	return 1.0 / (denom * denom);
}


// 物理ライト減衰を取得
// IN: surfPos 頂点位置
// IN: lightPos ライトの位置
// IN: radius ライト半径
// OUT: ライト減衰
float _physicalLightAtten2(vec3 surfPos, vec3 lightPos, float radius)
{
	float d = max(length(lightPos - surfPos) - radius, 0.0);
	float denom = d / radius;
	return 1.0 / (denom * denom);
}


// 物理ライト減衰を取得
// IN: surfPos 頂点位置
// IN: lightPos ライトの位置
// OUT: ライト減衰
float _physicalLightAtten3(vec3 surfPos, vec3 lightPos)
{
	float d = length(lightPos - surfPos);
	return 1.0 / (d * d + 1.0);
}



// 物理ライト減衰を取得
// IN: surfPos 頂点位置
// IN: lightPos ライトの位置
// IN: radius ライト半径
// OUT: ライト減衰
float _physicalLightAtten4(float distance, float radius)
{
	float dl4 = 1.0 - pow(distance / radius, 4.0);
	float sat = saturate(dl4 * dl4);
	return sat / (distance * distance + 1);
}

// 物理ライト減衰を取得
// IN: surfPos 頂点位置
// IN: lightPos ライトの位置
// IN: radius ライト半径
// OUT: ライト減衰
float _physicalLightAtten5(float distance, float radius, float intensity)
{
	float dl4 = saturate(1.0 - pow(distance / radius, 4.0));
	float sat = dl4 * dl4;
	sat *= pow(max(intensity, 0.0), 2.0);
	return sat / (distance * distance + 1);
}

// 光度を考慮したポイントライト照度を取得
float _physicalPointLightAtten(float distance, float intensity)
{
	// 照度
	return (intensity * intensity) / max(distance * distance, 1.0);

	//float phi = i * 4 * _PI;
}



// 通常のスペキュラを取得する
// IN: surfToEye 頂点から視点への向き
// IN: normal 法線
// IN: surfToLight 頂点からライトへの向き
// IN: shininess スペキュラ係数
// OUT: スペキュラ
float _specular(vec3 surfToEye, vec3 normal, vec3 surfToLight, float shininess)
{
	vec3 refDir = normalize(reflect(-surfToLight, normal));
	float factor = dot(surfToEye, refDir);
	return pow(max(factor, 0), shininess);
}



// G: 面の幾何学的減衰係数を取得する
float _geomShadow(float NdotH, float NdotV, float NdotL, float VdotH)
{
	float NH2 = 2.0 * NdotH;
    float g1 = (NH2 * NdotV) / VdotH;
    float g2 = (NH2 * NdotL) / VdotH;
    return min(1.0, min(g1, g2));
}

// G: implicit
float _implicit(float NdotL, float NdotV)
{
	return NdotL * NdotV;
}

// G: neumann
float _neumann(float NdotL, float NdotV)
{
	return (NdotL * NdotV) / max(NdotL, NdotV);
}

// G: kelemen
float _kelemen(float NdotL, float NdotV, float VdotH)
{
	return (NdotL * NdotV) / (VdotH * VdotH);
}

// G: fixed disney
float _gFixedDisney(float roughness, float NdotV, float NdotL)
{
	float k = pow(roughness + 1, 2) * 0.125;
	float g1 = NdotV / ((1 - k) * NdotV + k);
	float g2 = NdotL / ((1 - k) * NdotL + k);
	return g1 * g2;
}

// D: ベックマン分布関数
float _beckmann(float squRoughness, float NdotH)
{
	float SquNH = NdotH * NdotH;
	float r1 = 1.0 / ( 4.0 * squRoughness * pow(NdotH, 4.0));
    float r2 = (SquNH - 1.0) / (squRoughness * SquNH);
    return r1 * exp(r2);
}

// D: Blinn-Phong分布関数
float _blinnphong(float squRoughness, float NdotH)
{
	return ((squRoughness + 2) * pow(NdotH, squRoughness)) / _PI2;
}

// D: Trowbridge-Reitz(GGX)分布関数
float _ggx(float squRoughness, float NdotH)
{
	float rou4 = squRoughness * squRoughness;
	float squNH = NdotH * NdotH;
	float m = squNH * (rou4 - 1);
	float m2 = (m + 1) * (m + 1);
	return rou4 / (m2 * _PI);
}

// F: schlickフレネル項
vec3 _schlick(vec3 f0, float VdotH)
{
	vec3 f = vec3(pow(1.0 - VdotH, 5.0));
    f *= (1.0 - f0);
    return f + f0;
}

// F: cook-torranceフレネル項
vec3 _fresnel(vec3 f0, float VdotH)
{
	vec3 c = vec3(VdotH);
	vec3 f1 = pow(f0, vec3(0.5));
	vec3 n = (vec3(1.0) + f1) / (vec3(1.0) - f1);
	vec3 g = pow(n * n + c * c - vec3(1.0), vec3(0.5));

	return vec3(0.5) * pow((g-c)/(g+c), vec3(2.0)) * (1.0 + pow( ((g + c) * c - vec3(1.0)) / ((g - c) * c + vec3(1.0)), vec3(2.0)));
}

// F: Spherical Gaussian Approximation
vec3 _sga(vec3 f0, float VdotH)
{
	return f0 + (1 - f0) * exp2((-5.55473 * VdotH - 6.98316) * VdotH);
}

// BRDFのスペキュラ (Cook-Torrance)
vec3 _specularBRDF(vec3 f0, vec3 surfToEye, vec3 normal, vec3 surfToLight, float roughness)
{
	vec3 halfVec = normalize(surfToEye + surfToLight);
	float NL = dot(normal, surfToLight);
	float NdotH = max(dot(normal, halfVec), 0.001);
	float NdotV = max(dot(normal, surfToEye), 0.001);
	float NdotL = max(dot(normal, surfToLight), 0.001);
	float VdotH = max(dot(surfToEye, halfVec), 0.001);
	float squRoughness = roughness * roughness;

	// G: 面の幾何学的減衰係数
	//float G = geomShadow(NdotH, NdotV, NdotL, VdotH);
	float G = _implicit(NdotL, NdotV);
	//float G = neumann(NdotL, NdotV);
	//float G = kelemen(NdotL, NdotV, VdotH);

	// D: マイクロファセット分布関数
	//float D = beckmann(squRoughness, NdotH);
	//float D = blinnphong(squRoughness, NdotH);
	float D = _ggx(squRoughness, NdotH);

	// F: フレネル項
	vec3 F = _schlick(f0, VdotH);

	NL = min(1.0, 1.0 + NL);
	NL = pow(NL, 8);

	return (G * D * F) / (NdotV * NdotL * 4) * NL;
}

vec3 _specularBRDF2(vec3 f0, float roughness, vec3 v, vec3 n, vec3 l)
{
	vec3 h = normalize(v + l);
	float NdotH = max(dot(n, h), 0.001);
	float NdotV = max(dot(n, v), 0.001);
	float NdotL = max(dot(n, l), 0.001);
	float VdotH = dot(v, h);
	float a = roughness * roughness;

	// D: GGX (Trowbridge-Reitz)
	float D = _ggx(a, NdotH);

	// G: GGX (Fixed Disney)
	float G = _gFixedDisney(roughness, NdotV, NdotL);

	// F: SGA
	vec3 F = _sga(f0, VdotH);

	return (D * G * F) / (NdotL * NdotV * 4);
}

vec3 _diffuseBRDF(vec3 diff)
{
	return diff * _OVERPI;
}


vec3 _importanceSampleGGX(vec2 xi, float roughness, vec3 n)
{
	float a = roughness * roughness;
	float phi = _PI2 * xi.x;
	float ct = sqrt((1 - xi.y) / (1 + (a * a - 1) * xi.y));
	float st = sqrt(1 - ct * ct);
	vec3 h;
	h.x = st * cos(phi);
	h.y = st * sin(phi);
	h.z = ct;
	//vec3 up = abs(n.z) < 0.999 ? vec3(0, 0, 1) : vec3(1, 0, 0);
	vec3 up = vec3(0, 0, 1);
	vec3 tx = normalize(cross(up, n));
	vec3 ty = cross(n, tx);
	return tx * h.x + ty * h.y + n * h.z;
}

float radicalInverse_VdC(uint bits)
{
    bits = (bits << 16u) | (bits >> 16u);
    bits = ((bits & 0x55555555u) << 1u) | ((bits & 0xAAAAAAAAu) >> 1u);
    bits = ((bits & 0x33333333u) << 2u) | ((bits & 0xCCCCCCCCu) >> 2u);
    bits = ((bits & 0x0F0F0F0Fu) << 4u) | ((bits & 0xF0F0F0F0u) >> 4u);
    bits = ((bits & 0x00FF00FFu) << 8u) | ((bits & 0xFF00FF00u) >> 8u);
    return float(bits) * 2.3283064365386963e-10;
}

vec2 hammersley2d(uint i, uint N)
{
	return vec2(float(i) / float(N), noise1(float(i)));
	//return (noise2(float(i) / float(N)) + vec2(1.0)) * 0.5;
    //return vec2(float(i) / float(N), radicalInverse_VdC(i));
}

vec3 _specularIBL(vec3 spec, float roughness, vec3 n, vec3 v, samplerCube envMap)
{
	vec3 specLight = vec3(0);
	float NdotV = max(dot(n, v), 0.0);

	for (uint i = 0; i < IBL_SPEC_SAMPLE_NUM; i++)
	{
		vec2 xi = hammersley2d(i, IBL_SPEC_SAMPLE_NUM);
		vec3 h = _importanceSampleGGX(xi, roughness, n);
		float VdotH = max(dot(v, h), 0.0);
		vec3 l = 2.0 * VdotH * h - v;
		float NdotL = max(dot(n, l), 0.0);
		float NdotH = max(dot(n, h), 0.0);
		if (NdotL > 0)
		{
			vec3 samp = texture(envMap, l).rgb;
			float G = _gFixedDisney(roughness, NdotV, NdotL);
			float Fc = pow(1 - VdotH, 5.0);
			vec3 F = (1 - Fc) * spec + Fc;
			specLight += samp * F * G * VdotH / (NdotH * NdotV);
		}
	}
	return max(specLight / IBL_SPEC_SAMPLE_NUM, 0.0);
}

vec3 _diffuseIBL(float roughness, vec3 n, vec3 v, samplerCube envMap)
{
	vec3 diffLight = vec3(0);
	float NdotV = max(dot(n, v), 0.0);
	for (uint i = 0; i < IBL_DIFF_SAMPLE_NUM; i++)
	{
		vec2 xi = hammersley2d(i, IBL_DIFF_SAMPLE_NUM);
		xi.y = xi.y * 0.5 + 0.5;
		vec3 h = _importanceSampleGGX(xi, roughness, n);
		float VdotH = max(dot(v, h), 0.0);
		vec3 l = 2.0 * VdotH * h - v;
		vec3 samp = textureLod(envMap, l, roughness * 5.0).rgb;
		diffLight += samp;
	}
	return max(diffLight / IBL_DIFF_SAMPLE_NUM, 0.0);
}


/*
vec3 PrefilterEnvMap(float roughness , vec3 r)
{
	vec3 n = r;
	vec3 v = r;
	
	vec3 PrefilteredColor = vec3(0);
	 
	const uint NumSamples = 1024;
	for(uint i = 0; i < NumSamples; i++)
	{
		vec2 xi = Hammersley(i, NumSamples);
		vec3 h = ImportanceSampleGGX(xi, roughness , n);
		vec3 l = 2 * dot(v, h) * h - v;
	   
		float NdotL = saturate(dot(n, l));
		if( NdotL > 0 )
		{
			PrefilteredColor += EnvMap.SampleLevel( EnvMapSampler , l, 0 ).rgb * NdotL;
			TotalWeight += NdotL;
		}
	}

	return PrefilteredColor / TotalWeight;
}

vec3 _approxSpecularIBL(vec3 spec, float roughness, vec3 n, vec3 v)
{
	float NdotV = saturate(dot(n, v));
	vec3 r = 2 * NdotV * n - v;
	
	vec3 PrefilteredColor = PrefilterEnvMap(roughness , r);
	vec2 EnvBRDF = IntegrateBRDF(roughness , NdotV);
	
	return PrefilteredColor * (spec * EnvBRDF.x + EnvBRDF.y);
}
*/

// roughnessから反射角を取得
vec3 GetSpecularDominantDir(vec3 normal, vec3 reflection, float roughness)
{
    float smoothness = 1.0 - roughness;
    float lerpFactor = smoothness * (sqrt(smoothness) + roughness);
    return mix(normal, reflection, lerpFactor);
}

// roughnessからmipmapレベルを取得
float GetMipFromRoughness(float roughness)
{
    float Level = 3.0 - 1.15 * log2( roughness );
    return 9.0 - Level;
}

// 環境光のBRDFの近似
vec3 ApproxEnvBRDF(vec3 color, float roughness, float NdotV)
{
	vec4 c0 = vec4(-1, -0.0275, -0.572, 0.022 );
    vec4 c1 = vec4(1, 0.0425, 1.0, -0.04 );
    vec4 r = roughness * c0 + c1;
    float a004 = min( r.x * r.x, exp2( -9.28 * NdotV ) ) * r.x + r.y;
    vec2 AB = vec2( -1.04, 1.04 ) * a004 + r.zw;
    return color * AB.x + AB.y;
}

// IBL
vec3 ImageBasedLighting(samplerCube envMap, vec3 normal, vec3 surfToEye, float roughness, float intensity)
{
    float NdotV = clamp(dot(-surfToEye, normal), 0.0, 1.0);

    float mipSelect = GetMipFromRoughness(roughness);
    //float brightness = clamp(intensity, 0.0, 1.0);
    float darknessCutoff = clamp((intensity - 1.0) * 0.1, 0.0, 0.25);

	//float rate = 1.0 - pow((1.0 - roughness), 16.0);
	float rate = roughness;
	
	vec3 reflectVec = GetSpecularDominantDir(normal, reflect(normal, -surfToEye), rate);
	//vec3 envSpe = texture(envMap, reflectVec, 1.0).rgb;
	vec3 envDif = texture(envMap, normal, mipSelect).rgb;
	return envDif;
	//vec3 envSpe = envDif;
	//vec3 env = mix(envSpe, envDif, rate);
	//vec3 color = mix(specColor, diffColor, rate);
	
	// env
	//env = pow(env + darknessCutoff, vec3(max(1.0, intensity)));
    //env += max(vec3(0.0), env - vec3(1.0)) * hdrMax;
	//color = ApproxEnvBRDF(color, roughness, NdotV);
	//color *= mix(0.2, 1.0, roughness);

	// specular
    //envSpe = pow(envSpe + darknessCutoff, vec3(max(1.0, intensity)));
    //envSpe += max(vec3(0.0), envSpe - vec3(1.0)) * hdrMax;
	//vec3 spe = ApproxEnvBRDF(specColor, roughness, NdotV);
	//spe *= mix(0.1, 1.0, roughness * roughness);
	//spe = max(spe, vec3(0));
	//vec3 spe = _specularBRDF(specColor, surfToEye, normal, reflectVec, roughness);
	//vec3 spe = specColor;
	//vec3 spe = vec3(0);

	// diffuse
    //envDif = pow(envDif + darknessCutoff, vec3(max(1.0, intensity)));
    //envDif += max(vec3(0.0), envDif - vec3(1.0)) * hdrMax;
    //vec3 dif = max(ApproxEnvBRDF(diffColor, 1.0, NdotV), vec3(0));
	//dif *= mix(0.1, 1.0, roughness);
	//vec3 dif = diffColor * _OVERPI;
	//vec3 dif = diffColor;
	//vec3 dif = vec3(0);

	
    //return (envSpe * spe + envDif * dif) * brightness;
	//return (env * color) * intensity;
}