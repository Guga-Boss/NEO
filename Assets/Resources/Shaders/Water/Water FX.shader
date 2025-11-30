Shader "Custom/WaterDroplets_50"
{
    Properties
    {
        [PerRendererData] _MainTex ("Base Texture", 2D) = "white" {}
        _WaterTex ("Water Texture", 2D) = "white" {}
		_LavaTex ("Lava Texture", 2D) = "white" {}

        _BlueThreshold ("Blue Threshold", Range(0,1)) = 0.7
        _WaterScale ("Water Scale", Float) = 1.0
        _WaterColor ("Shallow Color", Color) = (0.3,0.5,0.8,1)
        _DeepWaterColor ("Deep Color", Color) = (0.1,0.2,0.4,1)
        _ColorIntensity ("Color Intensity", Range(0,2)) = 1.0

        _WaveSpeed ("Wave Speed", Range(0,5)) = 1.0
        _WaveIntensity ("Wave Intensity", Range(0,0.5)) = 0.05
        _WaveAngle ("Wave Angle", Range(0,180)) = 45
        _WaveFrequency ("Wave Frequency", Range(1,50)) = 20

        _DropletSize ("Droplet Size", Float) = 0.5
        _DropletIntensity ("Droplet Intensity", Range(0,2)) = 1.0
        _DropletSpeed ("Droplet Wave Speed", Float) = 5.0
        _DropletLifetime ("Droplet Lifetime", Float) = 2.0
        _DropletCount ("Droplet Count", Int) = 0

        // Adição do reflexo do sol:
        _SunReflection ("Sun Intensity", Range(0,1)) = 0.5
        _ReflectionColor ("Sun Color", Color) = (1,0.9,0.7,1)
        _ReflectionSharpness ("Sharpness", Range(1,50)) = 15
        _SunDirection ("Sun Direction", Vector) = (0.3, 0.8, 0.5, 0)

        _RedThreshold ("Red Threshold", Range(0,1)) = 0.7
        _LavaScale ("Lava Scale", Float) = 1.0
        _LavaColor ("Shallow Lava Color", Color) = (0.3,0.5,0.8,1)
        _DeepLavaColor ("Deep Lava Color", Color) = (0.1,0.2,0.4,1)
        _LavaColorIntensity ("Lava Color Intensity", Range(0,2)) = 1.0
	    _LavaWaveSpeed ("Lava Wave Speed", Range(0,5)) = 1.0
        _LavaWaveIntensity ("Lava Wave Intensity", Range(0,0.5)) = 0.05
        _LavaWaveAngle ("Lava Wave Angle", Range(0,180)) = 45
        _LavaWaveFrequency ("Lava Wave Frequency", Range(1,50)) = 20
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _WaterTex;
			sampler2D _LavaTex;

            float _BlueThreshold, _WaterScale;
            float4 _WaterColor, _DeepWaterColor;
            float _ColorIntensity;

            float _WaveSpeed, _WaveIntensity, _WaveAngle, _WaveFrequency;

            float _DropletSize, _DropletIntensity, _DropletSpeed, _DropletLifetime;
            int _DropletCount;

            // Variáveis adicionadas para reflexo do sol:
            float _SunReflection;
            float4 _ReflectionColor;
            float4 _SunDirection;
            float _ReflectionSharpness;

            // Slots gerados dinamicamente (até 50)
            float4 _DropletPositions_0;  float _DropletTimes_0;
            float4 _DropletPositions_1;  float _DropletTimes_1;
            float4 _DropletPositions_2;  float _DropletTimes_2;
            float4 _DropletPositions_3;  float _DropletTimes_3;
            float4 _DropletPositions_4;  float _DropletTimes_4;
            float4 _DropletPositions_5;  float _DropletTimes_5;
            float4 _DropletPositions_6;  float _DropletTimes_6;
            float4 _DropletPositions_7;  float _DropletTimes_7;
            float4 _DropletPositions_8;  float _DropletTimes_8;
            float4 _DropletPositions_9;  float _DropletTimes_9;
            float4 _DropletPositions_10; float _DropletTimes_10;
            float4 _DropletPositions_11; float _DropletTimes_11;
            float4 _DropletPositions_12; float _DropletTimes_12;
            float4 _DropletPositions_13; float _DropletTimes_13;
            float4 _DropletPositions_14; float _DropletTimes_14;
            float4 _DropletPositions_15; float _DropletTimes_15;
            float4 _DropletPositions_16; float _DropletTimes_16;
            float4 _DropletPositions_17; float _DropletTimes_17;
            float4 _DropletPositions_18; float _DropletTimes_18;
            float4 _DropletPositions_19; float _DropletTimes_19;
            float4 _DropletPositions_20; float _DropletTimes_20;
            float4 _DropletPositions_21; float _DropletTimes_21;
            float4 _DropletPositions_22; float _DropletTimes_22;
            float4 _DropletPositions_23; float _DropletTimes_23;
            float4 _DropletPositions_24; float _DropletTimes_24;
            float4 _DropletPositions_25; float _DropletTimes_25;
            float4 _DropletPositions_26; float _DropletTimes_26;
            float4 _DropletPositions_27; float _DropletTimes_27;
            float4 _DropletPositions_28; float _DropletTimes_28;
            float4 _DropletPositions_29; float _DropletTimes_29;
            float4 _DropletPositions_30; float _DropletTimes_30;
            float4 _DropletPositions_31; float _DropletTimes_31;
            float4 _DropletPositions_32; float _DropletTimes_32;
            float4 _DropletPositions_33; float _DropletTimes_33;
            float4 _DropletPositions_34; float _DropletTimes_34;
            float4 _DropletPositions_35; float _DropletTimes_35;
            float4 _DropletPositions_36; float _DropletTimes_36;
            float4 _DropletPositions_37; float _DropletTimes_37;
            float4 _DropletPositions_38; float _DropletTimes_38;
            float4 _DropletPositions_39; float _DropletTimes_39;
            float4 _DropletPositions_40; float _DropletTimes_40;
            float4 _DropletPositions_41; float _DropletTimes_41;
            float4 _DropletPositions_42; float _DropletTimes_42;
            float4 _DropletPositions_43; float _DropletTimes_43;
            float4 _DropletPositions_44; float _DropletTimes_44;
            float4 _DropletPositions_45; float _DropletTimes_45;
            float4 _DropletPositions_46; float _DropletTimes_46;
            float4 _DropletPositions_47; float _DropletTimes_47;
            float4 _DropletPositions_48; float _DropletTimes_48;
            float4 _DropletPositions_49; float _DropletTimes_49;	

		    float _RedThreshold, _LavaScale;
            float4 _LavaColor, _DeepLavaColor;
            float _LavaColorIntensity;

			float _LavaWaveSpeed, _LavaWaveIntensity, _LavaWaveAngle, _LavaWaveFrequency;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 viewDir : TEXCOORD2; // adicionado para reflexo do sol
            };

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = normalize(_WorldSpaceCameraPos - o.worldPos); // adicionado para reflexo do sol
                return o;
            }

            void ProcessDroplet(float4 pos, float time, float2 worldPos, inout float total) {
                if (time < 0 || (pos.w < 0.5 && time > _DropletLifetime)) return;
                float2 dropletPos = pos.xy;
                float dist = distance(worldPos, dropletPos);
                if (dist > _DropletSize * 2.0) return;
                float effect = saturate(1.0 - dist / _DropletSize);
                float waveTime = time * _DropletSpeed;
                float wave = sin(waveTime + dist * 10.0) * effect;
                float lifeFactor = (pos.w > 0.5) ? 1.0 : (1.0 - saturate(time / _DropletLifetime));
                total += wave * pow(effect, 2.0) * _DropletIntensity * lifeFactor;
            }

            float CalculateDropletEffect(float2 worldPos) {
                float total = 0.0;
                [unroll(50)]
                for (int i = 0; i < 50; i++) {
                    float4 pos = unity_ObjectToWorld._m00 + i * 2; // dummy for loop unrolling
                    float time = 0.0;
                    if (i == 0)  { pos = _DropletPositions_0;  time = _DropletTimes_0; }
                    if (i == 1)  { pos = _DropletPositions_1;  time = _DropletTimes_1; }
                    if (i == 2)  { pos = _DropletPositions_2;  time = _DropletTimes_2; }
                    if (i == 3)  { pos = _DropletPositions_3;  time = _DropletTimes_3; }
                    if (i == 4)  { pos = _DropletPositions_4;  time = _DropletTimes_4; }
                    if (i == 5)  { pos = _DropletPositions_5;  time = _DropletTimes_5; }
                    if (i == 6)  { pos = _DropletPositions_6;  time = _DropletTimes_6; }
                    if (i == 7)  { pos = _DropletPositions_7;  time = _DropletTimes_7; }
                    if (i == 8)  { pos = _DropletPositions_8;  time = _DropletTimes_8; }
                    if (i == 9)  { pos = _DropletPositions_9;  time = _DropletTimes_9; }
                    if (i == 10) { pos = _DropletPositions_10; time = _DropletTimes_10; }
                    if (i == 11) { pos = _DropletPositions_11; time = _DropletTimes_11; }
                    if (i == 12) { pos = _DropletPositions_12; time = _DropletTimes_12; }
                    if (i == 13) { pos = _DropletPositions_13; time = _DropletTimes_13; }
                    if (i == 14) { pos = _DropletPositions_14; time = _DropletTimes_14; }
                    if (i == 15) { pos = _DropletPositions_15; time = _DropletTimes_15; }
                    if (i == 16) { pos = _DropletPositions_16; time = _DropletTimes_16; }
                    if (i == 17) { pos = _DropletPositions_17; time = _DropletTimes_17; }
                    if (i == 18) { pos = _DropletPositions_18; time = _DropletTimes_18; }
                    if (i == 19) { pos = _DropletPositions_19; time = _DropletTimes_19; }
                    if (i == 20) { pos = _DropletPositions_20; time = _DropletTimes_20; }
                    if (i == 21) { pos = _DropletPositions_21; time = _DropletTimes_21; }
                    if (i == 22) { pos = _DropletPositions_22; time = _DropletTimes_22; }
                    if (i == 23) { pos = _DropletPositions_23; time = _DropletTimes_23; }
                    if (i == 24) { pos = _DropletPositions_24; time = _DropletTimes_24; }
                    if (i == 25) { pos = _DropletPositions_25; time = _DropletTimes_25; }
                    if (i == 26) { pos = _DropletPositions_26; time = _DropletTimes_26; }
                    if (i == 27) { pos = _DropletPositions_27; time = _DropletTimes_27; }
                    if (i == 28) { pos = _DropletPositions_28; time = _DropletTimes_28; }
                    if (i == 29) { pos = _DropletPositions_29; time = _DropletTimes_29; }
                    if (i == 30) { pos = _DropletPositions_30; time = _DropletTimes_30; }
                    if (i == 31) { pos = _DropletPositions_31; time = _DropletTimes_31; }
                    if (i == 32) { pos = _DropletPositions_32; time = _DropletTimes_32; }
                    if (i == 33) { pos = _DropletPositions_33; time = _DropletTimes_33; }
                    if (i == 34) { pos = _DropletPositions_34; time = _DropletTimes_34; }
                    if (i == 35) { pos = _DropletPositions_35; time = _DropletTimes_35; }
                    if (i == 36) { pos = _DropletPositions_36; time = _DropletTimes_36; }
                    if (i == 37) { pos = _DropletPositions_37; time = _DropletTimes_37; }
                    if (i == 38) { pos = _DropletPositions_38; time = _DropletTimes_38; }
                    if (i == 39) { pos = _DropletPositions_39; time = _DropletTimes_39; }
                    if (i == 40) { pos = _DropletPositions_40; time = _DropletTimes_40; }
                    if (i == 41) { pos = _DropletPositions_41; time = _DropletTimes_41; }
                    if (i == 42) { pos = _DropletPositions_42; time = _DropletTimes_42; }
                    if (i == 43) { pos = _DropletPositions_43; time = _DropletTimes_43; }
                    if (i == 44) { pos = _DropletPositions_44; time = _DropletTimes_44; }
                    if (i == 45) { pos = _DropletPositions_45; time = _DropletTimes_45; }
                    if (i == 46) { pos = _DropletPositions_46; time = _DropletTimes_46; }
                    if (i == 47) { pos = _DropletPositions_47; time = _DropletTimes_47; }
                    if (i == 48) { pos = _DropletPositions_48; time = _DropletTimes_48; }
                    if (i == 49) { pos = _DropletPositions_49; time = _DropletTimes_49; }
                    ProcessDroplet(pos, time, worldPos, total);
                }
                return total;
            }

            fixed4 frag(v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);
                float isWater = step(_BlueThreshold, col.b) * (1 - step(0.3, col.r)) * (1 - step(0.3, col.g));
                if (isWater > 0.5) {
                    float angle = radians(_WaveAngle);
                    float2 waveDir = float2(cos(angle), sin(angle));
                    float wave = sin(_Time.y * _WaveSpeed + dot(i.worldPos.xy, waveDir) * _WaveFrequency) * _WaveIntensity;
                    wave += CalculateDropletEffect(i.worldPos.xy);
                    float2 waterUV = (i.worldPos.xy + wave * waveDir) / _WaterScale;
                    fixed4 water = tex2D(_WaterTex, waterUV);
                    water.rgb *= lerp(_WaterColor.rgb, _DeepWaterColor.rgb, 0.5) * _ColorIntensity;

                    // Reflexo do sol ADICIONADO sem alterar nada do código existente:
                    float3 reflectDir = reflect(-i.viewDir, float3(0,1,0));
                    float3 sunDir = normalize(_SunDirection.xyz);
                    float sunDot = saturate(dot(reflectDir, sunDir));
                    float sunReflection = pow(sunDot, _ReflectionSharpness) * _SunReflection;
                    water.rgb += sunReflection * _ReflectionColor.rgb;

                    water.a = _WaterColor.a;
                    return water;
                }

                // LAVA PIXELS
                float isLava = step(_RedThreshold, col.r) * (1 - step(0.3, col.b)) * (1 - step(0.3, col.g));
                if (isLava > 0.5)
                {
                    float angle = radians(_LavaWaveAngle);
                    float2 waveDir = float2(cos(angle), sin(angle));
                    float wave = sin(_Time.y * _LavaWaveSpeed + dot(i.worldPos.xy, waveDir) * _LavaWaveFrequency) * _LavaWaveIntensity;
                    wave += CalculateDropletEffect(i.worldPos.xy);
                    float2 lavaUV = (i.worldPos.xy + wave * waveDir) / _LavaScale;
                    fixed4 lava = tex2D(_LavaTex, lavaUV);
                    lava.rgb *= lerp(_LavaColor.rgb, _DeepLavaColor.rgb, 0.5) * _LavaColorIntensity;

                    float3 reflectDir = reflect(-i.viewDir, float3(0,1,0));
                    float3 sunDir = normalize(_SunDirection.xyz);
                    float sunDot = saturate(dot(reflectDir, sunDir));
                    float sunReflection = pow(sunDot, _ReflectionSharpness) * _SunReflection;
                    lava.rgb += sunReflection * _ReflectionColor.rgb;

                    lava.a = _LavaColor.a;
                    return lava;
                }
                return col;
            }
            ENDCG
        }
    }
    FallBack "Sprites/Default"
}
