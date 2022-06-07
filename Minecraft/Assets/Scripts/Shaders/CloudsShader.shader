// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Lorenc/CloudsShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color (RGBA)", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100
        Lighting Off

        Pass
        {
            CGPROGRAM
// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members pos)
//#pragma exclude_renderers d3d11
            #pragma vertex vertFunction alpha
            #pragma fragment fragFunction alpha
            #pragma target 2.0

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            //    float3 pos;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
             //   float3 pos;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float GlobalLightIntensity;
            float4 _Color;

            v2f vertFunction (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);

                o.uv = v.uv;
                o.color = v.color;

                return o;
            }

            fixed4 fragFunction(v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                float localLightLevel = clamp(GlobalLightIntensity + i.color.a, 0, 0.9375);
              //  clip(col.a - 1); // wsparcie przezroczystosci
                col = lerp(col, float4(0, 0, 0, 1), localLightLevel); // swiatlo
                //col *= _Color;

         /*       float distanced = length(ObjSpaceViewDir(i.vertex));
           //     float3 distanced = distance(_WorldSpaceCameraPos, UnityObjectToViewPos(i.vertex));
                if (distanced > 1000)
                {
                //    _Color.a -= (distanced - 200000) * 0.000001;
                    _Color.a = 0;
                    if (_Color.a < 0)
                    {
                        _Color.a = 0;
                    }
                } */

                float beforeMaxAlpha = _Color.a;
                float cameraDist = length(i.posWorld.xyz - _WorldSpaceCameraPos.xyz);
                _Color.a = _Color.a / (cameraDist * 0.003);

                _Color.a = clamp(_Color.a, 0, beforeMaxAlpha);

                col *= _Color;
                // apply fog
              //  UNITY_APPLY_FOG(i.fogCoord, col);
              //  UNITY_OPAQUE_ALPHA(col.a); // klyj testowanie niby cos z fogiem ale nwm
                return col;
            }
            ENDCG
        }
    }
}
