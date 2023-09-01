Shader "MapUtilsShaders/weightsMapShader"
{
    Properties
    {
        _MaxBound("MaxBound", Vector) = (-0.5,-0.5,-0.5, 0.0)
        _MinBound("MinBound", Vector) = ( 0.5, 0.5, 0.5, 0.0)
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

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 depth  : TEXCOORD0;
            };

            float4    _MaxBound;
            float4    _MinBound;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.depth = (v.vertex.xyz - _MinBound.xyz) / (_MaxBound.xyz - _MinBound.xyz);
                ///o.depth *= o.depth;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(i.depth.y, i.depth.y, i.depth.y, 1.0);
            }
            ENDCG
        }
    }
}
