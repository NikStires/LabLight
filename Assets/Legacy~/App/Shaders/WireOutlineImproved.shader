// Improvements
// Clip space width
Shader "LabLightAR/WireOutlineImproved"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        _OutlineColor("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth("Outline Width", float) = 0.03
    }

    Subshader
    {
        //Tags { "RenderType" = "Opaque"   }
        Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }

        Pass
        {
            Cull Front

            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #include "UnityCG.cginc"

            #pragma vertex vert
            #pragma fragment frag

            half _OutlineWidth;

            struct v2f
            {
                fixed4 clipPosition : SV_POSITION;
                fixed2 offset : TEXCOORD1;
                float l : VALUE;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata_base v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                v2f o;

                float4 clipPosition = UnityObjectToClipPos(v.vertex);
                float3 clipNormal = mul((float3x3) UNITY_MATRIX_VP, mul((float3x3) UNITY_MATRIX_M, v.normal));

                if (false)
                {
                    // Fixed width pixels
                    float2 offset = normalize(clipNormal.xy) * _OutlineWidth * clipPosition.w;
                    float2 offsetNDC = offset / _ScreenParams.xy * 2;
                    clipPosition.xy += offsetNDC;
                    o.offset = offset;
                }
                else
                {
                    // Variable width
                    float2 offset = normalize(clipNormal.xy) * _OutlineWidth;
                    float2 offsetNDC = offset / _ScreenParams.xy * 2;
                    clipPosition.xy += offsetNDC;
                    o.offset = offset;
                    o.l = length(offset);
                }

                o.clipPosition = clipPosition;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                return o;
            }

            half4 _OutlineColor;

            half4 frag(v2f i) : COLOR
            {                
                //float2 fw = fwidth(i.l);
                //float a = 1-fw;

                float a = saturate(i.l / fwidth(i.l * 2.0) + 0.5);

                return half4(a,a,a,1);
            }

            ENDCG
        }
    }
}
