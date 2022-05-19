Shader "Custom/Terrain"
{
	Properties
	{
		_RockInnerShallow ("Rock Inner Shallow", Color) = (1,1,1,1)
		_RockInnerDeep ("Rock Inner Deep", Color) = (1,1,1,1)
		_RockLight ("Rock Light", Color) = (1,1,1,1)
		_RockDark ("Rock Dark", Color) = (1,1,1,1)
		_GrassLight ("Grass Light", Color) = (1,1,1,1)
		_GrassDark ("Grass Dark", Color) = (1,1,1,1)


		//_Color ("Test Color", Color) = (1,1,1,1)
		_BiomeTexture ("Biome Texture", 2D) = "white" {}
		_testColor ("Test Color", Color) = (1,1,1,1)
		_snowLight ("SnowLight Color", Color) = (1,1,1,1)
		_snowDark ("SnowDark Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Test ("Test", Float) = 0.0

		_NoiseTex("Noise Texture", 2D) = "White" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0


		sampler2D _MainTex;
		sampler3D DensityTex;
		sampler2D _NoiseTex;

		struct Input
		{
			float2 uv_MainTex;
			float3 worldNormal;
			float3 worldPos;
		};

		half _Glossiness;
		fixed4 _Color;
		float4 _RockInnerShallow;
		float4 _RockInnerDeep;
		float4 _RockLight;
		float4 _RockDark;
		float4 _GrassLight;
		float4 _GrassDark;
		float _Test;
		float planetBoundsSize;

		float oceanRadius;

		float4 triplanarOffset(float3 vertPos, float3 normal, float3 scale, sampler2D tex, float2 offset) {
			float3 scaledPos = vertPos / scale;
			float4 colX = tex2D (tex, scaledPos.zy + offset);
			float4 colY = tex2D(tex, scaledPos.xz + offset);
			float4 colZ = tex2D (tex,scaledPos.xy + offset);
			
			// Square normal to make all values positive + increase blend sharpness
			float3 blendWeight = normal * normal;
			// Divide blend weight by the sum of its components. This will make x + y + z = 1
			blendWeight /= dot(blendWeight, 1);
			return colX * blendWeight.x + colY * blendWeight.y + colZ * blendWeight.z;
		}

		float3 worldToTexPos(float3 worldPos) {
			return worldPos / planetBoundsSize + 0.5;
		}

		float biomeZone;

		sampler3D _biomeNoiseTex;
		sampler3D _warmthNoiseTex;
		sampler3D _rainfallNoiseTex;

		float4 _testColor;
		float4 _snowLight;
		float4 _snowDark;


		int _biomeAmount;
		sampler3D _biomeNoiseTexArray[10];
		float _graphLocX[10];//Humiddity
		float _graphLocY[10];//Warmth

		float _minWarmth[10];
		float _maxWarmth[10];
		float _minHumiddity[10];
		float _maxHumiddity[10];

		float _biomeGraphSize[10];
		float4 _biomeGrassLight[10];
		float4 _biomeGrassDark[10];


		float4 grassRef[10];
		float4 grassCol;

		float4 grassColor(float4 _GrassLight, float4 _GrassDark, float4 noise) {

			float4 Out;
			
			return Out = lerp(_GrassLight, _GrassDark, noise);
		}

		float4 inverseLerp(float4 A, float4 B, float4 T)
		{
			float4 Out = (T - A) / (B - A);

			return Out;
		}


		//CaveColor
		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			float3 t = worldToTexPos(IN.worldPos);
			float density = tex3D(DensityTex, t);
			// 0 = flat, 0.5 = vertical, 1 = flat (but upside down)
			float steepness = 1 - (dot(normalize(IN.worldPos), IN.worldNormal) * 0.5 + 0.5);
			float dstFromCentre = length(IN.worldPos);

			float4 noise = triplanarOffset(IN.worldPos, IN.worldNormal, 30, _NoiseTex, 0);
			float4 noise2 = triplanarOffset(IN.worldPos, IN.worldNormal, 50, _NoiseTex, 0);
			//float angle01 = dot(normalize(IN.worldPos), IN.worldNormal) * 0.5 + 0.5;
			//o.Albedo = lerp(float3(1,0,0), float3(0,1,0), smoothstep(0.4,0.6,angle01));

			float metallic = 0;
			float rockMetalStrength = 0.4;
			

			float biomeDensity = tex3D(_biomeNoiseTex, t);


			float threshold = 0.005;

			/*
			if (biomeDensity < threshold) {
				float4 grassCol = lerp(_testColor, _testColor, noise.r);
				int r = 100;
				//noise for Rock
				float4 rockCol = lerp(_RockLight, _RockDark, (int)(noise2.r * r) / float(r));
				float n = (noise.r - 0.4) * _Test;

				float rockWeight = smoothstep(0.24 + n, 0.24 + 0.001 + n, steepness);
				o.Albedo = lerp(grassCol, rockCol, rockWeight);
				//o.Albedo = steepness > _Test;
				metallic = lerp(0, rockMetalStrength, rockWeight);
			}
			*/


			float warmthDensity = tex3D(_warmthNoiseTex, t);
			float humiddityDensity = tex3D(_rainfallNoiseTex, t);

			//if (warmthDensity == threshold && rainfallDensity == threshold) {
			float4 grassCol = lerp(_GrassLight, _GrassDark, noise.r);

			//}
			//else
			if (density < -threshold) {
				float rockDepthT = saturate(abs(density + threshold) * 20);
				o.Albedo = lerp(_RockInnerShallow, _RockInnerDeep, rockDepthT);
				metallic = lerp(rockMetalStrength, 1, rockDepthT);
			}
			else
			if (dstFromCentre > 250) {
				grassCol = lerp(_snowLight, _snowDark, noise.r);
				int r = 10;
				//noise for Rock
				float4 rockCol = lerp(_RockLight, _RockDark, (int)(noise2.r * r) / float(r));
				float n = (noise.r - 0.4) * _Test;

				float rockWeight = smoothstep(0.24 + n, 0.24 + 0.001 + n, steepness);
				o.Albedo = lerp(grassCol, rockCol, rockWeight);
				//o.Albedo = steepness > _Test;
				metallic = lerp(0, rockMetalStrength, rockWeight);
			}
			else {
				/*if (biomeZone > 0) {
					float4 grassCol = lerp(_testColor, _GrassDark, noise.r);
				}
				*/
				//float4 biomeNoise = triplanarOffset(IN.worldPos, IN.worldNormal, 60, _biomeNoiseTex, 0);
				//float4 biomeGrassCol = lerp(_testColor, _GrassDark, noise.r);
				//float warmthWeight = inverseLerp(_minWarmth[0], _maxWarmth[0], 0.5);
				//float humiddityWeight = inverseLerp(_minHumiddity[0], _maxHumiddity[0], 0.5);


				float warmthWeight = inverseLerp(_graphLocY[0], 0, 0.5);
				float humiddityWeight = inverseLerp(_minHumiddity[0], _maxHumiddity[0], _graphLocX[0]);

				//grassCol = lerp(lerp(_biomeGrassLight[0], _biomeGrassDark[0], noise.r), lerp(_GrassLight, _GrassDark, noise.r), (warmthWeight - humiddityWeight) / 2);
				
				//if (warmthDensity > _minWarmth[0] && warmthDensity < _maxWarmth[0] && humiddityDensity > _minHumiddity[0] && humiddityDensity < _maxHumiddity[0]) {
				//if (warmthDensity >= _minWarmth[0] && warmthDensity <= _maxWarmth[0] && humiddityDensity >= _minHumiddity[0] && humiddityDensity <= _maxHumiddity[0]) {
				if (warmthDensity < _graphLocY[0] && humiddityDensity < _graphLocX[0])
					{
					//biomeDensityArray[i] = tex3D(_biomeNoiseTexArray[i], t);

					//float weight = inverseLerp(_minWarmth[0], _maxWarmth[0], _graphLocY[0]);



					
					grassCol = lerp(lerp(_biomeGrassLight[0], _biomeGrassDark[0], noise.r), lerp(_GrassLight, _GrassDark, noise.r), clamp(lerp(_graphLocY[0], warmthDensity, 0.5),1,0));
					//grassCol = lerp(lerp(_biomeGrassLight[0], _biomeGrassDark[0], noise.r), lerp(_GrassLight, _GrassDark, noise.r), 100);
					//grassCol = lerp(_biomeGrassLight[0], _biomeGrassDark[0], noise.r);
					int r = 100;
					//noise for Rock
					float4 rockCol = lerp(_RockLight, _RockDark, (int)(noise2.r * r) / float(r));
					float n = (noise.r - 0.4) * _Test;

					float rockWeight = smoothstep(0.24 + n, 0.24 + 0.001 + n, steepness);
					o.Albedo = lerp(grassCol, rockCol, rockWeight);
					//o.Albedo = steepness > _Test;
					metallic = lerp(0, rockMetalStrength, rockWeight);
				}
				
				else if (warmthDensity < _graphLocY[1] && humiddityDensity < _graphLocX[1])
				{
					grassCol = lerp(_biomeGrassLight[1], _biomeGrassDark[1], noise.r);
					//grassRef[1] = lerp(_biomeGrassLight[1], _biomeGrassDark[1], noise.r);
					int r = 100;
					//noise for Rock
					float4 rockCol = lerp(_RockLight, _RockDark, (int)(noise2.r * r) / float(r));
					float n = (noise.r - 0.4) * _Test;

					float rockWeight = smoothstep(0.24 + n, 0.24 + 0.001 + n, steepness);
					o.Albedo = lerp(grassCol, rockCol, rockWeight);
					//o.Albedo = steepness > _Test;
					metallic = lerp(0, rockMetalStrength, rockWeight);
				}
				
				
				






				//float4 grassCol2 = lerp(_biomeGrassLight[0], _biomeGrassDark[0], noise.r) * warmthDensity;
					


						//grassRef[2] = lerp(_GrassLight, _GrassDark, noise.r);
					
						/*
					for (int i = 0; i < length; i++)
					{
						
					}
					*/
						
						int r = 10;
						//noise for Rock
						float4 rockCol = lerp(_RockLight, _RockDark, (int)(noise2.r * r) / float(r));
						float n = (noise.r - 0.4) * _Test;

						float rockWeight = smoothstep(0.24 + n, 0.24 + 0.001 + n, steepness);
						o.Albedo = lerp(grassCol, rockCol, rockWeight);
						//o.Albedo = lerp(lerp(grassCol, biomeGrassCol, biomeNoise * 10), rockCol, rockWeight);
						//o.Albedo = steepness > _Test;
						metallic = lerp(0, rockMetalStrength, rockWeight);
					
				
				//grassCol = InverseLerp(-0.5, 0.5, 0.5);


						/*
						Maak een lerp via de 3dnoises en add ze met elkaar (A + B)
						*/

			}





			//o.Albedo = dstFromCentre > oceanRadius;

			
			//o.Albedo = metallic;

			o.Metallic = metallic;
			o.Smoothness = _Glossiness;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
