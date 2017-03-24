﻿Shader "Custom/Bubble" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Cutoff ("Cutoff", Range(0,1)) = 1
	}
	SubShader {
		/*	GrabPass
		{
			"_GrabTexture"
		}*/

		Tags { "RenderType"="Transparent" }
		LOD 200
		Cull off 
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf NoLighting fullforwardshadows alphatest:_Cutoff //vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float4 proj : TEXCOORD;
			float4 grabPos : TEXCOORD1;
		};

		struct appdata
		{
			float4 vertex : POSITION;
			float4 texcoord : TEXCOORD0;
			float3 normal : NORMAL;
		};

		fixed4 _Color;
		//sampler2D _GrabTexture;

		fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
		{
			fixed4 c;
			c.rgb = s.Albedo;
			c.a = s.Alpha;
			return c;
		}
		/*
		void vert(inout appdata v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);

			//https://forum.unity3d.com/threads/refraction-example.78750/
			//refraction distorting uvs
			float4 oPos = mul(UNITY_MATRIX_MVP, v.vertex);
			#if UNITY_UV_STARTS_AT_TOP
				float scale = -1.0;
			#else
				float scale = 1.0;
			#endif
			o.proj.xy = (float2(oPos.x, oPos.y*scale) + oPos.w) * .5;
			o.proj.zw = oPos.zw;

			o.grabPos = ComputeGrabScreenPos(v.vertex);
		}*/

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
			//float4 uv = float4(IN.uv_MainTex.x, IN.uv_MainTex.y, IN.uv_MainTex.y, IN.uv_MainTex.y);
			o.Albedo = _Color;// * tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(IN.proj) * uv).rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	//FallBack "Transparent/Cutout/Diffuse"
}