Shader "Custom/TerrainSurfaceShader" {
	Properties
	{
		_Blend("Blend", Range(0, 1)) = 0.0
		_Color("Color", Color) = (1,1,1,1)

		_MainTex("Albedo 1", 2D) = "white" {}
		_BumpMap("Normal Map 1", 2D) = "bump" {}
		_OcclusionTex("Occlusion 1", 2D) = "white" {}

		_MainTex2("Albedo 2", 2D) = "white" {}
		_BumpMap2("Normal Map 2", 2D) = "bump" {}
		_OcclusionTex2("Occlusion 2", 2D) = "white" {}

		_Glossiness("Smoothness", Range(0, 1)) = 0.5
		_Metallic("Metallic", Range(0, 1)) = 0.0
		_OcclusionIntentsity("Occlusion intensity", Range(0,1)) = 1.0

	}

		SubShader
	{

		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex, _MainTex2, _BumpMap, _BumpMap2, _OcclusionTex, _OcclusionTex2;
		struct Input {
			float2 uv_MainTex;
			float2 uv_MainTex2;
			float2 uv_BumpMap;
			float2 uv_BumpMap2;
			float2 uv_OcclusionTex;
			float2 uv_OcclusionTex2;
		};
		half _Blend, _Glossiness, _Metallic, _OcclusionIntentsity;
		fixed4 _Color;

		void surf(Input IN, inout SurfaceOutputStandard o) {

			// Albedo map with colour tint
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color * (1 - _Blend);
			c += tex2D(_MainTex2, IN.uv_MainTex2) * _Color * (_Blend);
			o.Albedo = c.rgb;

			// Normal map
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap)) * (1 - _Blend);
			o.Normal += UnpackNormal(tex2D(_BumpMap2, IN.uv_BumpMap2)) * (_Blend);

			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;

			// ambient occulsion map
			o.Occlusion = lerp(1.0, tex2D(_OcclusionTex, IN.uv_OcclusionTex), _OcclusionIntentsity) * (1 - _Blend);
			o.Occlusion += lerp(1.0, tex2D(_OcclusionTex2, IN.uv_OcclusionTex2), _OcclusionIntentsity) * (_Blend);
		}

		ENDCG
	}

	FallBack "Diffuse"

}