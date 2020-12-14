Shader "Custom/Fire"
{
	// References:
	// https://twitter.com/ciaccodavide/status/964407412634472448?s=20
	// https://www.patreon.com/posts/quick-game-art-17021975
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1, 1, 1, 1)


		_NoiseTex("Noise", 2D) = "white" {}
		_GradientTex("Gradient", 2D) = "white" {}

		_Speed("Speed", Float) = 1
		_GradientThreshold("Gradient Threshold", Float) = 0.5
		_Edge("Edge Size", Float) = 1
	}
		SubShader
		{
			Tags { "Queue" = "Transparent" }
			LOD 200
			Pass{
				
				// Transparency options
				ZWrite Off // Don't write to depth buffer
				Cull Off // Draw inside faces
				Blend SrcAlpha OneMinusSrcAlpha

				CGPROGRAM
				// Use shader model 3.0 target, to get nicer looking lighting
				#pragma target 3.0
				#pragma vertex vert
				#pragma fragment frag	

				sampler2D _MainTex;
				sampler2D _NoiseTex;
				sampler2D _GradientTex;

				float4 _Color;

				float _Speed;
				float _GradientThreshold;
				float _Edge;

				struct vertInput {
					float4 vertex : POSITION;
					float3 texCoord : TEXCOORD0;
				};

				struct vertOutput {
					float4 pos : SV_POSITION;
					float3 texCoord : TEXCOORD0;
				};

				vertOutput vert(vertInput input) {
					vertOutput o;
					o.pos = UnityObjectToClipPos(input.vertex); // Get position in clip/screen space
					o.texCoord = input.texCoord;

					return o;
				}

				float4 frag(vertOutput input) : COLOR {
					// Get main texture color and add it to our color
					float4 albedo = tex2D(_MainTex, input.texCoord.xy);
					float4 color = float4(albedo.rgba * _Color.rgba);

					// sample the nosie and gradient at the texture coord
					// scroll the noise texture using time and speed
					float noise = tex2Dlod(_NoiseTex, float4(input.texCoord.x, input.texCoord.y - _Time.y*_Speed, 0, 0));
					float gradient = tex2Dlod(_GradientTex, float4(input.texCoord.xy, 0, 0));

					// use gradient and noise to determine if the pixel is visible
					if (noise < (1.0f - gradient.r) * _GradientThreshold)
						color.a = 1;
					else
						color.a = 0;

					// add gradient and noise to fire texture
					// use _Edge value to increase color brightness at the top of the flame
					color.rgb *= 1.0f + gradient.r * _Edge + gradient.r * (1.0f - noise);

					return color;
				}
			ENDCG
	}
		}
}