Shader "Custom/SpriteGlow"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _GlowColor ("Glow Color", Color) = (1, 0.5, 0, 1)
        _GlowStrength ("Glow Strength", Range(0, 10)) = 2
        _GlowWidth ("Glow Width", Range(0, 1)) = 0.2
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _GlowColor;
            float _GlowStrength;
            float _GlowWidth;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                float alpha = col.a;

                // Glow 영역 계산 (외곽에서 glowWidth 내로 점점 퍼지게)
                float glow = smoothstep(1.0, 1.0 - _GlowWidth, alpha);

                // GlowColor를 alpha 기반으로 감싸줌
                float glowIntensity = (1.0 - glow) * _GlowStrength;

                float3 resultRGB = col.rgb + _GlowColor.rgb * glowIntensity;
                return float4(resultRGB, col.a);
            }
            ENDCG
        }
    }
}