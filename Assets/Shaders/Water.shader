// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// This shader is a bit odd because i messed up the water model in blender, so the normals in the model don't make much sense.
Shader "Custom/Water" {
	Properties {
		[NoScaleOffset] _SkyCubemap("Skybox", Cube) = "" {}
		[NoScaleOffset] _SkyCubemapCorrupted("Corrupted Skybox", Cube) = "" {}
		_CorruptionFactor("Corruption Factor", Range(0, 1)) = 0
	}
	SubShader{
		Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			AlphaToMask On

			Tags{
				"RenderType" = "Fade"
				"Queue" = "Transparent"
				"LightMode" = "ForwardBase"
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing

			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"
			#include "utils.hlsl"
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

			// vertex input: position, tangent
			struct appdata {
				float4 vertex : POSITION;
				float4 normal : NORMAL;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				float3 posEye : TEXCOORD0;
				float3 lightDirEye : TEXCOORD1;
				float3 worldRefl : TEXCOORD2;
				float3 eyeNormal : TEXCOORD3;
				SHADOW_COORDS(4) // put shadows data into TEXCOORD4
				fixed3 diff : COLOR0;
				fixed3 ambient : COLOR1;
			};

			samplerCUBE _SkyCubemap;
			samplerCUBE _SkyCubemapCorrupted;
			float _CorruptionFactor;

			v2f vert(appdata v) {
				v2f o;
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
				//Normal manipulation for wavy effect
				float3 samplePos = worldPos - _Time.y * 0.25;
				samplePos.xyz *= 11.2;
				float3 normalMod = float3(noise(samplePos), noise(samplePos + float3(247.5, 567.4, 31.74)), noise(samplePos + float3(247.5, 567.4, 31.74) / 2));
				float wavyness = 0.05;
				float3 modifiedNormal = normalize(v.normal + normalMod * wavyness);
				//Only sampling the skybox in the up direction helps blend egdes in with the rest of the water body
				// Even if it makes the reflection inaccurate.
				float3 modifiedReflectNormal = normalize(float3(0, 1, 0) + normalMod * wavyness);
				//Usefull data
				o.pos = UnityObjectToClipPos(v.vertex);
				o.posEye = UnityObjectToViewPos(v.vertex);
				o.lightDirEye = mul(UNITY_MATRIX_V, _WorldSpaceLightPos0); //It's a directional light
				o.eyeNormal = mul(UNITY_MATRIX_MV, modifiedNormal); //Important note, UnityObjectToViewPos fucks up nomral transforms, because it uses float4(normal, 1)
				half3 worldNormal = UnityObjectToWorldNormal(modifiedNormal);
				//Reflection
				float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
				o.worldRefl = reflect(-worldViewDir, modifiedReflectNormal);
				//Lighting
				half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
				o.diff = nl * _LightColor0;
				o.ambient = ShadeSH9(half4(worldNormal, 1));
				// Shadow
				TRANSFER_SHADOW(o);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target{
				//Reflection
				half3 skyColor1 = DecodeHDR(texCUBE(_SkyCubemap, i.worldRefl), unity_SpecCube0_HDR);
				half3 skyColor2 = DecodeHDR(texCUBE(_SkyCubemapCorrupted, i.worldRefl), unity_SpecCube0_HDR);
				half3 skyColor = lerp(skyColor1, skyColor2, _CorruptionFactor);
				//Shadow
				fixed shadow = SHADOW_ATTENUATION(i);
				//----Light----
				float3 specular = calcSpecular(i.lightDirEye, i.eyeNormal, i.posEye, 5);
				float3 light = (i.diff + specular) * shadow + i.ambient * 0.5;
				//Combine into final color
				half4 c = { 1, 1, 1, 0.94 };
				c.rgb = skyColor;
				c.rgb *= light;
				return c;
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
