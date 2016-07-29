Shader "Hidden/ScanLines"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		nIntensity("N Intensity", Range(0, 3)) = 0
		sIntensity("S Intensity", Range(0, 1)) = 0
		sCount("S Count", Range(0, 1)) = 0
		amount("Amount", Range(0, 1)) = 0
	}
	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform float amount;
			uniform float nIntensity;
			uniform float sIntensity;
			uniform float sCount;

			float4 frag(v2f_img i) : COLOR{
				float4 cTextureScreen = tex2D(_MainTex, i.uv);

				float2 coord = i.uv;

				float3 cResult = cTextureScreen.rgb;

				float2 sc = float2(sin(coord.y * sCount * amount), cos(coord.y * sCount * amount));
				cResult += cTextureScreen.rgb * float3(sc.x, sc.y, sc.x) * sIntensity * amount;
				cResult = cTextureScreen.rgb + clamp(nIntensity * amount, 0.0, 1.0) * (cResult - cTextureScreen.rgb);

				float4 result = float4(cResult.r, cResult.g, cResult.b, cTextureScreen.a);

				return result;
			}
			ENDCG
		}
	}
}
