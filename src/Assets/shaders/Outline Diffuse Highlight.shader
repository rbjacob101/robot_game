﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "OutlinedHighlight" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _RimCol ("Rim Colour" , Color) = (1,0,0,1)
        _RimPow ("Rim Power", Float) = 0.333333333
        _Alpha ("Alpha", Range(0,1)) = 1.0
    }
    SubShader {
        Pass {
                Name "Behind"
                Tags { "RenderType"="transparent" "Queue" = "Transparent" }
                Blend SrcAlpha OneMinusSrcAlpha
                ZTest Always              // here the check is for the pixel being greater or closer to the camera, in which
                Cull Off                   // case the model is behind something, so this pass runs
                ZWrite Off
                LOD 100                    
               
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"
               
                struct v2f {
                    float4 pos : SV_POSITION;
                    float2 uv : TEXCOORD0;
                    float3 normal : TEXCOORD1;      // Normal needed for rim lighting
                    float3 viewDir : TEXCOORD2;     // as is view direction.
                };
               
                sampler2D _MainTex;
                float4 _RimCol;
                float _RimPow;
               
                float4 _MainTex_ST;
               
                v2f vert (appdata_tan v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                    o.normal = normalize(v.normal);
                    o.viewDir = normalize(ObjSpaceViewDir(v.vertex));       //this could also be WorldSpaceViewDir, which would
                    return o;                                               //return the World space view direction.
                }
               
                half4 frag (v2f i) : COLOR
                {
                    half Rim = 1 - saturate(dot(normalize(i.viewDir), i.normal));       //Calculates where the model view falloff is       
                                                                                                                                       //for rim lighting.
                   
                    half4 RimOut = _RimCol * _RimPow;
                    return RimOut;
                }
                ENDCG
            }
        }
    FallBack "VertexLit"
}