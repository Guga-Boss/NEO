Shader "Kilt/DebugDraw" {
        Properties {
            _Color ("Main Color", Color) = (1,1,1,1)
        }
        SubShader {
			Cull Off
			Lighting Off
			//ZWrite Off
			Fog { Mode Off }
			Blend SrcAlpha OneMinusSrcAlpha
			Tags
			{ 
			"Queue"="Overlay" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent"
			}
            Pass {
                Color [_Color]
            }
        }
    }