// 2D Foliage Wind Shader
// Compatible with both URP and Built-in rendering pipelines

Shader "Custom/2D Foliage Wind" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        
        [Space(10)]
        [Header(Wind Settings)]
        _WindSpeed ("Wind Speed", Range(0, 10)) = 1
        _WindStrength ("Wind Strength", Range(0, 0.1)) = 0.02
        _WindFrequency ("Wind Frequency", Range(0, 10)) = 2
        _WindDirection ("Wind Direction", Range(0, 360)) = 0
        
        [Space(10)]
        [Header(Plant Settings)]
        _GrassTopBend ("Top Bend Multiplier", Range(0, 5)) = 1
        _PivotOffset ("Pivot Offset (0=Bottom, 1=Top)", Range(0, 1)) = 0
        _RandomOffset ("Randomness", Range(0, 10)) = 1
        
        [Space(10)]
        [Header(Rendering Settings)]
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        
        [Toggle(USE_OBJECT_POSITION)] _UseObjectPosition("Use Position-based Randomization", Float) = 1
    }
    
    SubShader {
        Tags { 
            "Queue" = "Transparent" 
            "IgnoreProjector" = "True" 
            "RenderType" = "Transparent" 
            "PreviewType" = "Plane"
            "DisableBatching" = "True"
        }
        
        Stencil {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ UNITY_UI_CLIP_RECT
            #pragma shader_feature USE_OBJECT_POSITION
            #include "UnityCG.cginc"
            
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            
            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float4 worldPosition : TEXCOORD1;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _WindSpeed;
            float _WindStrength;
            float _WindFrequency;
            float _WindDirection;
            float _GrassTopBend;
            float _PivotOffset;
            float _RandomOffset;
            
            float4 _ClipRect;
            
            // Simple hash function
            float hash(float2 p) {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }
            
            v2f vert (appdata v) {
                v2f o;
                
                // Generate randomness for each plant/sprite
                float randomFactor = 1.0;
                
                #ifdef USE_OBJECT_POSITION
                    // World position based randomization - makes each plant move differently
                    float4 worldPos = mul(unity_ObjectToWorld, float4(0,0,0,1));
                    randomFactor = hash(worldPos.xz) * _RandomOffset;
                #endif
                
                // Convert wind direction to radians
                float windDir = _WindDirection * (3.14159265359 / 180.0);
                float2 windVec = float2(cos(windDir), sin(windDir));
                
                // Calculate min/max Y in object local coordinates (mesh bounding box)
                float minY = 0;  // This should be adjusted to match your actual mesh bounding box
                float maxY = 1;  // This should be adjusted to match your actual mesh bounding box
                
                // Calculate normalized height (0 = at pivot position, closer to 1 as you go up)
                // _PivotOffset allows adjusting the pivot position
                float pivotY = lerp(minY, maxY, _PivotOffset);
                float normalizedHeight = saturate((v.vertex.y - pivotY) / ((maxY - minY) + 0.001));
                
                // Time-based wind + random offset (slightly different timing for each plant)
                float windTime = (_Time.y + randomFactor) * _WindSpeed;
                
                // Effect gets stronger toward the top
                float windEffect = normalizedHeight * _GrassTopBend;
                
                // Only move vertices above the pivot
                if (v.vertex.y > pivotY) {
                    // Position-based oscillation + randomness
                    float posFactorX = v.vertex.x * _WindFrequency;
                    float windDisplacement = sin(windTime + posFactorX + randomFactor) * _WindStrength * windEffect;
                    
                    // Apply displacement in wind direction
                    v.vertex.xy += windVec * windDisplacement;
                }
                
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                
                #ifdef UNITY_UI_CLIP_RECT
                // Manual implementation of 2D clipping instead of using UnityGet2DClipping
                float2 clipPosition = i.worldPosition.xy;
                col.a *= step(_ClipRect.x, clipPosition.x) * step(clipPosition.x, _ClipRect.z) * 
                         step(_ClipRect.y, clipPosition.y) * step(clipPosition.y, _ClipRect.w);
                #endif
                
                return col;
            }
            ENDCG
        }
    }
    
    // Fallback to a simple transparent shader
    Fallback "Sprites/Default"
}