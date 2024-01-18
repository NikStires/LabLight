// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "LabLightAR/BoxLines"
{
	Properties
	{
		_Color("Base Color", Color) = (0.0, 0.0, 0.0)
		_LineColorLarge("Line Color", Color) = (1.0, 0.8, 0.01)
		_LineSize("Line Width Factor", Range(0.01, 0.5)) = .02
		_CornerSize("Corner Width Factor", Range(0.01, 0.5)) = .02
		_CorneLength("Corner Length Factor", Range(0.01, 0.5)) = .02
		_Scale("Scale", Vector) = (5., 5., 0.)
 	}

		SubShader
	{
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 100

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency
			Cull Front

			CGPROGRAM
			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag

			fixed4 _Color;
			fixed2 _Scale;
			fixed _LineSize;
			fixed _Smoothness;
			fixed4 _LineColorLarge;

			// https://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm
			// p position
			// b box size
			// e edge thickness
			float sdBoxFrame(float3 p, float3 b, float e)
			{
				p = abs(p) - b;
				float3 q = abs(p + e) - e;
				return min(min(
					length(max(float3(p.x, q.y, q.z), 0.0)) + min(max(p.x, max(q.y, q.z)), 0.0),
					length(max(float3(q.x, p.y, q.z), 0.0)) + min(max(q.x, max(p.y, q.z)), 0.0)),
					length(max(float3(q.x, q.y, p.z), 0.0)) + min(max(q.x, max(q.y, p.z)), 0.0));
			}

			float sdBox(float3 p, float3 b)
			{
				float3 q = abs(p) - b;
				return length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0);
			}

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
				float3 world : TEXCOORD1;

				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(appdata v)
			{
				UNITY_SETUP_INSTANCE_ID(v);

				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.world = v.vertex;

				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float sd = sdBoxFrame(i.world, float3(1, 1, 1), _LineSize);

				fixed4 col = _LineColorLarge;

				if (sd < 0 && sd > ((fwidth(sd) - 1) * 2))
				{
					col.a = 1;
				}
				else
				{
					col.a = 0;
				}

				return col;
			}
			ENDCG
		}

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency
			Cull Back

			CGPROGRAM
			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag

			fixed4 _Color;
			fixed3 _Scale;
			fixed _LineSize;
			fixed _Smoothness;
			fixed4 _LineColorLarge;

			float sdBoxFrame(float3 p, float3 b, float e)
			{
				p = abs(p) - b;
				float3 q = abs(p + e) - e;
				return min(min(
					length(max(float3(p.x, q.y, q.z), 0.0)) + min(max(p.x, max(q.y, q.z)), 0.0),
					length(max(float3(q.x, p.y, q.z), 0.0)) + min(max(q.x, max(p.y, q.z)), 0.0)),
					length(max(float3(q.x, q.y, p.z), 0.0)) + min(max(q.x, max(q.y, p.z)), 0.0));
			}

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
				float4 world : TEXCOORD1;
				float3 scale : TEXCOORD2;

				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(appdata v)
			{
				UNITY_SETUP_INSTANCE_ID(v);

				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;

				o.scale = float3(length(unity_ObjectToWorld._m00_m10_m20),	length(unity_ObjectToWorld._m01_m11_m21), length(unity_ObjectToWorld._m02_m12_m22));
				o.world.xyz = v.vertex * o.scale;
				o.world.w = 1;

				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float sd = sdBoxFrame(i.world, i.scale, _LineSize);

				fixed4 col = _Color;

				float aaf = fwidth(sd);
				col.a = smoothstep(aaf, 0,  sd);


				//if (sd < 0 && sd >((fwidth(sd) - 1) * 2))
				//{
				//	col.a = 1;
				//}
				//else
				//{
				//	col.a = 0;
				//}

				return col;
			}
			ENDCG
		}
	}
}
