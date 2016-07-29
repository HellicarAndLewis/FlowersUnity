Shader "Hidden/RGBShift"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_amount("Amount", Range(0, 3)) = 0
		_angle("Angle", Range(0, 1)) = 0
	}
	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"
			#define M_PI 3.1415926535897932384626433832795

			uniform sampler2D _MainTex;
			uniform float amount;
			uniform float angle;

			float4 frag(v2f_img i) : COLOR{
				float2 offset = 0.3 * amount * float2(1.0, 1.0) * float2(cos(angle * amount * M_PI), sin(angle * amount * M_PI));

				float4 cr = tex2D(_MainTex, i.uv + offset);
				float4 cga = tex2D(_MainTex, i.uv);
				float4 cb = tex2D(_MainTex, i.uv - offset);

				float4 result = float4(cr.r, cga.g, cb.b, cga.a);

				return result;
			}
			ENDCG
		}
	}
}
