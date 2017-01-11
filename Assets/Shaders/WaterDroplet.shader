﻿// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// SHADER SMEARING FROM CHRIS WADE'S 3D SMEAR SHADER! 
// https://github.com/cjacobwade/HelpfulScripts/blob/master/SmearEffect/Smear.shader

Shader "Custom/WaterDroplet" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0

		_PushPosA("Push Position A", Vector) = (-1, 0, 0, 0)
		_PushPosB("Push Position B", Vector) = (1, 0, 0, 0)
		_PushDistance("Push Distance", Float) = 1
		_PushAmount("Push Amount", Float) = 1
		_PushScale("Push Scale", Float) = 1
		_PushImpact("Push Impact", Float) = 1

		_Position("Position", Vector) = (0, 0, 0, 0)
		_PrevPosition("Prev Position", Vector) = (0, 0, 0, 0)
		_SmearIntensity("Smear Intensity", Float) = 2

		//_NoiseScale("Noise Scale", Float) = 15
		//_NoiseHeight("Noise Height", Float) = 1.3
	}
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard vertex:vert fullforwardshadows addshadow

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input 
		{
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		fixed4 _PushPosA;
		fixed4 _PushPosB;
		half _PushDistance;
		half _PushAmount;
		half _PushScale;
		half _PushImpact;

		fixed4 _PrevPosition;
		fixed4 _Position;
		half _SmearIntensity;
	
		//half _NoiseScale;
		//half _NoiseHeight;

		/*
		float hash(float n)
		{
			return frac(sin(n)*43758.5453);
		}

		float noise(float3 x)
		{
			// The noise function returns a value in the range -1.0f -> 1.0f
	
			float3 p = floor(x);
			float3 f = frac(x);
	
			f = f*f*(3.0 - 2.0*f);
			float n = p.x + p.y*57.0 + 113.0*p.z;
	
			return lerp(lerp(lerp(hash(n + 0.0), hash(n + 1.0), f.x),
				lerp(hash(n + 57.0), hash(n + 58.0), f.x), f.y),
				lerp(lerp(hash(n + 113.0), hash(n + 114.0), f.x),
					lerp(hash(n + 170.0), hash(n + 171.0), f.x), f.y), f.z);
		}
		*/

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			fixed4 worldPos = mul(unity_ObjectToWorld, v.vertex);
	
			fixed3 worldOffset = _Position.xyz - _PrevPosition.xyz; // -5
			fixed3 localOffset = worldPos.xyz - _Position.xyz; // -5
	
			// World offset should only be behind swing
			float dirDot = dot(normalize(worldOffset), normalize(localOffset));
			fixed3 unitVec = fixed3(1, 1, 1) /* * _NoiseHeight */;
			worldOffset = clamp(worldOffset, unitVec * -1, unitVec);
			worldOffset *= -clamp(dirDot, -1, 0) * lerp(1, 0, step(length(worldOffset), 0));
	
			fixed3 smearOffset = -worldOffset.xyz /* * lerp(1, noise(worldPos * _NoiseScale), step(0, _NoiseScale)) */;
			worldPos.xyz += smearOffset * _SmearIntensity;
			v.vertex = mul(unity_WorldToObject, worldPos);

			// Vertex Depression
			/*
			float dist1 = clamp(distance(worldPos, _PushPosA), 0, _PushDistance);
        	float dist2 = clamp(distance(worldPos, _PushPosB), 0, _PushDistance);
            //v.vertex.y += _Amount * sin(abs(dist1/_Scale)-_Impact) * sin(abs(dist2/_Scale)-_Impact);
            v.vertex.xyz += v.normal * _PushAmount * sin(abs(dist1 / _PushScale) - _PushImpact) * sin(abs(dist2 / _PushScale) - _PushImpact);
            v.vertex.xyz += v.normal;
			*/
		}

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
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
