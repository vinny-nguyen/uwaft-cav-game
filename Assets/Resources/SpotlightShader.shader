Shader "UI/Spotlight" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (0,0,0,0.5)
        _Center ("Center", Vector) = (0.5,0.5,0,0)
        _Radius ("Radius", Float) = 0.2
        _SoftEdge ("Soft Edge", Float) = 0.1
        _AspectRatio ("Aspect Ratio", Float) = 1.0
    }
    
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Always
        
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            float4 _Color;
            float2 _Center;
            float _Radius;
            float _SoftEdge;
            float _AspectRatio;
            
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target {
                // Apply aspect ratio correction to create a true circle
                float2 correctedUV = float2(i.uv.x * _AspectRatio, i.uv.y);
                float2 correctedCenter = float2(_Center.x * _AspectRatio, _Center.y);
                
                // Calculate distance with aspect ratio correction
                float dist = distance(correctedUV, correctedCenter) / _AspectRatio;
                
                float circle = 1 - smoothstep(_Radius - _SoftEdge, _Radius, dist);
                fixed4 col = _Color;
                col.a *= (1 - circle);
                return col;
            }
            ENDCG
        }
    }
}