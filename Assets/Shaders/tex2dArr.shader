Shader "Custom/2DArrayTexture" {
	Properties {
		_TexArr("Tex", 2DArray) = "" {}
		_Color("Color", Color) = (1,1,1,1)
		_Type("Type (0: Base block type, 1: Modifier type)", Int) = 0
	}
	SubShader {
		Pass {
			Tags{ "Queue" = "Transparent" }
			LOD 200

			Blend SrcAlpha OneMinusSrcAlpha
			AlphaToMask On


			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma target 3.5

			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float4 color : COLOR;
#if _Type == 0
				float2 uv : TEXCOORD0;
#else
				float2 uv : TEXCOORD1;
#endif
			};	


			struct v2f {
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
#if _Type == 0
				float3 uv : TEXCOORD0;
#else
				float3 uv : TEXCOORD1;
#endif
			};


			v2f vert(appdata i) {
				v2f o;
				o.vertex = UnityObjectToClipPos(i.vertex);
				o.uv.xy = i.uv;
				o.color = i.color;
				return o;
			}

			UNITY_DECLARE_TEX2DARRAY(_TexArr);

			half4 frag(v2f i) : SV_Target {
				i.uv.z = i.color.r * 255;
				
				return UNITY_SAMPLE_TEX2DARRAY(_TexArr, i.uv.xyz);

			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}