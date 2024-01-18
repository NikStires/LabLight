// Traditional two pass shell based outline
// Depth prepass can be done in a shader, but it is preferred to do it as material pass so order can be controlled between different objects

Shader "LabLightAR/WireOutline"
{
	Properties
	{
		_WireColor("Wire color", Color) = (1.0, 1.0, 1.0, 1.0)
		_Amount("Wire Thickness", float) = 0.01
		_Fade("Fade", float) = 0.01
		[Enum(UnityEngine.Rendering.CullMode)] _CullMode("Cull Mode", Float) = 2
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent+1" }

		Pass
		{
			Cull [_CullMode]
			ColorMask 0

			CGPROGRAM
			#include "UnityCG.cginc" 

			#pragma vertex vert
			#pragma fragment frag

			fixed _Amount;

			struct v2f
			{
				fixed4 viewPos : SV_POSITION;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(appdata_base v)
			{
				UNITY_SETUP_INSTANCE_ID(v);
				v2f o;
				o.viewPos = UnityObjectToClipPos(v.vertex);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				return 0;
			}
			ENDCG
		}

		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent-1" }


		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency
			Cull Front

			CGPROGRAM
			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag

			fixed4 _WireColor;
			fixed _Amount;
			float _Fade;

			struct v2f
			{
				fixed4 extrudedviewPos : SV_POSITION;
				fixed4 origViewPos : TEXCOORD0;
				fixed4 origScreenPos : TEXCOORD1;
				fixed4 extrudedScreenPos : TEXCOORD2;
				fixed2 offset : TEXCOORD3;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(appdata_base v)
			{
				UNITY_SETUP_INSTANCE_ID(v);
				v2f o;

				o.origViewPos = UnityObjectToClipPos(v.vertex);
				o.extrudedviewPos = UnityObjectToClipPos(v.vertex);//  UnityObjectToClipPos(v.vertex + normalize(v.normal) * _Amount);

				o.origScreenPos = ComputeScreenPos(o.origViewPos);
				
				// Offset along normal in projection space
				float3 norm = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal));
				o.offset = TransformViewToProjection(norm.xy);

				o.extrudedviewPos.xy += o.offset * _Amount;
				o.extrudedScreenPos = ComputeScreenPos(o.extrudedviewPos);

				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				float2 origScreenUV = i.origScreenPos.xy / i.origScreenPos.w;
				float2 extrudedScreenUV = i.extrudedScreenPos.xy / i.extrudedScreenPos.w;

				//float2 dir = normalize(extrudedScreenUV - origScreenUV);

				float d = distance(origScreenUV, extrudedScreenUV);

				//return float4(float2(0.5, .5) * (dir.xy + float2(1, 1)) , 0, 1);
				//return _WireColor* d * 100;
				//return float4(i.offset.xy *.5 +.5, 0, 1);

				//fixed2 fw = fwidth(i.offset);
				//fixed fade = max(fw.x, fw.y);

				//return  (1-fade) * _WireColor;

				//*= _Fade;

				float4 color = smoothstep(_Fade + fwidth(d), _Fade - fwidth(d), d);

				return color;// float4(d, d, d, 1);
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}

