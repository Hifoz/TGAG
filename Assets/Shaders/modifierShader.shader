///
/// Custom Shader for the modifier layer of the block textures
/// Has transparency and uses the second uv set on the mesh
///


Shader "Custom/modifier" {
	Properties{
		_MainTex("Metallic (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.0
		_Metallic("Metallic", Range(0,1)) = 0.0
	}
	SubShader{
		Tags{ "Queue" = "Transparent" }
		LOD 200

		Blend SrcAlpha OneMinusSrcAlpha
		AlphaToMask On

		CGPROGRAM

		#pragma surface surf Standard fullforwardshadows alpha

		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv2_MainTex;
		};

		half _Glossiness;
		half _Metallic;


		UNITY_INSTANCING_CBUFFER_START(Props)
		UNITY_INSTANCING_CBUFFER_END

		void surf(Input IN, inout SurfaceOutputStandard o) {
			fixed4 c = tex2D(_MainTex, IN.uv2_MainTex);
			o.Albedo = c.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
	ENDCG
	}
	FallBack "Diffuse"
}