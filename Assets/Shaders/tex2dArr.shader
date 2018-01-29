// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Directly taken from Unity doco
Shader "Example/Sample2DArrayTexture" {
	Properties {
		_MyArr("Tex", 2DArray) = "" {}
		_Color("Color", Color) = (1,1,1,1)
	}
	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// to use texture arrays we need to target DX10/OpenGLES3 which
			// is shader model 3.5 minimum
			#pragma target 3.5

			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv2_MainTex : TEXCOORD1;
				float4 color : COLOR;
			};


			struct v2f {
				float3 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
			};

			fixed4 _Color;

			v2f vert(appdata i) {
				v2f o;
				o.vertex = UnityObjectToClipPos(i.vertex);
				o.uv.xy = (i.vertex.xy + 0.5);
				o.color = i.color;
				return o;
			}

			UNITY_DECLARE_TEX2DARRAY(_MyArr);

			half4 frag(v2f i) : SV_Target {
				i.uv.z = i.color.r * 255;
				
				return UNITY_SAMPLE_TEX2DARRAY(_MyArr, i.uv);
			}
			ENDCG
		}
	}
}