Shader "Custom/AlwaysOnTop"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Overlay"  // 항상 위로 렌더링
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }

        Pass
        {
            ZTest Always           // 항상 렌더링 (Depth Test 무시)
            ZWrite Off             // Depth Buffer에 기록하지 않음
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off               // 양면 렌더링

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.texcoord) * _Color;
                return col;
            }
            ENDCG
        }
    }

    FallBack "UI/Default"
}
