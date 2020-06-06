Shader "Ian's Fire Pack/Self-Illumin-Shimmer" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
	
	_Overlay ("Overlay (RGB)", 2D) = "white" {}
	_OverlayColor ("Overlay Color", Color) = (1,1,1,1)
	
	_ScrollDir ("Scroll Direction", Vector) = (0, 0, 0, 0)
	
}

SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 200
	
CGPROGRAM
#pragma surface surf Lambert

sampler2D _MainTex;
sampler2D _BumpMap;
fixed4 _Color;
sampler2D _Overlay;
fixed4 _OverlayColor;
fixed4 _ScrollDir;

struct Input {
	float2 uv_MainTex;
	float2 uv_Overlay;
	float2 uv_BumpMap;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
	fixed4 c = tex * _Color;
	
	fixed3 overlay1 = tex2D(_Overlay, IN.uv_Overlay + float2(_Time.x * _ScrollDir.x, _Time.x * _ScrollDir.y) ).rgb;
	fixed3 overlay2 = tex2D(_Overlay, IN.uv_Overlay + float2(_Time.x * _ScrollDir.z, _Time.x * _ScrollDir.w) ).rgb;
	
	
	fixed3 shimmer = pow(c.a, 2) * (overlay1 * overlay2);
	
	o.Albedo = c.rgb;
	o.Emission = c.rgb * c.a + shimmer*_OverlayColor*1.5;
	o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
}
ENDCG
} 
FallBack "Legacy Shaders/Self-Illumin/VertexLit"
CustomEditor "LegacyIlluminShaderGUI"
}
