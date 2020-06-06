// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Ian's Fire Pack/Additive Shimmer" {
Properties {
	_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Particle Texture", 2D) = "white" {}

	_Overlay ("Overlay (RGB)", 2D) = "white" {}
	_OverlayColor ("Overlay Color. Overlay Strength(A)", Color) = (1,1,1,1)
	
	_ScrollDir ("Scroll Direction", Vector) = (0, 0, 0, 0)
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend SrcAlpha One
	ColorMask RGB
	Cull Off Lighting Off ZWrite Off
	
	SubShader {
		Pass {
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_particles
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			fixed4 _TintColor;
			sampler2D _Overlay;
			fixed4 _OverlayColor;
			fixed4 _ScrollDir;
			
			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 uv_Overlay : TEXCOORD2;
				UNITY_FOG_COORDS(1)
			};
			
			float4 _MainTex_ST, _Overlay_ST; 

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);

				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				o.uv_Overlay = TRANSFORM_TEX(v.texcoord, _Overlay); 
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{

				fixed4 overlay1 = tex2D(_Overlay, i.uv_Overlay + float2(_Time.x * _ScrollDir.x, _Time.x * _ScrollDir.y) );
				fixed4 overlay2 = tex2D(_Overlay, i.uv_Overlay + float2(_Time.x * _ScrollDir.z, _Time.x * _ScrollDir.w) );
	

				fixed4 col = 2.0f * i.color * _TintColor * tex2D(_MainTex, i.texcoord) * lerp(fixed4(1, 1, 1, 1), (overlay1*overlay2)*_OverlayColor, _OverlayColor.a);
				UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(0,0,0,0)); // fog towards black due to our blend mode
				return col;
			}
			ENDCG 
		}
	}	
}
}
