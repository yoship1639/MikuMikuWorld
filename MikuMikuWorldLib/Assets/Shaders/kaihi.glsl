//
// バリアンスシャドー
//
float calcVarianceShadow()
{
	float dist = length(wCamPos - In.wPos);
	if (dist < 5)
	{
		if (In.shadowCoord3.x > 0.0 && In.shadowCoord3.x < 1.0 && In.shadowCoord3.y > 0.0 && In.shadowCoord3.y < 1.0)
		{
			vec4 depth = texture(shadowMap3, In.shadowCoord3.xy + noise2(In.shadowCoord3.z));
			float d = In.shadowCoord3.z;

			float depth_sq = depth.g * depth.g;
			float variance = max(depth.b - depth_sq, 0.0000001);
			float md = d - depth.g;
			float p = variance / (variance + (md * md));

			return clamp(p, 0.0, 1.0);
		}
	}
	else if (dist < 20)
	{
		if (In.shadowCoord2.x > 0.0 && In.shadowCoord2.x < 1.0 && In.shadowCoord2.y > 0.0 && In.shadowCoord2.y < 1.0)
		{
			vec4 depth = texture(shadowMap2, In.shadowCoord2.xy);
			float d = In.shadowCoord2.z;

			float depth_sq = depth.g * depth.g;
			float variance = max(depth.b - depth_sq, 0.0000001);
			float md = d - depth.g;
			float p = variance / (variance + (md * md));

			return clamp(p, 0.0, 1.0);
		}
	}
	else
	{
		vec4 depth = texture(shadowMap1, In.shadowCoord1.xy);
		float d = In.shadowCoord1.z;

		float depth_sq = depth.g * depth.g;
		float variance = max(depth.b - depth_sq, 0.0000001);
		float md = d - depth.g;
		float p = variance / (variance + (md * md));

		return clamp(p, 0.0, 1.0);
	}
	return 1.0;
}


//
// 通常のシャドー
//
float calcShadow()
{
	float dist = length(wCamPos - In.wPos);
	if (dist < 5)
	{
		if (In.shadowCoord3.x > 0.0 && In.shadowCoord3.x < 1.0 && In.shadowCoord3.y > 0.0 && In.shadowCoord3.y < 1.0)
		{
			float rate = step(texture(shadowMap3, In.shadowCoord3.xy).r, In.shadowCoord3.z - shadowBias3);
			return mix(1.0, shadowAtten, rate);
		}
	}
	else if (dist < 20)
	{
		if (In.shadowCoord2.x > 0.0 && In.shadowCoord2.x < 1.0 && In.shadowCoord2.y > 0.0 && In.shadowCoord2.y < 1.0)
		{
			float rate = step(texture(shadowMap2, In.shadowCoord2.xy).r, In.shadowCoord2.z - shadowBias2);
			return mix(1.0, shadowAtten, rate);
		}
	}
	else
	{
		float rate = step(texture(shadowMap1, In.shadowCoord1.xy).r, In.shadowCoord1.z - shadowBias1);
		return mix(1.0, shadowAtten, rate);
	}
	return 1.0;
}

//
// ソフトシャドー
//
vec2 poissonDisk[4] = vec2[](
  vec2( -0.94201624, -0.39906216 ),
  vec2( 0.94558609, -0.76890725 ),
  vec2( -0.094184101, -0.92938870 ),
  vec2( 0.34495938, 0.29387760 )
);

float calcSoftShadow()
{
	float dist = length(wCamPos - In.wPos);
	if (dist < 5)
	{
		if (In.shadowCoord3.x > 0.0 && In.shadowCoord3.x < 1.0 && In.shadowCoord3.y > 0.0 && In.shadowCoord3.y < 1.0)
		{
			float rate = 0.0;
			for (int i = 0; i < 4; i++)
			{
				vec2 coord = In.shadowCoord3.xy + poissonDisk[i] * resolutionInverse;
				float r = step(texture(shadowMap3, coord).r, In.shadowCoord3.z - shadowBias3);
				rate += mix(1.0, shadowAtten, r);
			}
			rate *= 0.25;
			return rate;
		}
	}
	else if (dist < 20)
	{
		if (In.shadowCoord2.x > 0.0 && In.shadowCoord2.x < 1.0 && In.shadowCoord2.y > 0.0 && In.shadowCoord2.y < 1.0)
		{
			float rate = step(texture(shadowMap2, In.shadowCoord2.xy).r, In.shadowCoord2.z - shadowBias2);
			return mix(1.0, shadowAtten, rate);
		}
	}
	else
	{
		float rate = step(texture(shadowMap1, In.shadowCoord1.xy).r, In.shadowCoord1.z - shadowBias1);
		return mix(1.0, shadowAtten, rate);
	}
	return 1.0;
}