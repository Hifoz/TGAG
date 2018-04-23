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
			static const float lodDist = 250; //250
			static const float lodStartDist = 100; //100

			static float frequencies[COLOR_COUNT] = {
				7.74,	//Dirt
				4.74,	//Stone
				7.74,	//Sand
				10.74,	//Grass
				2.74	//Snow
			};

			static fixed3 colors1[COLOR_COUNT] = {
				fixed3(0.729, 0.505, 0.070),	//Dirt
				fixed3(0.509, 0.509, 0.509),	//Stone
				fixed3(0.988, 0.827, 0.474),	//Sand
				fixed3(0.584, 0.811, 0.027),	//Grass
				fixed3(1, 1, 1)					//Snow
			};

			static fixed3 colors2[COLOR_COUNT] = {
				colors1[0] / 1.5,	//Dirt
				colors1[1] / 2,		//Stone
				colors1[2] / 1.4,	//Sand
				colors1[3] / 1.4,	//Grass
				colors1[4] / 1.5	//Snow
			};

			fixed3 calculateColor(float3 samplePos, int index, float lod) {
				float n = noise(samplePos, frequencies[index] * (1 - lod) + frequencies[index] * lod * 0.3);
				return lerp(colors1[index], colors2[index], n);
			}
	
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

			float4 frag(v2f i) : SV_Target {
				//shadow
				fixed shadow = SHADOW_ATTENUATION(i);
				//light
				fixed3 specular = calcSpecular(i.lightDirEye, i.eyeNormal, i.posEye, 2);
				fixed3 light = (i.diff + specular * 0.15) * shadow  + i.ambient * 0.5;
				//Color
				//colorIndex gets encoded into uv as such: uv.x = index / COLOR_COUNT + small float	
				//LOD works by making block 100% modifier color at a distance, so grass block would become 100% green at a distance
				// This helps reduce aliasing, which shows it self as brown/green wave thinges.
				// The block transition into 100% modifier color between lodStart and lod.
				float dist = length(i.posEye);
				float lod = dist < lodDist;
				float lodStart = dist > lodStartDist;
				float lodLevel = saturate((dist - lodStart) / lodDist);
				fixed3 color1 = calculateColor(i.worldPos, i.color.r + 0.01, lodStart);
				fixed3 color2 = calculateColor(i.worldPos, i.color.g + 0.01, lodStart);
				
				//work out modifiers, for side blending
				float blendingNoise = noise(i.worldPos, 6.4) / 10;
				fixed normal = (i.uv.y  < ((0.8 + blendingNoise * (1 - lodStart)) * (1 - lodLevel))) * lod;
				fixed modifier = 1 - normal;
				//Calculate final color
				fixed4 o = fixed4(1, 1, 1, 1);
				o.rgb = color1 * normal + color2 * modifier;
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
	}
}