﻿Shader "Unlit/SplatNormal"
{
	Properties
	{
		_MainTex ("Texture (RGB)", 2D) = "white" {}
		_MaskTex("Mask (R)", 2D) = "white" {}
		_HeightTex("Height (B)", 2D) = "black" {}
		_SubtractiveColor("Subtractive Color", Color) = (0,0,0,0)
	}
	SubShader
	{
//		Blend One SrcAlpha
//			Blend SrcAlpha One
			Blend SrcAlpha OneMinusSrcAlpha

		Zwrite Off
		

		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
			};

			sampler2D _MainTex;
			sampler2D _MaskTex;
			sampler2D _HeightTex;
			float4 _MainTex_ST;
			float4 _SubtractiveColor;

			//float _Cruncher;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = (tex2D(_MainTex, i.uv) - _SubtractiveColor.rgba) * i.color; // / _Cruncher
				float mask = 1 - tex2D(_MaskTex, i.uv).r;
				col.b -= tex2D(_HeightTex, i.uv).b;
				col.a -= mask;
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}