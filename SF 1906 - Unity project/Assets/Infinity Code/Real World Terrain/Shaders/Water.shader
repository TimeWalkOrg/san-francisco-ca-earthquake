Shader "Real World Terrain Water"
{
	Properties 
	{
		_Color("Color", Color) = (1,1,1,1)
		_Texture1("Texture1", 2D) = "black" {}
		_Texture2("Texture2", 2D) = "black" {}
		_MainTexSpeed("MainTexSpeed", Float) = 0
		_Texture2Speed("Texture2Speed", Float) = 0
		_DistortionMap("DistortionMap", 2D) = "black" {}
		_DistortionSpeed("DistortionSpeed", Float) = 0
		_DistortionPower("DistortionPower", Range(0,0.04) ) = 0
		_Specular("Specular", Range(0,7) ) = 1
		_Gloss("Gloss", Range(0.1,2) ) = 0.3
	}
	
	SubShader 
	{
		Tags
		{
			"Queue"="Transparent"
			"IgnoreProjector"="False"
			"RenderType"="Overlay"
		}

		Cull Back
		ZWrite On
		ZTest LEqual
		ColorMask RGBA
		Blend SrcAlpha OneMinusSrcAlpha
		Fog{}
		
		CGPROGRAM
		#pragma surface surf BlinnPhongEditor noforwardadd
		#pragma target 3.0
		
		fixed4 _Color;
		uniform sampler2D _Texture1;
		uniform sampler2D _Texture2;
		half _MainTexSpeed;
		half _Texture2Speed;
		uniform sampler2D _DistortionMap;
		half _DistortionSpeed;
		half _DistortionPower;
		fixed _Specular;
		fixed _Gloss;

		struct EditorSurfaceOutput 
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half3 Gloss;
			half Specular;
			half Alpha;
			half4 Custom;
		};
			
		inline half4 LightingBlinnPhongEditor_PrePass (EditorSurfaceOutput s, half4 light)
		{
			half3 spec = light.a * s.Gloss;
			half4 c;
			c.rgb = (s.Albedo * light.rgb + light.rgb * spec);
			c.a = s.Alpha;
			return c;
		}

		inline half4 LightingBlinnPhongEditor (EditorSurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
		{
			half3 h = normalize (lightDir + viewDir);
			
			half diff = max (0, dot ( lightDir, s.Normal ));
			
			float nh = max (0, dot (s.Normal, h));
			float spec = pow (nh, s.Specular*128.0);
			
			half4 res;
			res.rgb = _LightColor0.rgb * diff;
			res.w = spec * Luminance (_LightColor0.rgb);
			res *= atten * 2.0;

			return LightingBlinnPhongEditor_PrePass( s, res );
		}
		
		struct Input 
		{
			float3 viewDir;
			float2 uv_DistortionMap;
			float2 uv_Texture1;
			float2 uv_Texture2;
		};

		void surf (Input IN, inout EditorSurfaceOutput o) 
		{
			o.Normal = float3(0.0,0.0,1.0);
			o.Alpha = 1.0;
			o.Albedo = 0.0;
			o.Emission = 0.0;
			o.Gloss = 0.0;
			o.Specular = 0.0;
			o.Custom = 0.0;			
					
			float DistortSpeed=_DistortionSpeed * _Time;
			float2 DistortUV=(IN.uv_DistortionMap.xy) + DistortSpeed;
			float4 DistortNormal = float4(UnpackNormal( tex2D(_DistortionMap,DistortUV)).xyz, 1.0 );
			float2 FinalDistortion = DistortNormal.xy * _DistortionPower;
			
			float Multiply2=_Time * _MainTexSpeed;
			float2 MainTexUV=(IN.uv_Texture1.xy) + Multiply2; 
			
			float4 Tex2D0=tex2D(_Texture1,MainTexUV + FinalDistortion);
			
			float Multiply3=_Time * _Texture2Speed;
			float2 Tex2UV=(IN.uv_Texture2.xy) + Multiply3;
			
			float4 Tex2D1=tex2D(_Texture2,Tex2UV + FinalDistortion); 
			
			float4 TextureMix=Tex2D0 * Tex2D1;
			
			float4 FinalDiffuse=_Color * TextureMix;	
			
			
			o.Albedo = FinalDiffuse;
			o.Emission = FinalDiffuse;
			o.Specular = _Gloss;
			o.Gloss = _Specular;

			o.Normal = normalize(o.Normal);
		}
	ENDCG
	}
	Fallback "Diffuse"
}