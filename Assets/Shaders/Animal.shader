Shader "Custom/Animal" {
	Properties {
		_MainTex("Texture", 2D) = "white" {}
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
			#include "textures.hlsl"
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
				o.eyeNormal = normalize(UnityObjectToViewPos(v.normal));
				o.posEye = UnityObjectToViewPos(v.vertex);
				//o.lightDirEye = normalize(_WorldSpaceLightPos0); //It's a directional light
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
				//float3 specular = calcSpecular(i.lightDirEye, i.eyeNormal, i.posEye, 5);
				fixed3 light = (i.diff /*+ specular * 0.4*/) * shadow + i.ambient;

				half4 o;
				half3 green = { 0, 1, 0 };
				half3 darkGreen = { 0, 0.4, 0 };
				half3 red = { 1, 0, 0 };
				half3 darkRed = { 0.4, 0, 0 };
				half3 blue = { 0, 0, 1 };
				half3 darkBlue = { 0, 0, 0.4 };
				half3 white = { 1, 1, 1 };
				half3 purple = { 0.5, 0, 0.5 };
				
				float3 seed = float3(1, 1, 1) * 841.4 * sin(i.animalData.x * 2.4 * i.animalData.y);
				float frequency = 111.3 * i.animalData.x;

				//The magic numbers in the range comes from the fact that skin types are encoded as { 1, 2, 3, 4 } / 5;
				//in i.animalData.y
				float skinType1 = inRange(i.animalData.y, 0.18, 0.22);
				float skinType2 = inRange(i.animalData.y, 0.38, 0.42);
				float skinType3 = inRange(i.animalData.y, 0.58, 0.62);
				float skinType4 = inRange(i.animalData.y, 0.78, 0.82);

				float n = noise(i.noisePos * frequency + seed) * 0.8f + 0.1;

				o.a = 1;
				o.rgb = 
					(green * inRange(n, 0.0, 0.45) + darkGreen * inRange(n, 0.55, 1.0)) * skinType1 +
					(red * inRange(n, 0.0, 0.45) + darkRed * inRange(n, 0.55, 1.0)) * skinType2 +
					(blue * inRange(n, 0.0, 0.45) + darkBlue * inRange(n, 0.55, 1.0)) * skinType3 +
					(white * inRange(n, 0.0, 0.45) + purple * inRange(n, 0.55, 1.0)) * skinType4;
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