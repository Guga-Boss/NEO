Shader "Custom/DrawLightCircleFixed"
{
    Properties {
        _Center ("Center", Vector) = (0.5, 0.5, 0, 0)
        _Radius ("Radius", Float) = 0.1
        _Intensity ("Intensity", Float) = 1.0
        _InnerColor ("Inner Color", Color) = (1, 0.5, 0.3, 1)
        _OuterColor ("Outer Color", Color) = (0.2, 0.4, 1, 1)
        _ColorBlend ("Color Blend", Range(0,1)) = 0.5
        _FalloffCurve ("Falloff", Range(1,10)) = 3.0
    }

    SubShader {
        Tags { "RenderType"="Transparent" }
        Pass {
            ZTest Always Cull Off ZWrite Off
            Blend One One // usamos soma, pois o resultado é uma máscara acumulativa

            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _Center;
            float _Radius;
            float _Intensity;
            fixed4 _InnerColor;
            fixed4 _OuterColor;
            float _ColorBlend;
            float _FalloffCurve;

            fixed4 frag(v2f_img i) : SV_Target {
                float2 uv = i.uv;
                float2 center = _Center.xy;
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 adjustedUV = (uv - center) * float2(aspect, 1.0);

                float dist = length(adjustedUV);
                float normalizedDist = saturate(dist / _Radius);

                float falloff = pow(saturate(1.0 - normalizedDist), _FalloffCurve);
                float intensity = falloff * _Intensity;

                fixed3 finalColor = lerp(_InnerColor.rgb, _OuterColor.rgb, normalizedDist * _ColorBlend);

                return fixed4(finalColor * intensity, intensity);
            }
            ENDCG
        }
    }
}
