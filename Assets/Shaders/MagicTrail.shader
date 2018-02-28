Shader "Custom/MagicTrail" {
	Properties{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Color", Color) = (0, 1, 0, 1)
	}
	SubShader {
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent-5" }
		Blend One OneMinusSrcAlpha // Premultiplied transparency
		AlphaToMask On

		Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog

			#include "UnityCG.cginc"
			#include "utils.hlsl"

			struct appdata {
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
				float4 vertex : SV_POSITION;
				UNITY_FOG_COORDS(5)
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			uniform float4 _Color;

			v2f vert(appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				// Fog
				UNITY_TRANSFER_FOG(o, o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target{
				float seed = _Time * 75.3f;
				float3 samplePos = i.worldPos;
				float n = noise(samplePos * 13.1f + seed);
				n = noise(samplePos*n);

				fixed3 black = { 0, 0, 0 };

				fixed4 col = { 1, 1, 1, 1 };
				float cutoff = ceil((0.5 - (length(i.uv - 0.5) + n * 0.2)) * 2 - 0.1);
				col.rgb = lerp(_Color.rgb, black, n);
				col *= cutoff;
				// Fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				//UNITY_OPAQUE_ALPHA(col.a);
				return col;
			}
			ENDCG
		}
	}
}
