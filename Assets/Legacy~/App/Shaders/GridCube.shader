// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "LabLightAR/GridCube"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        _Progress("Progress", Range(0,1)) = 0
        _Range("Quad Scale Range", Range(0,1)) = .1
        _DistanceY("Height Offset", Range(0,1)) = .5
        _RangeY("Height Range", Range(0,1)) = .1
        _FillDistance("FillDistance", Range(0,1)) = .5
        _FillRange("RillRange", Range(0,1)) = .5
    }
    SubShader
    {
        Cull Back
        ZWrite On
        ZTest Always
        Blend OneMinusDstColor One // Soft Additive

        Tags { "RenderType" = "Transparent"  "Queue" = "Transparent" }

        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 uv2 : TEXCOORD2;

                UNITY_VERTEX_INPUT_INSTANCE_ID 
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_OUTPUT_STEREO 
            };

            float4 _Color;
            float4 _CenterPosition;
            
            float _Progress;
            float _Range;
            float _DistanceY;
            float _RangeY;

            float _FillDistance;
            float _FillRange;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float3 localPivot = v.uv2 - float3(0,0.05,-0.1);
                // Pivot in world coordinate
                float3 worldPivot = mul(unity_ObjectToWorld, float4(v.uv2 - float3(0, 0.0, -0.1), 1));
                // Distance to world center
                float d = distance(worldPivot, _CenterPosition);

                // Exponential step to generate a bell curve fallof across distance
                // Bell curve is shifted upward by Progress
                // https://www.iquilezles.org/www/articles/functions/functions.htm
                float height = exp(-pow(d, 2)) + lerp(-1, 1, _Progress);

                // Hard on/off scale switch
                //float scale = step(pivot.y, height);

                // XZ quad scale
                float scale = smoothstep(worldPivot.y , worldPivot.y + _Range, height);
                float scaleY = smoothstep(worldPivot.y , worldPivot.y + _RangeY, height - _DistanceY);

                o.vertex = UnityObjectToClipPos((v.vertex - localPivot) * float3(scale, scaleY, scale) + localPivot);
                o.uv = 2 * (v.uv - 0.5);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                i.uv = abs(i.uv);
                float d = max(i.uv.x, i.uv.y);
                fixed4 col = _Color * smoothstep(_FillDistance - _FillRange, _FillDistance + _FillRange, d);
                return col;
            }
            ENDCG
        }
    }
}
