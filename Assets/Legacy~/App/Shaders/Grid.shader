Shader "LabLightAR/WorkSpaceGrid"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CenterX("CenterX", Float) = 0.5
        _CenterY("CenterY", Float) = 0.5 
        _ThresholdX("Threshold X", Float) = 0.5
        _ThresholdY("Threshold Y", Float) = 0.5
        _Edge("EdgeWidth", Float) = 0.01
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        //ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

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
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _CenterX;
            float _CenterY;
            float _ThresholdX;
            float _ThresholdY;
            float _Edge;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv1 = v.vertex.xz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
              UNITY_SETUP_INSTANCE_ID(i);
              float4 tex = tex2D(_MainTex, i.uv);

              float tX = smoothstep(_ThresholdX , _ThresholdX - _Edge, abs(i.uv1.x - _CenterX));
              float tY = smoothstep(_ThresholdY , _ThresholdY - _Edge, abs(i.uv1.y - _CenterY));
              float t = min(tX, tY);

              return tex * t;
            }
            ENDCG
        }
    }
}
