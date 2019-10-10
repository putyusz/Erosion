Shader "Custom/Terrain" {
	Properties{
		testTexture("Texture", 2D) = "white"{}
		testScale("Scale", Float) = 1
	}
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		const static int maxLayerCount = 4;
		const static float epsilon = 1E-4;

		int layerCount;
		float3 baseColors[maxLayerCount];
		float baseStartHeights[maxLayerCount];
		float baseBlends[maxLayerCount];
		float baseColorStrengths[maxLayerCount];
		float baseTextureScales[maxLayerCount];

		float minHeight;
		float maxHeight;

		sampler2D testTexture;
		float testScale;

		UNITY_DECLARE_TEX2DARRAY(baseTextures);

		struct Input {
			float3 worldPos;
			float3 worldNormal;
		};
		
		float inverseLerp(float a, float b, float value) {
			return saturate((value - a) / (b - a));
		}

		float3 triplanar(float3 worldPos, float scale, float3 blendAxes, int textureIndex) {
			float3 scaledWorldPos = worldPos / scale;
			float3 xProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.y, scaledWorldPos.z, textureIndex)) * blendAxes.x;
			float3 yProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.x, scaledWorldPos.z, textureIndex)) * blendAxes.y;
			float3 zProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.x, scaledWorldPos.y, textureIndex)) * blendAxes.z;
			return xProjection + yProjection + zProjection;
		}
		
		float calculateGradient(float3 wn) {
			wn = normalize(wn);
			float3 up = float3(0, 1, 0);
			float a = up.x * wn.x + up.y * wn.y + up.z * wn.z;
			float b = sqrt(pow(up.x, 2) + pow(up.y, 2) + pow(up.z, 2));
			float c = sqrt(pow(wn.x, 2) + pow(wn.y, 2) + pow(wn.z, 2));
			float grad = abs(acos(a / (b * c)));	
			return saturate(grad);
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			float heightPercent = inverseLerp(minHeight, maxHeight, IN.worldPos.y);
			float3 blendAxes = abs(IN.worldNormal);
			blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;		
			for (int i = 0; i < layerCount - 1; i++)
			{
				float stoneStrength = calculateGradient(IN.worldNormal) * inverseLerp(-baseBlends[maxLayerCount - 1] / 2 - epsilon, baseBlends[maxLayerCount - 1] / 2, heightPercent - baseStartHeights[maxLayerCount - 1]);
				float drawStrength = inverseLerp(-baseBlends[i] / 2 - epsilon, baseBlends[i] / 2, heightPercent - baseStartHeights[i]);

				float3 baseColor = baseColors[i] * baseColorStrengths[i] + baseColors[maxLayerCount - 1] * baseColorStrengths[maxLayerCount - 1] * stoneStrength;
				float3 textureColor = triplanar(IN.worldPos, baseTextureScales[i], blendAxes, i) * (1 - baseColorStrengths[i]) * (1 - stoneStrength) + triplanar(IN.worldPos, baseTextureScales[maxLayerCount - 1], blendAxes, maxLayerCount - 1) * (1 - baseColorStrengths[maxLayerCount - 1]) * stoneStrength;

				o.Albedo = o.Albedo * (1 - drawStrength) + (baseColor + textureColor) * drawStrength;
			}			
		}
		ENDCG
	}
	FallBack "Diffuse"
}
