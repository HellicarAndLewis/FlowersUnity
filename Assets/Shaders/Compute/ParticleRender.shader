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
			Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "LightMode" = "ForwardBase" }
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
				#pragma multi_compile_fog

				#pragma target 5.0
				
				#include "UnityCG.cginc"
				#include "UnityLightingCommon.cginc" // for _LightColor0
				#include "Particle.cginc"
				#include "Noise.cginc"

				float4x4 ModelViewProjection;
				StructuredBuffer<ParticleData> particles;
				StructuredBuffer<float3> quadPoints;
				sampler2D _MainTex;
				float4 texBounds;
				float4 _Color;
				int revealType;
				float scale;
				float minBright;
				int fogEnabled;
				float4 growFrom;
				float4 growTo;

				// ----------------------------------------------------------
				struct v2f
				{
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
					float4 color: COLOR;
					//Used to pass fog amount around number should be a free texcoord.
					UNITY_FOG_COORDS(1)
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
					if (revealType == 1)
					{
						if (id == 1 || id == 2 || id == 3)
						{
							float sinX = sin((1 - particles[inst].enabled) * 3);
							float cosX = cos((1 - particles[inst].enabled) * 3);
							float2x2 rotationMatrix = float2x2(cosX, -sinX, sinX, cosX);
							quadPoint.xy = mul(quadPoint.xy, rotationMatrix);
						}
						quadPoint *= particles[inst].enabled;
						quadPoint *= size;
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
						float scaledSize = size * particles[inst].enabled * scale;

						float4 growthPos = mul(ModelViewProjection, float4(worldPosition, 1.0f)) + float4(quadPoint, 0.0f);
						/*
						scaledDiff = (
							(pos.x > growFrom.x && pos.x < growTo.x) &&
							(pos.y > growFrom.y && pos.y < growTo.y) &&
							(pos.z > growFrom.z && pos.z < growTo.z)
							);
							*/
						//scaledSize *= map(growthPos.z, growFrom.z, growTo.z, 0, 1, true);
						//scaledSize *= map(growthPos.y, growFrom.y, growTo.y, 0, 1, true);

						quadPoint *= scaledSize;
						quadPoint.y += (scaledSize * 0.5);
					}

					
					// set vertex position using projection and view matrices and the quad point
					float4 pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(quadPoint, 0.0f));
					if (revealType == 3)
					{
						pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, mul(ModelViewProjection, float4(worldPosition, 1.0f)) + float4(quadPoint, 0.0f)));
					}

					o.pos = pos;


					// Rotate the texture coordinates around the z axis
					//float2 uvPoint = quadPoints[id];
					float sinX = sin(angle);
					float cosX = cos(angle);
					float2x2 rotationMatrix = float2x2(cosX, -sinX, sinX, cosX);
					float2 uvPoint = mul(quadPoints[id], rotationMatrix);

					// texture coord based on spritesheet logic (offset and size)
					o.uv = particles[inst].texOffset + ( (uvPoint + 0.5f) * float2(texBounds.xy));

					// get vertex normal in world space
					half3 worldNormal = UnityObjectToWorldNormal(float3(0,1,0));
					// dot product between normal and light direction for
					// standard diffuse (Lambert) lighting
					half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
					// factor in the light color
					float diff = nl * _LightColor0;

					float scaledDiff = map(diff.r, 0, 1, minBright, 1, true);

					o.color = float4 (particles[inst].colour.rgb * scaledDiff, 1);
					//o.color = float4 (1, 1, 1, 1);

					if (fogEnabled == 1)
					{
						//Compute fog amount from clip space position.
						UNITY_TRANSFER_FOG(o, o.pos);
					}
					

					return o;
				}

				// ----------------------------------------------------------
				float4 frag (v2f i) : COLOR
				{
					float4 texCol = tex2Dbias (_MainTex, float4(i.uv, 0.0f, -1.0f));
					float4 particleCol = i.color;
					float4 colour = float4(texCol.rgb * particleCol.rgb, texCol.a * particleCol.a);

					if (fogEnabled == 1)
					{
						//Apply fog (additive pass are automatically handled)
						UNITY_APPLY_FOG(i.fogCoord, colour);
					}

					return colour;
				}

			ENDCG
		}
	} 
	FallBack "Diffuse"
}
