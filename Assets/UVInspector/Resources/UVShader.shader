Shader "Unlit/UVShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Center ("Center", Vector) = (0, 0, 0, 0)
        _Extents ("Extents", Vector) = (1, 1, 1, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Cull Off

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
                float2 uv : TEXCOORD0;
                float3 objectPosition : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float3 _Center;
            float3 _Extents;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.uv = v.uv;
                o.vertex = float4(o.uv * 2 - 1 + 1.0/_ScreenParams.xy + 1e-3, 0, 1);
                o.objectPosition = ((v.vertex - _Center) / _Extents) * 0.5 + 0.5;
                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
                return float4(i.objectPosition, 1.0);
            }
            ENDCG
        }
    }
}
