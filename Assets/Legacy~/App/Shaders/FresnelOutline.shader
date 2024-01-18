Shader "LabLightAR/FresnelOutline"
{
	Properties
	{
		_Color("Color", Color) = (0.26,0.19,0.16,0.0)
		_Offset("Offset", Range(0.0,1.0)) = 0.0
		_Scale("Scale", Range(0.0,10.0)) = 1.0
		_RimPower("Rim Power", Range(0.1,8.0)) = 3.0
		_Amount("Wire Thickness", float) = 0.01
	}

		SubShader{
			Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent-1" }
			Blend OneMinusDstColor One

			Pass
			{
				Cull Back

				CGPROGRAM
				#include "UnityCG.cginc"

				#pragma vertex vert
				#pragma fragment frag

				fixed4 _Color;
				fixed _Offset;
				fixed _Scale;
				fixed _RimPower;
				fixed _Amount;

				struct v2f
				{
					fixed4 viewPos : SV_POSITION;
					fixed3 normal : NORMAL;
					fixed3 worldSpaceViewDir : TEXCOORD0;
					UNITY_VERTEX_OUTPUT_STEREO
				};

				v2f vert(appdata_base v)
				{
					UNITY_SETUP_INSTANCE_ID(v);
					v2f o;
					o.viewPos = UnityObjectToClipPos(v.vertex + normalize(v.normal) * _Amount);
					o.worldSpaceViewDir = WorldSpaceViewDir(v.vertex);
					o.normal = mul(unity_ObjectToWorld, fixed4(v.normal, 0.0)).xyz;
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					return o;
				}

				fixed4 frag(v2f i) : COLOR
				{
					fixed4 o = 1;
					half rim = 1.0 - saturate(dot(normalize(i.worldSpaceViewDir), normalize(i.normal)));
					return _Color * (_Offset + _Scale * pow(rim, _RimPower));
				}
				ENDCG
			}
		}
		FallBack "Diffuse"
}
