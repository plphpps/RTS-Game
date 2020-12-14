Shader "Custom/Outline"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0

		_OutlineColor("Outline Color", Color) = (0, 0, 0, 1)
		_OutlineWidth("Outline Width", Range(0, 0.1)) = 0.03
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			// Draw the silhouette before the diffuse so that objects with this shader don't silhouette when behind each other.
			Pass{
				ZWrite Off
				ZTest Greater

				CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag

				half4 _OutlineColor;
				half _OutlineWidth;

				float4 vert(float4 position : POSITION, float3 normal : NORMAL) : SV_POSITION {

					position.xyz += normal * _OutlineWidth; // Draw normals away from their position by outline width.

					return UnityObjectToClipPos(position);
				}

				half4 frag() : SV_TARGET {
					return _OutlineColor;
				}

				ENDCG
			}

			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface surf Standard fullforwardshadows

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0

			sampler2D _MainTex;

			struct Input
			{
				float2 uv_MainTex;
			};

			half _Glossiness;
			half _Metallic;
			fixed4 _Color;

			// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
			// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
			// #pragma instancing_options assumeuniformscaling
			UNITY_INSTANCING_BUFFER_START(Props)
				// put more per-instance properties here
			UNITY_INSTANCING_BUFFER_END(Props)

			void surf(Input IN, inout SurfaceOutputStandard o)
			{
				// Albedo comes from a texture tinted by color
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				o.Albedo = c.rgb;
				// Metallic and smoothness come from slider variables
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Alpha = c.a;
			}
			ENDCG
		}
			FallBack "Diffuse"
}
