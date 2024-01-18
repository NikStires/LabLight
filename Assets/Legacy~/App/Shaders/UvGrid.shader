Shader "LabLightAR/UvGrid"
{
	Properties
	{
		_Color("Base Color", Color) = (0.0, 0.0, 0.0)
		_LineColorLarge("Line Color", Color) = (1.0, 0.8, 0.01)
		_LineSize("Line Width Factor", Range(0.01, 0.5)) = .02
		_Scale("Scale", Vector) = (5., 5., 0.)
		_FadeDir("FadeDirection", Vector) = (1., 0., 0.)
		_Fade("Fade", Range(0, 1)) = .5
	}

	SubShader
	{
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 100

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency
			Cull OFF

			CGPROGRAM
			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag

			fixed4 _Color;
			fixed2 _Scale;
			fixed _LineSize;
			fixed _LineCrispness;
			fixed _Smoothness;
			fixed4 _LineColorLarge;
			float _Fade;
			fixed4 _FadeDir;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{

				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;

				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(appdata v)
			{
				UNITY_SETUP_INSTANCE_ID(v);

				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;

				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{

				float2 gridUVs = saturate((1 - abs(frac(i.uv * _Scale) - 0.5) * 2) - _LineSize);
				float2 smoothedUVs = 1 - saturate(gridUVs / fwidth(i.uv * _Scale * 2.0));


				if (dot(i.uv, normalize(_FadeDir.xy)) > _Fade)
					discard;

				return lerp(_Color, _LineColorLarge,  max(smoothedUVs.x, smoothedUVs.y));
			}
			ENDCG
		}
	}
}
