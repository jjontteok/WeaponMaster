Shader "Custom/PlayerCenteredVignette"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _VignetteColor ("Vignette Color", Color) = (0, 0, 0, 1)
        _PlayerPos ("Player Position", Vector) = (0.5, 0.5, 0, 0)
        _Radius ("Radius", Range(0, 1)) = 0.5
        _Softness ("Softness", Range(0, 1)) = 0.5
        _Darkness ("Darkness", Range(0, 1)) = 0.5
        _OrthoSize ("Orthographic Size", Float) = 5.0
        _BaseOrthoSize ("Base Orthographic Size", Float) = 5.0
        _UseColor ("Use Color Instead of Texture", Float) = 0
    }
    SubShader
    {
        // UI보다 먼저 렌더링되도록 설정 (UI는 보통 Overlay 큐(4000)에 있음)
        // Transparent+10 (3010)은 UI 요소(4000)보다 먼저 렌더링됨
        Tags { "Queue"="Transparent+10" "RenderType"="Transparent" "PreviewType"="Plane" }
        LOD 100

        Pass
        {
            ZTest Always
            ZWrite Off
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _VignetteColor;
            float4 _PlayerPos;
            float _Radius;
            float _Softness;
            float _Darkness;
            float _OrthoSize;
            float _BaseOrthoSize;
            float _UseColor;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // 항상 원본 텍스처를 가져오기
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // 현재 픽셀과 플레이어 위치 사이의 거리 계산
                float dist = distance(i.uv, _PlayerPos.xy);
                
                // 카메라 줌에 따른 스케일 조정 계수
                float zoomScale = _OrthoSize / _BaseOrthoSize;
                
                // 거리에 줌 스케일 적용
                float scaledDist = dist * (zoomScale > 0 ? zoomScale : 1.0);
                
                // 거리에 따라 어두워지는 정도 계산
                float vignetteAmount = smoothstep(_Radius, _Radius + max(_Softness, 0.001), scaledDist);
                
                // 최종 색상 적용
                if (_UseColor > 0.5) {
                    // 단색 모드: 가장자리에만 선택한 색상의 비네트 효과 적용
                    col.rgb = lerp(col.rgb, _VignetteColor.rgb, vignetteAmount * _Darkness);
                } else {
                    // 일반 모드: 가장자리만 어둡게
                    col.rgb *= 1.0 - (vignetteAmount * _Darkness);
                }
                
                return col;
            }
            ENDCG
        }
    }
    // 에디터 전용 대체 셰이더 - 미리보기가 작동하지 않을 경우를 위한 대비책
    Fallback "Unlit/Texture"
}