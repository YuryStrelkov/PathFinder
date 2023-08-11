Shader "MapUtilsShaders/GridShader"
{
    Properties
    {
        _grid_x_step ("_grid_x_step", Float) = 1.0 
        _grid_y_step ("_grid_y_step", Float) = 1.0 
        _line_size   ("_line_size"  , Float) = 0.05
        _fade        ("_fade"       , Float) = 0.25
        _grid_alpha  ("_grid_alpha" , Float) = 0.25
        _fade_radius ("_fade_radius", Float) = 10.0
        _y_axis_color("_y_axis_color", Vector) = (1.0, 0.5, 0.5, 1.0)
        _x_axis_color("_x_axis_color", Vector) = (0.5, 1.0, 0.5, 1.0)
        _line_color  ("_line_color",   Vector) = (0.5, 0.5, 0.5, 1.0)
    }
    SubShader
    {
        LOD 100
        ZTest Less
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
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
                float2 scaling : TEXCOORD0;
                float2 uv      : TEXCOORD1;
                float4 vertex  : SV_POSITION;
            };

            float remainder(float value, float div)
            {
                return value - round(value / div) * div;
            }

            float  _grid_x_step;
            float  _grid_y_step;
            float  _line_size  ;
            float  _fade       ;
            float  _grid_alpha ;
            float  _fade_radius;
            float4 _y_axis_color;
            float4 _x_axis_color;
            float4 _line_color  ;

            v2f vert (appdata v)
            {
                float x_scale = sqrt(unity_ObjectToWorld[0][0] * unity_ObjectToWorld[0][0] +
                                     unity_ObjectToWorld[1][0] * unity_ObjectToWorld[1][0] +
                                     unity_ObjectToWorld[2][0] * unity_ObjectToWorld[2][0]);

                float z_scale = sqrt(unity_ObjectToWorld[0][2] * unity_ObjectToWorld[0][2] +
                                     unity_ObjectToWorld[1][2] * unity_ObjectToWorld[1][2] +
                                     unity_ObjectToWorld[2][2] * unity_ObjectToWorld[2][2]);

                v2f o;
                o.scaling = float2(x_scale, z_scale);
                o.uv      = float2(v.vertex.x * x_scale, v.vertex.z * z_scale);
                o.vertex  = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {

                    float fade_distance = length(i.uv.xy / i.scaling);

                    float alpha = _grid_alpha;

                    float fade_bound = (_fade_radius) * 0.5 - _fade;

                    if (fade_distance >= fade_bound)
                    {
                        fade_distance -= fade_bound;
                        alpha *= (smoothstep(_fade, 0.0, fade_distance));
                        alpha = 1 - alpha;
                    }

                    if (abs(i.uv.x) <= _line_size * 0.5)return fixed4(_x_axis_color.rgb, alpha);

                    if (abs(i.uv.y) <= _line_size * 0.5)return fixed4(_y_axis_color.rgb, alpha);

                    if (abs(remainder(i.uv.y, _grid_y_step)) <= 0.5 * _line_size)return fixed4(_line_color.rgb, alpha);

                    if (abs(remainder(i.uv.x, _grid_x_step)) <= 0.5 * _line_size)return fixed4(_line_color.rgb, alpha);
                    
                    discard;

                    return fixed4(0.0, 0.0, 0.0, 0.0);
            }
            ENDCG
        }
    }
}
