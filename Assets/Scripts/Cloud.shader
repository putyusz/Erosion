Shader "Custom/Cloud"
{
    Properties
    {
		testTexture("Texture", 2D) = "white"{}
		testScale("Scale", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows alpha:fade

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0      

		sampler2D testTexture;
		float testScale;

        struct Input
        {
			float3 worldPos;
        };      

		//-----------------------------------------------------------------------------
		//
		// Noise Shader Library for Unity - https://github.com/keijiro/NoiseShader
		//
		// Original work (webgl-noise) Copyright (C) 2011 Stefan Gustavson
		// Translation and modification was made by Keijiro Takahashi.
		//
		// This shader is based on the webgl-noise GLSL shader. For further details
		// of the original shader, please see the following description from the
		// original source code.
		//

		//
		// GLSL textureless classic 2D noise "cnoise",
		// with an RSL-style periodic variant "pnoise".
		// Author:  Stefan Gustavson (stefan.gustavson@liu.se)
		// Version: 2011-08-22
		//
		// Many thanks to Ian McEwan of Ashima Arts for the
		// ideas for permutation and gradient selection.
		//
		// Copyright (c) 2011 Stefan Gustavson. All rights reserved.
		// Distributed under the MIT license. See LICENSE file.
		// https://github.com/ashima/webgl-noise
		//

		float4 mod(float4 x, float4 y)
		{
			return x - y * floor(x / y);
		}

		float4 mod289(float4 x)
		{
			return x - floor(x / 289.0) * 289.0;
		}

		float4 permute(float4 x)
		{
			return mod289(((x*34.0) + 1.0)*x);
		}

		float4 taylorInvSqrt(float4 r)
		{
			return (float4)1.79284291400159 - r * 0.85373472095314;
		}

		float2 fade(float2 t) {
			return t * t*t*(t*(t*6.0 - 15.0) + 10.0);
		}

		// Classic Perlin noise
		float cnoise(float2 P)
		{
			float4 Pi = floor(P.xyxy) + float4(0.0, 0.0, 1.0, 1.0);
			float4 Pf = frac(P.xyxy) - float4(0.0, 0.0, 1.0, 1.0);
			Pi = mod289(Pi); // To avoid truncation effects in permutation
			float4 ix = Pi.xzxz;
			float4 iy = Pi.yyww;
			float4 fx = Pf.xzxz;
			float4 fy = Pf.yyww;

			float4 i = permute(permute(ix) + iy);

			float4 gx = frac(i / 41.0) * 2.0 - 1.0;
			float4 gy = abs(gx) - 0.5;
			float4 tx = floor(gx + 0.5);
			gx = gx - tx;

			float2 g00 = float2(gx.x, gy.x);
			float2 g10 = float2(gx.y, gy.y);
			float2 g01 = float2(gx.z, gy.z);
			float2 g11 = float2(gx.w, gy.w);

			float4 norm = taylorInvSqrt(float4(dot(g00, g00), dot(g01, g01), dot(g10, g10), dot(g11, g11)));
			g00 *= norm.x;
			g01 *= norm.y;
			g10 *= norm.z;
			g11 *= norm.w;

			float n00 = dot(g00, float2(fx.x, fy.x));
			float n10 = dot(g10, float2(fx.y, fy.y));
			float n01 = dot(g01, float2(fx.z, fy.z));
			float n11 = dot(g11, float2(fx.w, fy.w));

			float2 fade_xy = fade(Pf.xy);
			float2 n_x = lerp(float2(n00, n01), float2(n10, n11), fade_xy.x);
			float n_xy = lerp(n_x.x, n_x.y, fade_xy.y);
			return 2.3 * n_xy;
		}	
		//-----------------------------------------------------------------------------

		float2 rotate(float2 v, float a) {
			float r = radians(a);
			float x = cos(r) * v.x - sin(r) * v.y;
			float y = sin(r) * v.x + cos(r) * v.y;
			return float2(x, y);
		}

		float cloudNoise(float2 pos) {
			float v = cnoise(pos * 0.01f);
			v = exp(v);
			v = sin(_Time.x) + 1.9f - v;
			return max(0, v);
		}

        void surf (Input IN, inout SurfaceOutputStandard o)
        { 
			o.Albedo = fixed3(255,255,255);		

			float2 pos1 = float2(IN.worldPos.x, IN.worldPos.z);
			float2 pos2 = rotate(pos1, 45);
			pos1 += _Time.y * 20;
			pos2 += _Time.y * 20;

			float v1 = cloudNoise(pos1);
			float v2 = cloudNoise(pos2);

			o.Alpha = fixed(lerp(1, 0, v1 + v2));
        }
        ENDCG
    }
    FallBack "Diffuse"
}
