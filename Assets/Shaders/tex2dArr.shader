Shader "Custom/2DArrayTexture" {
	Properties {
		_TexArr("Texture Array", 2DArray) = "" {}
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
				float2 uv : TEXCOORD0;

			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
				float3 uv : TEXCOORD0;
			};

			// Converts rgba to hsva, lerps in hsv and returns a rgba value
			float3 RGBtoHSV(float3 RGB) {
				float4 k = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				float4 p = RGB.g < RGB.b ? float4(RGB.b, RGB.g, k.w, k.z) : float4(RGB.gb, k.xy);
				float4 q = RGB.r < p.x ? float4(p.x, p.y, p.w, RGB.r) : float4(RGB.r, p.yzx);
				float d = q.x - min(q.w, q.y);
				float e = 1.0e-10;
				return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}

			float3 HSVToRGB(float3 HSV) {
				float4 k = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
				float3 p = abs(frac(HSV.xxx + k.xyz) * 6.0 - k.www);
				return HSV.z * lerp(k.xxx, clamp(p - k.xxx, 0.0, 1.0), HSV.y);
			}

			v2f vert(appdata i) {
				v2f o;
				o.vertex = UnityObjectToClipPos(i.vertex);
				o.uv.xy = i.uv;
				o.color = i.color;
				return o;
			}

			UNITY_DECLARE_TEX2DARRAY(_TexArr);

			float4 frag(v2f i) : SV_Target {
				int slice = i.color.r - 2.5;
				int modSlice = i.color.g - 2.5;
				


				half4 modTex = UNITY_SAMPLE_TEX2DARRAY(_TexArr, float3(i.uv.x, i.uv.y, modSlice));
				half4 baseTex = UNITY_SAMPLE_TEX2DARRAY(_TexArr, float3(i.uv.x, i.uv.y, slice));


				float4 modHSV = float4(RGBtoHSV(modTex.rgb), modTex.a);
				float4 baseHSV= float4(RGBtoHSV(baseTex.rgb), baseTex.a);

				half4 o = half4(HSVToRGB(lerp(modHSV, baseHSV, 1 - modTex.a).rgb),modTex.a + baseTex.a);
				o.a = 1;
				return o;
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}