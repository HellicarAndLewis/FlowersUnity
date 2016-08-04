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
			Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
			//Less | Greater | LEqual | GEqual | Equal | NotEqual | Always
			ZTest LEqual
			ZWrite Off
			Cull Back
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
				float4 texBounds;
				float4 _Color;
				int revealType;
				float scale;

				// ----------------------------------------------------------
				struct v2f
				{
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
					float4 color: COLOR;
				};

				float4x4 rotate(float3 r, float4 d) // r=rotations axes
				{
					float cx, cy, cz, sx, sy, sz;
					sincos(r.x, sx, cx);
					sincos(r.y, sy, cy);
					sincos(r.z, sz, cz);
					return float4x4(cy*cz, -sz, sy, d.x,
						sz, cx*cz, -sx, d.y,
						-sy, sx, cx*cy, d.z,
						0, 0, 0, d.w);
				}

				// ----------------------------------------------------------
				// vertex shader with no inputs
				// uses the system values SV_VertexID and SV_InstanceID to read from compute buffers
				v2f star_vertex(uint id : SV_VertexID, uint inst : SV_InstanceID)
				{
					v2f o;
					float3 worldPosition = particles[inst].position;
					float size = particles[inst].size;
					float3 quadPoint = quadPoints[id];
					float angle = particles[inst].angle;

					// Quad deform for blossom effect (WIP)
					// 123 : RIGHT
					if (revealType == 1 && (id == 1 || id == 2 || id == 3))
					{
						float sinX = sin((1 - particles[inst].enabled) * 3);
						float cosX = cos((1 - particles[inst].enabled) * 3);
						float2x2 rotationMatrix = float2x2(cosX, -sinX, sinX, cosX);
						quadPoint.xy = mul(quadPoint.xy, rotationMatrix);
						quadPoint *= particles[inst].enabled;
						quadPoint *= (size * scale);
					}
					else if (revealType == 2)
					{
						if (id == 0 || id == 1 || id == 5)
							quadPoint.y *= ((particles[inst].enabled * 2) - 1);
						quadPoint *= particles[inst].enabled;
						quadPoint *= size;
						quadPoint.y += (0.5 * size * particles[inst].enabled * scale);
					}
					else if (revealType == 3)
					{
						quadPoint *= size * particles[inst].enabled * ((particles[inst].angle * 0.5) + 0.8) * scale;
						//quadPoint -= 0.5;
					}

					
					// set vertex position using projection and view matrices and the quad point
					o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(quadPoint, 0.0f));

					// Rotate the texture coordinates around the z axis
					//float2 uvPoint = quadPoints[id];
					float sinX = sin(angle);
					float cosX = cos(angle);
					float2x2 rotationMatrix = float2x2(cosX, -sinX, sinX, cosX);
					float2 uvPoint = mul(quadPoints[id], rotationMatrix);

					// texture coord based on spritesheet logic (offset and size)
					o.uv = particles[inst].texOffset + ( (uvPoint + 0.5f) * float2(texBounds.xy));

					//o.color = float4 (particles[inst].colour.rgb, particles[inst].enabled);
					//o.color = float4 (1, 1, 1, particles[inst].enabled);
					//o.color = float4 (particles[inst].colour.rgb, 1);
					o.color = float4 (1, 1, 1, 1);

					return o;
				}

				// ----------------------------------------------------------
				float4 frag (v2f i) : COLOR
				{
					float4 texCol = tex2Dbias (_MainTex, float4(i.uv, 0.0f, -1.0f));
					float4 particleCol = i.color;
					return float4(texCol.rgb * particleCol.rgb, texCol.a * particleCol.a);
					//return float4 (1.0f - (1.0f - texCol.rgb) * (1.0f - particleCol.rgb), texCol.a * particleCol.a );
				}

			ENDCG
		}
	} 
	FallBack "Diffuse"
}
