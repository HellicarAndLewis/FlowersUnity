// This source code is based on the code demonstrated by Arthur Brusee [1]
//
// [1] "Unite 2013 - The GPU as a general processing unit DX11 in Unity", http://www.youtube.com/watch?v=I1uZnPAkInI,
// retrieved 2014-2-1, Standard YouTube Licence
//

Shader "Custom/ParticleRender" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
			_Color(" Color", Color) = (1, 0.5, 0.5, 1)
	}
	SubShader {
		Pass 
		{
			Tags { "RenderType"="Transparent"}
			ZTest On
			ZWrite Off
			Cull Off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it does not contain a surface program or both vertex and fragment programs.
#pragma exclude_renderers gles
				#pragma vertex star_vertex
				#pragma fragment frag

				#pragma target 5.0
				
				#include "UnityCG.cginc"
				#include "Particle.cginc"

				StructuredBuffer<ParticleData> particles;
				StructuredBuffer<float3> quadPoints;

				sampler2D _MainTex;

				float4 _Color;

				// ----------------------------------------------------------
				struct v2f
				{
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
					float4 color: COLOR;
				};

				// ----------------------------------------------------------
				// vertex shader with no inputs
				// uses the system values SV_VertexID and SV_InstanceID to read from compute buffers
				v2f star_vertex(uint id : SV_VertexID, uint inst : SV_InstanceID)
				{
					v2f o;

					float3 worldPosition = particles[inst].position;
					float size = particles[inst].size;
					float3 quadPoint = quadPoints[id] * size;

					o.pos = mul (UNITY_MATRIX_P, mul (UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(quadPoint, 0.0f));
					o.uv = quadPoints[id] + 0.5f;
					o.color = float4 (1, 1, 1, particles[inst].enabled);

					return o;
				}

				// ----------------------------------------------------------
				float4 frag (v2f i) : COLOR
				{
					float4 texCol = tex2Dbias (_MainTex, float4(i.uv, 0.0f, -1.0f));
					float4 particleCol = i.color;
					return float4(texCol.rgb, texCol.a * particleCol.a);
					//return float4 (1.0f - (1.0f - texCol.rgb) * (1.0f - particleCol.rgb), texCol.a * particleCol.a );
				}

			ENDCG
		}
	} 
	FallBack "Diffuse"
}
