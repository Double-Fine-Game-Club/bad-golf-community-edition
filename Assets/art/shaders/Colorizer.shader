Shader "Custom/Colorizer" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_MaskTex ("Base (RGBA)", 2D) = "black" {}

		_Color01("Color Tint 1", Color) = (1, 1, 1, 1)  
		_Color02("Color Tint 2", Color) = (1, 1, 1, 1)
		_Color03("Color Tint 3", Color) = (1, 1, 1, 1)
		_Color04("Color Tint 4", Color) = (1, 1, 1, 1)
	}
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex, _MaskTex;

		fixed3 _Color01, _Color02, _Color03, _Color04;

		struct Input 
		{
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) 
		{
			fixed3 c = tex2D (_MainTex, IN.uv_MainTex).rgb;
			fixed4 mask = tex2D(_MaskTex, IN.uv_MainTex).rgba;
			
			o.Albedo = c.rgb ;
			o.Albedo = lerp( o.Albedo, _Color01, mask.r);
			o.Albedo = lerp( o.Albedo, _Color02, mask.g);
			o.Albedo = lerp( o.Albedo, _Color03, mask.b);
			o.Albedo = lerp( o.Albedo, _Color04, mask.a); 
			o.Alpha = 1;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
