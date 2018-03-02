Shader "Custom/Animal" {
	Properties {

	}
	SubShader {
		Pass {
			Tags{ 
				"Queue" = "Geometry"
				"LightMode" = "ForwardBase" 
			}
			LOD 300

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
				float3 posEye : TEXCOORD0;
				float3 lightDirEye : TEXCOORD1;
				float3 eyeNormal : TEXCOORD2;
				float3 worldPos : TEXCOORD3;
				SHADOW_COORDS(4) // put shadows data into TEXCOORD4
				float3 noisePos : TEXCOORD5;
				float2 animalData : TEXCOORD6;
				fixed3 diff : COLOR0;
				fixed3 ambient : COLOR1;
			};
			

			v2f vert(appdata v) {
				v2f o;
				//Usuefull data
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				half3 worldNormal = UnityObjectToWorldNormal(v.normal);
				o.eyeNormal = (mul(UNITY_MATRIX_MV, float4(v.normal.x, v.normal.y, v.normal.z, 0))).xyz;
				o.posEye = UnityObjectToViewPos(v.vertex);
				o.lightDirEye = mul(UNITY_MATRIX_V, -_WorldSpaceLightPos0); //It's a directional light
				//Light
				half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
				o.diff = nl;
				o.ambient = ShadeSH9(half4(worldNormal, 1));
				//Shadow
				TRANSFER_SHADOW(o);

				o.noisePos = v.color.rbg;
				o.animalData = v.uv;
				return o;
			}

			UNITY_DECLARE_TEX2DARRAY(_TexArr);

			float4 frag(v2f i) : SV_Target {
				//shadow
				fixed shadow = SHADOW_ATTENUATION(i);
				//light
				float3 specular = calcSpecular(i.lightDirEye, i.eyeNormal, i.posEye, 5);
				fixed3 light = (i.diff + specular * 0.5) * shadow + i.ambient;

				half4 o;
				half3 color1 = { cos((i.animalData.x * i.animalData.y)  * 517.72), cos((i.animalData.x + i.animalData.y) * 444.54), sin((i.animalData.x / i.animalData.y) * 314.22) };
				half3 color2 = { sin((i.animalData.x + i.animalData.y)  * 922.25), cos((i.animalData.x - i.animalData.y) * 231.97), sin((i.animalData.x * i.animalData.y) * 114.88) };
				//Remove negative color values
				color1 = saturate(color1);
				color2 = saturate(color2);
				//Make all black animals black/white:
				float nonZero = ceil((color1.x + color1.y + color1.z + color2.x + color2.y + color2.z - 0.05) / 12); // if the colors are not zero, this will be 1
				color1 += half3(1, 1, 1) * (1 - nonZero);
				
				float3 seed = float3(1, 1, 1) * 841.4 * i.animalData.y;
				float frequency = 111.3 * i.animalData.x;

				float n = noise(i.noisePos * frequency + seed) * 0.8f + 0.1;

				o.a = 1;
				o.rgb = color1 * inRange(n, 0.0, 0.45) + color2 * inRange(n, 0.55, 1.0);			
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