Shader "UI/RadialWipe"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Progress ("Progress", Range(0, 1)) = 0
        _SoftEdge ("Soft Edge", Range(0, 0.5)) = 0.05
        _AspectRatio ("Aspect Ratio", Float) = 1.0  // Default to 1:1 aspect ratio
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                half2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            fixed4 _Color;
            fixed _Progress;
            fixed _SoftEdge;
            float _AspectRatio;
            
            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.worldPosition = IN.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Calculate position from center (0 to 1)
                half2 center = half2(0.5, 0.5);
                half2 uvFromCenter = IN.texcoord.xy - center;
                
                // Correct for aspect ratio to make a true circle
                uvFromCenter.x *= _AspectRatio;
                
                // Calculate distance from center (0 to ~0.707 for corner with aspect correction)
                float dist = length(uvFromCenter);
                
                // Normalize to 0-1 range - calculate max distance for normalization
                // This will change based on aspect ratio
                float maxDist = length(half2(0.5 * _AspectRatio, 0.5));
                dist = dist / maxDist;
                
                // Calculate alpha based on distance and progress
                float alpha = 1.0;
                
                if (_Progress > 0)
                {
                    // Scale progress to account for soft edge
                    float scaledProgress = _Progress * (1.0 + _SoftEdge) - _SoftEdge;
                    
                    // Apply soft edge
                    alpha = smoothstep(scaledProgress - _SoftEdge, scaledProgress + _SoftEdge, dist);
                }
                
                return IN.color * fixed4(1,1,1,alpha);
            }
            ENDCG
        }
    }
}