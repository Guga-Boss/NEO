Shader "Custom/NightGlobalWithMask"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _LightMask ("Light Mask", 2D) = "black" {}

        _NightIntensity ("Night Intensity", Range(0,1)) = 0.8
        _DayLight ("Day Light", Range(0,1)) = 0.5
        _DarkColor ("Dark Color", Color) = (0.2, 0.3, 0.5, 1)

        _MasterColor ("Master Color", Color) = (1,1,1,1)
        _MasterIntensity ("Master Intensity", Range(0,5)) = 1
        [IntRange] _BlendMode ("Blend Mode", Range(0,7)) = 0

        _BloomThreshold ("Bloom Threshold", Range(0,3)) = 1.5
        _BloomIntensity ("Bloom Intensity", Range(0,5)) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            ZTest Always
            Cull Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _LightMask;

            float _NightIntensity;
            float _DayLight;
            fixed4 _DarkColor;
            fixed4 _MasterColor;
            float _MasterIntensity;
            int _BlendMode;
            float _BloomThreshold;
            float _BloomIntensity;

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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed3 BlendMultiply(fixed3 base, fixed3 blend) { return base * blend; }
            fixed3 BlendAdditive(fixed3 base, fixed3 blend) { return base + blend; }
            fixed3 BlendScreen(fixed3 base, fixed3 blend) { return 1.0 - (1.0 - base) * (1.0 - blend); }
            fixed3 BlendOverlay(fixed3 base, fixed3 blend)
            {
                return lerp(2.0 * base * blend, 1.0 - 2.0 * (1.0 - base) * (1.0 - blend), step(0.5, base));
            }
            fixed3 BlendSoftLight(fixed3 base, fixed3 blend)
            {
                return (blend <= 0.5) ? base - (1.0 - 2.0 * blend) * base * (1.0 - base)
                                      : base + (2.0 * blend - 1.0) * (sqrt(base) - base);
            }
            fixed3 BlendColorDodge(fixed3 base, fixed3 blend)
            {
                return saturate(base / max(1.0 - blend, 0.001));
            }
            fixed3 BlendDifference(fixed3 base, fixed3 blend)
            {
                return abs(base - blend);
            }

            fixed3 ApplyBlendMode(fixed3 base, fixed3 blend, int mode)
            {
                if (mode == 1) return BlendMultiply(base, blend);
                if (mode == 2) return BlendAdditive(base, blend);
                if (mode == 3) return BlendScreen(base, blend);
                if (mode == 4) return BlendOverlay(base, blend);
                if (mode == 5) return BlendSoftLight(base, blend);
                if (mode == 6) return BlendColorDodge(base, blend);
                if (mode == 7) return BlendDifference(base, blend);
                return base;
            }

            fixed4 frag(v2f i) : COLOR
            {
                fixed4 baseCol = tex2D(_MainTex, i.uv);
                fixed4 mask = tex2D(_LightMask, i.uv);

                float nightStrength = _NightIntensity * (1.0 - _DayLight);
                float darkness = pow(nightStrength * (1.0 - mask.a), 1.5);
                fixed3 darkened = lerp(baseCol.rgb, _DarkColor.rgb, saturate(darkness));

                fixed3 masterEffect = _MasterColor.rgb * _MasterIntensity;
                fixed3 blended = ApplyBlendMode(darkened, masterEffect, _BlendMode);

                fixed3 lit = blended + mask.rgb;

                float bloomFactor = max(0, dot(lit, fixed3(0.3, 0.59, 0.11)) - _BloomThreshold);
                fixed3 bloom = bloomFactor * bloomFactor * _BloomIntensity;
                fixed3 finalColor = lit + bloom;

                return fixed4(saturate(finalColor), baseCol.a * _MasterColor.a);
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}
