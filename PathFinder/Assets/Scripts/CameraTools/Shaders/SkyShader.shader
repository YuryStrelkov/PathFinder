Shader "MapUtilsShaders/SkyShader"
{
    Properties
    {
        _sky_color("sky_color",      Vector) = (0.6, 0.8, 1.0, 0.0)
        _deep_sky_color("deep_sky_color", Vector) = (0.5, 0.6, 1.0, 0.0)
        _ground_color("ground_color",   Vector) = (0.35, 0.3, 0.25, 0.0)
        _sun_color("sun_color",      Vector) = (1.0, 1.0, 0.9, 0.0)
        _sun_position("sun_position",   Vector) = (1.0, 2.0, 1.0, 0.0)
        _sun_size("sun_size",           Float) = 0.05
        _ground_threshold("ground_threshold",   Float) = 0.125
        _sun_threshold("sun_threshold",      Float) = 0.125
        _deep_sky_threshold("deep_sky_threshold", Float) = 0.25
    }
        SubShader
    {
        // Tags { "RenderType"="Opaque" }
        LOD 100
        ZTest Less
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _sky_color        ;
            float4 _deep_sky_color   ;
            float4 _ground_color     ;
            float4 _sun_color        ;
            float4 _sun_position     ;
            float _sun_size          ;
            float _ground_threshold  ;
            float _sun_threshold     ;
            float _deep_sky_threshold;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 position: TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.position = v.vertex.xyz; // normalize(v.vertex.xyz);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 direction = normalize(i.position);
                float factor = smoothstep(-_ground_threshold * 0.5, _ground_threshold * 0.5, -direction.y);
                float4 color = _sky_color * (1 - factor) + _ground_color * factor;
                factor = smoothstep(0.9 - _deep_sky_threshold, 0.9, direction.y);
                color = _deep_sky_color * factor + color * (1 - factor);
                factor = dot(direction, normalize(_sun_position.xyz));
                factor = factor > 0 ? factor : 0.0;
                factor = pow(smoothstep(1.0 - _sun_threshold - _sun_size, 1.0 - _sun_size, factor), 8);
                color = color * (1 - factor) + factor * _sun_color;
                return fixed4(color.xyz, 1);;
            }
            ENDCG
        }
    }
}
