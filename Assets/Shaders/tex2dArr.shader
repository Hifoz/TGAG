Shader "Example/Sample2DArrayTexture" {
	Properties {
		_TexArr("Tex", 2DArray) = "" {}
		_Color("Color", Color) = (1,1,1,1)
		_Type("Type (0: Base block type, 1: Modifier type)", Int) = 0
	}
	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma target 3.5

			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};


			struct v2f {
				float3 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
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
}