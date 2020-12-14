Shader "Custom/Water"
{
	// Resources: 
	//	https://roystan.net/articles/toon-water.html
	//	https://catlikecoding.com/unity/tutorials/flow/waves/
	Properties
	{
		// Water Depth Color Properties
		_ShallowColor("Shallow Color", Color) = (0.6, 0.75, 1, 0.725)
		_DeepColor("Deep Color", Color) = (.1, 0.45, 1, 0.75)
		_DepthMaxDistance("Depth Maximum Distance", Float) = 1

		// Wave Animation Properties
		_Speed("Wave Speed", Float) = 200
		_Amplitude("Amplitude", Float) = 1
		_WaveLength("Wave Length", Float) = 10
	}
		SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
		}
		Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"


			// Depth texture properties
			sampler2D _CameraDepthTexture;
			float4 _ShallowColor;
			float4 _DeepColor;
			float _DepthMaxDistance;

			// Wave animation properties
			float _Speed;
			float _Amplitude;
			float _WaveLength;

			struct vertInput {
				float4 vertex : POSITION;
			};

			struct vertOutput {
				float4 pos : SV_POSITION;
				float4 screenPos : TEXCOORD2;
			};

			vertOutput vert(vertInput input) {
				vertOutput o;

				// find screen space position of vertex pos (needed for depth texture which is a full screen texture)
				o.pos = UnityObjectToClipPos(input.vertex);
				o.screenPos = ComputeScreenPos(o.pos);

				// wave displacement
				float4 worldPos = mul(unity_ObjectToWorld, input.vertex);
				float k = 2 * UNITY_PI / _WaveLength;

				worldPos.y += sin(k * (worldPos.x - _Speed * _Time)) * _Amplitude; // move up and down on x
				worldPos.y += sin(k * (worldPos.z - _Speed * _Time)) * _Amplitude; // move up and down on z

				o.pos = mul(UNITY_MATRIX_VP, worldPos);

				return o;
			}

			float4 frag(vertOutput input) : SV_Target{
				// sample our depth texture
				float depth = tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(input.screenPos)).r;
				float linearDepth = LinearEyeDepth(depth);

				float depthDifference = linearDepth - input.screenPos.w;

				// lerp color using depth
				float waterDepthDifference = saturate(depthDifference / _DepthMaxDistance);
				float4 waterColor = lerp(_ShallowColor, _DeepColor, waterDepthDifference);

				return waterColor;
			}

			ENDCG
		}
	}
}
