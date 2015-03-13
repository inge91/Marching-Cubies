Shader "Custom/3D Flat" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
	}
	SubShader {	
		Tags { "RenderType"="Opaque"  "Queue" = "Transparent" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert alpha
		
		fixed4 _Color;

		struct Input {
			float dummy;
		};
		
		
		void surf (Input IN, inout SurfaceOutput o) {
			o.Albedo = _Color.rgb;
			o.Alpha = 1.0f;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}