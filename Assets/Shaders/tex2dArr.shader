Shader "Custom/2DArrayTexture" {
	Properties {
		_TexArr("Texture Array", 2DArray) = "" {}
		_Color("Color", Color) = (1,1,1,1)
		_Type("Type (0: Base block type, 1: Modifier type)", Int) = 0
	}
	SubShader {
		Pass {
			Tags{ "Queue" = "Opaque" }
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
				float2 uv : TEXCOORD0;

			};	


			struct v2f {
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
				float3 uv : TEXCOORD0;
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
				int slice = i.color.r - 1;
				int modSlice = i.color.g - 1;


				half4 baseTex = UNITY_SAMPLE_TEX2DARRAY(_TexArr, float3(i.uv.x, i.uv.y, slice));
				half4 modTex = UNITY_SAMPLE_TEX2DARRAY(_TexArr, float3(i.uv.x, i.uv.y, modSlice));

				if (modTex.a != 0)
					return modTex;
				return baseTex;

			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}