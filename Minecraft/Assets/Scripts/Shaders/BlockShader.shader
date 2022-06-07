Shader "Lorenc/BlockShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue" = "AlphaTest" "IgnoreProjector" = "True" "RenderType" = "TransparentCutout" }
        LOD 100
        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vertFunction
            #pragma fragment fragFunction
            #pragma target 2.0
            #pragma multi_compile_fog // unity fog support
           // #pragma multi_compile 

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float GlobalLightIntensity;

            v2f vertFunction (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;

                /*    float distance = length(ObjSpaceViewDir(v.vertex));
                    if (distance > 30)
                    {
                        o.vertex.y += (distance - 30) * 0.25;
                    } */

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 fragFunction(v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);


            //    float localLightLevel = clamp(GlobalLightIntensity + i.color.a, 0, 1);
              //  clip(col.a - 1); // wsparcie przezroczystosci
             //   col = lerp(col, float4(0, 0, 0, 1), localLightLevel); // swiatlo


                clip(col.a - 1); // wsparcie przezroczystosci

                float naturalLight = clamp(GlobalLightIntensity + i.color.a, 0, 0.975);
                float artificialLight = i.color.b;
                float finalLight = naturalLight > artificialLight ? artificialLight : naturalLight;

                float darknessPercentage = i.color.g;
                float darknessScale = abs(finalLight - 1);
                float finalDarkness = darknessPercentage / 100 * darknessScale;

                col = lerp(col, float4(0, 0, 0, 1), finalLight + finalDarkness);


                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
              //  UNITY_OPAQUE_ALPHA(col.a); // klyj testowanie niby cos z fogiem ale nwm
                return col;
            }
            ENDCG
        }
    }
}
