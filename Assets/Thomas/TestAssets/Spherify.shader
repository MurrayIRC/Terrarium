﻿Shader "Custom/Spherify" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0

		_Center ("Center", vector) = (0,0,0,0)
		_SphereScale("Sphere Scale", float) = 1
		_Spherification ("Spherify Amount", Range(0,1)) = 0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert addshadow

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		float _Spherification;
		float _SphereScale;
		float4 _Center;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_CBUFFER_END
		
		void vert(inout appdata_full v) {
			float3 directionToCenter = normalize(_Center + v.vertex.xyz);
			float3 pointOnSphere = directionToCenter * _SphereScale;
			//float3 distanceToCenter = distance(_Center, v.vertex.xyz);
			//pointOnSphere = v.vertex/distanceToCenter*2; //inflation
			//pointOnSphere = clamp(pointOnSphere, -_SphereScale, _SphereScale); //cube
			v.vertex.xyz = lerp(v.vertex.xyz, pointOnSphere, _Spherification);
			v.normal = lerp(v.normal, directionToCenter, _Spherification);
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
