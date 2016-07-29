Shader "Hidden/BadTV"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		amount("Amount", Range(0, 3)) = 0
		distortion("Distortion", Range(0, 1)) = 0
		distortion2("Distortion 2", Range(0, 1)) = 0
		speed("Speed", Range(0, 1)) = 0
		rollSpeed("Roll Speed", Range(0, 1)) = 0
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
			uniform float distortion;
			uniform float distortion2;
			uniform float speed;
			uniform float rollSpeed;

			float3 mod289(float3 x) {
				return x - floor(x * (1.0 / 289.0)) * 289.0;
			}

			float2 mod289(float2 x) {
				return x - floor(x * (1.0 / 289.0)) * 289.0;
			}

			float3 permute(float3 x) {
				return mod289(((x*34.0) + 1.0)*x);
			}

			float snoise(float2 v) {
				const float4 C = float4(0.211324865405187,  // (3.0-sqrt(3.0))/6.0
										0.366025403784439,  // 0.5*(sqrt(3.0)-1.0)
										-0.577350269189626,  // -1.0 + 2.0 * C.x
										0.024390243902439);  // 1.0 / 41.0
				float2 i = floor(v + dot(v, C.yy));
				float2 x0 = v - i + dot(i, C.xx);
				float2 i1;
				i1 = (x0.x > x0.y) ? float2(1.0, 0.0) : float2(0.0, 1.0);
				float4 x12 = x0.xyxy + C.xxzz;
				x12.xy -= i1;
				i = mod289(i);
				float3 p = permute(permute( i.y + float3(0.0, i1.y, 1.0)) + i.x + float3(0.0, i1.x, 1.0));
				float3 m = max(0.5 - float3(dot(x0, x0), dot(x12.xy, x12.xy), dot(x12.zw, x12.zw)), 0.0);
				m = m*m;
				m = m*m;
				float3 x = 2.0 * frac(p * C.www) - 1.0;
				float3 h = abs(x) - 0.5;
				float3 ox = floor(x + 0.5);
				float3 a0 = x - ox;
				m *= 1.79284291400159 - 0.85373472095314 * (a0*a0 + h*h);
				float3 g;
				g.x = a0.x * x0.x + h.x * x0.y;
				g.yz = a0.yz * x12.xz + h.yz + x12.yw;
				return 130.0 * dot(m, g);
			}

			float4 frag(v2f_img i) : COLOR{

				float2 pos = i.uv;
				float4 passThruColor = tex2D(_MainTex, pos);

				float ty = _Time * speed;
				float yt = pos.y - ty;
				float offset = snoise(float2(yt*3.0, 0.0))*0.2;
				offset = pow(offset*distortion, 3.0) / distortion;
				offset += snoise(float2(yt*50.0, 0.0))*distortion2*0.001;
				float2 normalizedCoords = float2(frac(pos.x + offset), frac(pos.y - _Time.y*rollSpeed));

				float2 finalCoords = lerp(pos, normalizedCoords, amount);

				float4 result = tex2D(_MainTex, finalCoords);
				return result;
			}
		ENDCG
		}
	}
}
