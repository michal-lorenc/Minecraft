Shader "Lorenc/WaterShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color("Color (RGBA)", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
     //   Cull off
        LOD 100
        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vertFunction alpha
            #pragma fragment fragFunction alpha
            #pragma target 2.0
            #pragma multi_compile_fog // unity fog support

            
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
            float4 _Color;

            v2f vertFunction(appdata v)
            {
                v2f o;

              //  v.vertex.y = v.vertex.y - 0.2;
                o.vertex = UnityObjectToClipPos(v.vertex);

                o.uv = v.uv;
                o.uv = TRANSFORM_TEX(v.uv.xy, _MainTex);

                o.uv.x = o.uv.x * _Time;

                o.color = v.color;

            /*    float distance = length(ObjSpaceViewDir(v.vertex));
                if (distance > 30)
                {
                    o.vertex.y += (distance - 30) * 0.25;
                } */
          //      o.color.a = 0.5;
             //   o.Alpha = tex2D(_MainTex, IN.uv_MainTex).a;

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 fragFunction(v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

                float naturalLight = clamp(GlobalLightIntensity + i.color.a, 0, 0.975);
                float artificialLight = i.color.b;
                float finalLight = naturalLight > artificialLight ? artificialLight : naturalLight;

                float darknessPercentage = i.color.g;
                float darknessScale = abs(finalLight - 1);
                float finalDarkness = darknessPercentage / 100 * darknessScale;

                col = lerp(col, float4(0, 0, 0, 1), finalLight + finalDarkness);
                col *= _Color;

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
              }
              ENDCG
          }
    }
}
