Shader "Custom/modifier"
{
	Properties
	{
		_MainTex("Metallic (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.0
		_Metallic("Metallic", Range(0,1)) = 0.0
	}
	SubShader
	{
		Pass
		{
			Tags{ "Queue" = "Transparent" }
			LOD 200

			Blend SrcAlpha OneMinusSrcAlpha
			AlphaToMask On


			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv2_MainTex : TEXCOORD1;
			};

			struct v2f {
				float2 uv2_MainTex : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert(appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv2_MainTex = TRANSFORM_TEX(v.uv2_MainTex, _MainTex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv2_MainTex);

				return col;
		}

		ENDCG
		}
	}
	FallBack "Diffuse"
}