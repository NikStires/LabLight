Shader "Unlit/EdgeGlow"
{
    Properties
    {
        _Color("Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _InnerGlowPower("Inner Glow Power", Range(2.0, 32.0)) = 4.0
    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Front
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
                float4 normal : NORMAL;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 _Color;
            fixed _InnerGlowPower;

            v2f vert (appdata v)
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
                // sample the texture
                fixed4 col = _Color;

                fixed2 distanceToEdge;
                distanceToEdge.x = abs(i.uv.x - 0.5) * 2.0;
                distanceToEdge.y = abs(i.uv.y - 0.5) * 2.0;

                fixed2 uvGlow = pow(distanceToEdge * _Color.a, _InnerGlowPower);
                col.a = uvGlow.x + uvGlow.y;

                return col;
            }
            ENDCG
        }
    }
}
