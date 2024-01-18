// Adaption of Ben Golus MobileVRHighlightShader
// https://gist.github.com/bgolus/c67c29c218ec1768a770ca6966fa0765
//
// Since this shader does a ZTest always it will not respect scene depth. The final result is more like a screenspace overlay.
//
// 5 pass rendering to stencil buffer
// Disabled the occluded pass that render things behind 3d geometry as different color
// Pulsating between color and color faded
// Fixed pixel width outline
Shader "LabLightAR\StencilOutline" 
{
    Properties
    {
        _ColorOutline("Outline", Color) = (1,1,1,1)
        _ColorInterior("Interior", Color) = (0.25,0.25,0.25,0.25)
        _ColorInteriorFaded("Interior Faded", Color) = (0.1,0.1,0.1,0.1)
//        _ColorInteriorOcc("Interior Occluded", Color) = (0.15,0.15,0.15,0.15)
//        _ColorInteriorOccFaded("Interior Occluded Faded", Color) = (0.05,0.05,0.05,0.05)
        _PulseRateMod("Pulse Rate Modifier", Float) = 4.0
        _OutlneWidth("Outline Pixel Width", Float) = 1.0
    }

    Subshader
    {
    
        Tags{ "Queue" = "Overlay" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

        ZWrite off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        Offset -0.5, 0

        Stencil {
            ReadMask 2
            WriteMask 2
            Ref 2
            Comp NotEqual
            Pass Replace
        }

        CGINCLUDE
            #include "UnityCG.cginc"

            struct v2f
            {
                float4 pos : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };


            v2f vertDefault(appdata_base v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            float _OutlneWidth;

            v2f vertOutline(appdata_base v, float2 offsetDir)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = UnityObjectToClipPos(v.vertex);

                o.pos.xy += (offsetDir * 2.0) / _ScreenParams.xy * o.pos.w * _OutlneWidth;

                return o;
            }

            float _PulseRateMod;
            fixed4 pulse(fixed4 a, fixed4 b) { return lerp(a, b, saturate(sin(_Time.y * _PulseRateMod) * 0.5 + 0.5)); }

            fixed4 _ColorOutline;

            fixed4 fragOutline(v2f i) : SV_Target
            {
                return _ColorOutline;
            }
        ENDCG

        Pass 
        {
            Name "Unoccluded Fill"
            //ZTest LEqual

            CGPROGRAM
            #pragma vertex vertDefault
            #pragma fragment frag

            #pragma multi_compile_instancing
            fixed4 _ColorInterior, _ColorInteriorFaded;

            fixed4 frag(v2f i) : SV_Target
            {
                return pulse(_ColorInterior, _ColorInteriorFaded);
            }
            ENDCG
        }

        //Pass 
        //{
        //    Name "Occluded Fill"

        //    CGPROGRAM
        //    #pragma vertex vertDefault
        //    #pragma fragment frag

        //    #pragma multi_compile_instancing

        //    fixed4 _ColorInteriorOcc, _ColorInteriorOccFaded;

        //    fixed4 frag(v2f i) : SV_Target
        //    {
        //        return pulse(_ColorInteriorOcc, _ColorInteriorOccFaded);
        //    }
        //    ENDCG
        //}

        Pass 
        {
            Name "Outline UR"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragOutline

            #pragma multi_compile_instancing

            v2f vert(appdata_base v)
            {
                return vertOutline(v, float2(1, 1));
            }
            ENDCG
        }

        Pass 
        {
            Name "Outline UL"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragOutline

            #pragma multi_compile_instancing

            v2f vert(appdata_base v)
            {
                return vertOutline(v, float2(-1, 1));
            }
            ENDCG
        }

        Pass 
        {
            Name "Outline DR"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragOutline

            #pragma multi_compile_instancing

            v2f vert(appdata_base v)
            {
                return vertOutline(v, float2(1,-1));
            }
            ENDCG
        }

        Pass 
        {
            Name "Outline DL"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragOutline

            #pragma multi_compile_instancing

            v2f vert(appdata_base v)
            {
                return vertOutline(v, float2(-1,-1));
            }
            ENDCG
        }
    }
}