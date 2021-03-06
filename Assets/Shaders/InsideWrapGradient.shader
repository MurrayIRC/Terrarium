﻿Shader "InsideWrap/Hardness (Half Lambert) with Gradient"
{
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_SecondaryTex("Secondary (RGB)", 2D) = "white" {}
		_NormalTex("Normal Map", 2D) = "bump" {}

		_ColorTop("Top Color", Color) = (1,1,1,1)
		_ColorMid("Mid Color", Color) = (1,1,1,1)
		_ColorBot("Bot Color", Color) = (1,1,1,1)
		_Middle("Middle", Range(0.001, 0.999)) = 0.5

		_Hardness("Hardness", Range(.25, 1)) = 0.5
	}
		SubShader{
		Tags{ "Queue" = "Geometry" }
		Cull Off
		CGPROGRAM
		
	#pragma surface surf WrapLambert
	half _Hardness;
	half4 _ShadowColor;

	half4 LightingWrapLambert(SurfaceOutput s, half3 lightDir, half atten) {
		s.Normal = normalize(s.Normal);

		half distAtten;
		if (_WorldSpaceLightPos0.w == 0.0)
			distAtten = 1.0;
		else
			distAtten = saturate(1.0 / length(lightDir));

		half diff = (max(0, dot(s.Normal, lightDir)) * atten + 1 - _Hardness) * _Hardness; ;

		half4 c;
		c.rgb = (s.Albedo * diff * _LightColor0) * distAtten;
		c.a = s.Alpha;
		return c;
	}

	struct Input {
		float2 uv_MainTex;
		float2 uv_SecondaryTex;
		float2 uv_NormalTex;
		float3 viewDir;
		float4 screenPos : TEXCOORD1;
	};

	sampler2D _MainTex;
	sampler2D _SecondaryTex;
	sampler2D _NormalTex;

	fixed4 _ColorTop;
	fixed4 _ColorMid;
	fixed4 _ColorBot;
	float  _Middle;
	float4 screenPos;


	void surf(Input IN, inout SurfaceOutput o) {

		fixed4 gradient = lerp(_ColorBot, _ColorMid, IN.uv_MainTex.y / _Middle) * step(IN.uv_MainTex.y, _Middle);
		gradient += lerp(_ColorMid, _ColorTop, (IN.uv_MainTex.y - _Middle) / (1 - _Middle)) * step(_Middle, IN.uv_MainTex.y);

		o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb * tex2D(_SecondaryTex, IN.uv_SecondaryTex).rgb * gradient;
		o.Normal = UnpackNormal(tex2D(_NormalTex, IN.uv_NormalTex));
		//o.Albedo = screenPos;
		//if (dot(o.Normal, IN.viewDir) < -.25) {
		//	o.Albedo = float4(0, 0, 0, 1);
		//}
	}

	ENDCG
	}
		FallBack "Standard"
}
