// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "FX/Wire Animated (Standard)"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_Color("Color", Color) = (0.1470588,0.1470588,0.1470588,0)
		_MainTex("MainTex", 2D) = "white" {}
		_BumpMap("BumpMap", 2D) = "bump" {}
		_SwaySpeed("Sway Speed", Range( 0 , 10)) = 1.5
		_SpeedVariation("Speed Variation", Range( 0 , 1)) = 0.5
		_SwayStrength("Sway Strength", Range( 0 , 10)) = 1
		_Direction("Direction", Vector) = (1,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" }
		LOD 200
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma exclude_renderers d3d9 xbox360 psp2 n3ds wiiu 
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows nodirlightmap vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float _SwaySpeed;
		uniform float _SpeedVariation;
		uniform float _SwayStrength;
		uniform float3 _Direction;
		uniform sampler2D _BumpMap;
		uniform float4 _BumpMap_ST;
		uniform float4 _Color;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform float _Cutoff = 0.5;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float temp_output_6_0_g12 = ( _SwaySpeed * _Time.y );
			float lerpResult11_g12 = lerp( temp_output_6_0_g12 , ( temp_output_6_0_g12 * v.color.r ) , _SpeedVariation);
			v.vertex.xyz += ( ( sin( lerpResult11_g12 ) * ( v.color.g * v.color.a * _SwayStrength ) ) * _Direction );
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_BumpMap = i.uv_texcoord * _BumpMap_ST.xy + _BumpMap_ST.zw;
			o.Normal = UnpackNormal( tex2D( _BumpMap, uv_BumpMap ) );
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float4 tex2DNode142 = tex2D( _MainTex, uv_MainTex );
			o.Albedo = ( _Color * tex2DNode142 ).rgb;
			o.Smoothness = ( 1.0 - tex2DNode142.a );
			o.Alpha = 1;
			clip( tex2DNode142.a - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=16700
1927;29;1906;1004;-1801.541;582.4443;1.3;True;False
Node;AmplifyShaderEditor.CommentaryNode;179;2130.888,347.5534;Float;False;253;252;G=Strength A=Mask;1;78;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;162;2128.538,-16.49989;Float;False;253;252;R=Speed var;1;135;;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector3Node;168;2230.495,736.2986;Float;False;Property;_Direction;Direction;7;0;Create;True;0;0;False;0;1,0,0;1,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ColorNode;124;2688.777,-517.9031;Float;False;Property;_Color;Color;1;0;Create;True;0;0;False;0;0.1470588,0.1470588,0.1470588,0;0.1132075,0.1132075,0.1132075,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;135;2155.538,47.50005;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;174;2138.595,249.8952;Float;False;Property;_SpeedVariation;Speed Variation;5;0;Create;True;0;0;False;0;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;16;2138.356,631.615;Float;False;Property;_SwayStrength;Sway Strength;6;0;Create;True;0;0;False;0;1;10;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;13;2136.5,-97.51276;Float;False;Property;_SwaySpeed;Sway Speed;4;0;Create;True;0;0;False;0;1.5;0.31;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;78;2180.888,397.5529;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;142;2662.812,-331.2949;Float;True;Property;_MainTex;MainTex;2;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;143;3018.812,-386.2949;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;145;3069.9,-224.8338;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;144;2671.05,-118.3701;Float;True;Property;_BumpMap;BumpMap;3;0;Create;True;0;0;False;0;None;None;True;0;False;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;190;2672.905,210.737;Float;False;WireAnimation;-1;;12;680576445727747478653f64c7ca1583;0;7;20;FLOAT;0;False;21;FLOAT;0;False;28;FLOAT;0;False;23;FLOAT;0;False;25;FLOAT;0;False;26;FLOAT;0;False;27;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;3533.507,-333.7967;Float;False;True;2;Float;;200;0;Standard;FX/Wire Animated (Standard);False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Masked;0.5;True;True;0;False;TransparentCutout;;AlphaTest;All;False;True;True;True;True;True;True;False;True;True;False;False;False;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;0;4;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;1;False;-1;1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;200;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;143;0;124;0
WireConnection;143;1;142;0
WireConnection;145;0;142;4
WireConnection;190;20;13;0
WireConnection;190;21;135;1
WireConnection;190;28;174;0
WireConnection;190;23;78;4
WireConnection;190;25;78;2
WireConnection;190;26;16;0
WireConnection;190;27;168;0
WireConnection;0;0;143;0
WireConnection;0;1;144;0
WireConnection;0;4;145;0
WireConnection;0;10;142;4
WireConnection;0;11;190;0
ASEEND*/
//CHKSM=A2FE5F983D2D275CD5306E6D3D1488F8281C3BB8