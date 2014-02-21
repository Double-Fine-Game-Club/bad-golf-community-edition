Shader "Custom/TestShader" {
	Properties {
		_ColorTint("Color Tint", Color) = (1, 1, 1, 1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		
		CGPROGRAM
		#pragma surface surf Lambert

		struct Input {
			float4 color: Color;
			float2 uv_MainTex;
		};

		float4 _ColorTint;
		sampler2D _MainTex;

		void surf (Input IN, inout SurfaceOutput o) {
			IN.color = _ColorTint;
			o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb * IN.color;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
