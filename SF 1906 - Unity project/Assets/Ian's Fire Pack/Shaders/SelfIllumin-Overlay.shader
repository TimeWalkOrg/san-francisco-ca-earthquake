Shader "Ian's Fire Pack/Self-Illumin-Overlay" {
Properties {
	_Color ("Illum Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB) Alum(A)", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
	_Mask ("Overlay Mask (A)", 2D) = "white" {}
	_Overlay ("Overlay (RGB)", 2D) = "white" {}
	_OverlayColor ("Overlay Color", Color) = (1,1,1,1)
	_ScrollSpeed ("ScrollSpeed (Overlay)", Float) = 1.0
	_OverlayScale ("Overlay Scale", Range(0.0, 5.0)) = 1.0
}
SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 200
	
CGPROGRAM
#pragma surface surf Lambert


fixed4 _Color;
sampler2D _MainTex;
sampler2D _BumpMap;
sampler2D _Mask;
sampler2D _Overlay;
fixed4 _OverlayColor;
fixed _ScrollSpeed;
fixed _OverlayScale;

struct Input {
	float2 uv_MainTex;
	float2 uv_Overlay;
	float2 uv_BumpMap;
	INTERNAL_DATA
	float3 worldNormal;
	float3 worldPos;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
	fixed mask = tex2D(_Mask, IN.uv_MainTex).a;
	fixed4 c = tex;

	float3 worldNormalVector = WorldNormalVector (IN, float3( 0, 0, 1 ));

	fixed3 overlayX= tex2D(_Overlay, IN.worldPos.zy*_OverlayScale + float2(0, -_Time.x*_ScrollSpeed)).rgb*_OverlayColor * pow(abs(worldNormalVector.x), 2);
	fixed3 overlayZ= tex2D(_Overlay, IN.worldPos.xy*_OverlayScale + float2(0, -_Time.x*_ScrollSpeed)).rgb*_OverlayColor * pow(abs(worldNormalVector.z), 2);
	

	o.Albedo = c.rgb;
	o.Emission = c.rgb * c.a * 3 * _Color + (overlayX + overlayZ)*mask*2;;
	o.Alpha = c.a;
	o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
}
ENDCG
} 
FallBack "Legacy Shaders/Self-Illumin/VertexLit"
CustomEditor "LegacyIlluminShaderGUI"
}
