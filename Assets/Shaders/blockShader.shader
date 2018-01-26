Shader "Custom/block"
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
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv_MainTex : TEXCOORD0;
			};

			struct v2f {
				float2 uv_MainTex : TEXCOORD0;
				UNITY_FOG_COORDS(2)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert(appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv_MainTex = TRANSFORM_TEX(v.uv_MainTex, _MainTex);
				//UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv_MainTex);
				//UNITY_APPLY_FOG(i.fogCoord, col);

				//if(i.fogCoord < 1)
				//	col.a *= (i.fogCoord * 0.9f);

				return col;
		}

		ENDCG
		}
	}
	FallBack "Diffuse"
}