Shader "MapUtilsShaders/heatMapShader"
{
    Properties
    {
        _MainTex ("Texture" , 2D) = "white" {}
        _MaxBound("MaxBound", Vector) = (-1.0,-1.0,-1.0, 0.0)
        _MinBound("MinBound", Vector) = ( 1.0, 1.0, 1.0, 0.0)
    }
    SubShader
    {
        // Tags { "RenderType"="Opaque" }
        // LOD 100
        ZTest Less
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float height_map(float v, float vmin, float vmax)
            {
                return min(max((v - vmin) / (vmax - vmin), 0.0), 1.0);
            }

            float3 heatMap(float v, float vmin, float vmax)
            {
               float3 c = float3(1.0, 1.0, 1.0); // white

               v = height_map(v, vmin, vmax);

               if (v < 0.25)
                   {
                  c.x = 0.0;
                  c.y = 4.0 * v;
               }
               else if (v < 0.5)
               {
                  c.x = 0.0;
                  c.z = 1.0 + 4.0 * (0.25 - v);
               }
               else if (v < 0.75)
               {
                  c.x = 4.0 * (v - 0.5);
                  c.z = 0.0;
               }
               else
               {
                  c.y = 1.0 + 4.0 * (0.75 - v);
                  c.z = 0.0;
               }

               return c;
            };


            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 depth  : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4    _MaxBound;
            float4    _MinBound;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.depth = (v.vertex.xyz - _MinBound.xyz) / (_MaxBound.xyz - _MinBound.xyz);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(heatMap(i.depth.y, _MinBound.y, _MaxBound.y), 1.0);
            }
            ENDCG
        }
    }
}
