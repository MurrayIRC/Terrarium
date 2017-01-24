﻿Shader "Custom/TransparentColor" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Cutoff ("Alpha Cutoff", Range(0, 1)) = 1
	}
	SubShader 
	{
		Tags { "RenderType"="Transparent" "Queue"="AlphaTest" }
		LOD 200

		//Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Lambert vertex:vert alphatest:_Cutoff

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
		{
			fixed4 c;
			c.rgb = s.Albedo;
			c.a = s.Alpha;
			return c;
		}

		sampler2D _MainTex;

		struct Input 
		{
			float2 uv_MainTex;
		};

		fixed4 _Color;

		void vert (inout appdata_full v)
		{
			fixed4 worldPos = mul(transpose(unity_ObjectToWorld), float4(0, 1, 0, 1));
			v.normal = worldPos;
		}

		void surf (Input IN, inout SurfaceOutput o) 
		{
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Standard"
}
