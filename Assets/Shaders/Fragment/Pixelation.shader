Shader "Hidden/Pixelation"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_pixelWidth("Pixel Width", Range(0, 1)) = 0
		_pixelHeight("Pixel Height", Range(0, 1)) = 0
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
			uniform float _pixelWidth;
			uniform float _pixelHeight;

			float4 frag(v2f_img i) : COLOR {
				float2 uv = i.uv;

				float dx = _pixelWidth;
				float dy = _pixelHeight;
				float2 coord = float2(dx*floor(uv.x / dx), dy*floor(uv.y / dy));

				float4 c = tex2D(_MainTex, coord);

				float4 result = c;
				return result;
			}
			ENDCG
		}
	}
}
