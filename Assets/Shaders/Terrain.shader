//#define DRAW_BW
//#define DRAW_NORMAL


Shader "Custom/Terrain" {
	Properties {

	}
	SubShader {
		Pass {
			Tags{ 
				"Queue" = "Transparent"
				"LightMode" = "ForwardBase" 
			}
			LOD 300

			Blend SrcAlpha OneMinusSrcAlpha
			AlphaToMask On

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma target 3.5

			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"
			#include "utils.hlsl"
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

			struct appdata {
				float4 vertex : POSITION;
				float4 normal : NORMAL;
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				float4 color : COLOR0;
				float2 uv : TEXCOORD0;
				float3 posEye : TEXCOORD1;
				float3 lightDirEye : TEXCOORD2;
				float3 eyeNormal : TEXCOORD3;
				float3 worldPos : TEXCOORD4;
				SHADOW_COORDS(5) // put shadows data into TEXCOORD5
				fixed3 diff : COLOR2;
				fixed3 ambient : COLOR3;
			};

			static const int COLOR_COUNT = 5;

			static float frequencies[COLOR_COUNT] = {
				12, //Dirt
				12, //Stone
				12, //Sand
				12, //Grass
				12	//Snow
			};

			static half3 colors1[COLOR_COUNT] = {
				half3(0.725, 0.403, 0.035), //Dirt
				half3(0.509, 0.509, 0.509),	//Stone
				half3(1, 0.803, 0.580),		//Sand
				half3(0, 0.858, 0.341),		//Grass
				half3(1, 1, 1)				//Snow
			};

			static half3 colors2[COLOR_COUNT] = {
				half3(0.725, 0.403, 0.035) / 2, //Dirt
				half3(0.509, 0.509, 0.509) / 2,	//Stone
				half3(1, 0.803, 0.580) / 2,		//Sand
				half3(0, 0.858, 0.341) / 2,		//Grass
				half3(1, 1, 1) / 2				//Snow
			};
	
			v2f vert(appdata v) {
				v2f o;
				//Usuefull data
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				half3 worldNormal = UnityObjectToWorldNormal(v.normal);
				o.eyeNormal = mul(UNITY_MATRIX_MV, v.normal.xyz);
				o.posEye = UnityObjectToViewPos(v.vertex);
				o.lightDirEye = mul(UNITY_MATRIX_V, _WorldSpaceLightPos0); //It's a directional light
				//Light
				half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
				o.diff = nl;
				o.ambient = ShadeSH9(half4(worldNormal, 1));
				//Shadow
				TRANSFER_SHADOW(o);

				o.uv = v.uv;
				o.color = v.color;	
				return o;
			}

			UNITY_DECLARE_TEX2DARRAY(_TexArr);

			float4 frag(v2f i) : SV_Target {
				//shadow
				fixed shadow = SHADOW_ATTENUATION(i);
				//light
				float3 specular = calcSpecular(i.lightDirEye, i.eyeNormal, i.posEye, 5);
				fixed3 light = (i.diff + specular * 0.5) * shadow  + i.ambient;

				int colorIndex1 = i.color.r * COLOR_COUNT; //colorIndex gets encoded into uv as such: uv.x = index / COLOR_COUNT + small float
				int colorIndex2 = i.color.g * COLOR_COUNT; //colorIndex gets encoded into uv as such: uv.x = index / COLOR_COUNT + small float
				half4 o = half4(1, 1, 1, 1);

				float normal = i.uv.y < 0.8;
				float modifier = 1 - normal;				

				o.rgb = colors1[colorIndex1] * normal + colors1[colorIndex2] * modifier;
				o.rbg *= light;
				return o;
			}
			ENDCG
		}
	
		// shadow caster rendering pass
		// using macros from UnityCG.cginc
		Pass {
			Tags{ "LightMode" = "ShadowCaster" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#include "UnityCG.cginc"

			struct v2f {
				V2F_SHADOW_CASTER;
			};

			v2f vert(appdata_base v) {
				v2f o;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
					return o;
			}

			float4 frag(v2f i) : SV_Target{
				SHADOW_CASTER_FRAGMENT(i)
			}
				ENDCG
		}
		// shadow casting support
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
	}
}