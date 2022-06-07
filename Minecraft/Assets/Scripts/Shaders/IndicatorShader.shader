Shader "Lorenc/IndicatorShader"
{
    Properties
    {
        _MainTex ("Outline Texture", 2D) = "white" {}
        _Color("Outline Color (RGBA)", Color) = (1, 1, 1, 1)
        _ProgressTex("Progress Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100
        Lighting Off
        Offset -1, -1

        Pass
        {
            CGPROGRAM
            #pragma vertex vert alpha
            #pragma fragment frag alpha

            #include "UnityCG.cginc"


            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            sampler2D _ProgressTex;
            float4 _ProgressTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _ProgressTex);

                o.uv2 = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv2) * _Color;
                fixed4 secondCol = tex2D(_ProgressTex, i.uv);

                fixed4 finalCol = lerp(secondCol, col, col.a); //col *= _ProgressTex;
                return finalCol;
            }
            ENDCG
        }
    }
}
