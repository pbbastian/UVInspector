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
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
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
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
            float3 _Center;
            float3 _Extents;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = float4(TRANSFORM_TEX(v.uv, _MainTex) * 2 - 1, 0, 1);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.objectPosition = (v.vertex - _Center) / _Extents;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				// sample the texture
				float4 col = float4(i.objectPosition, 1.0);//tex2D(_MainTex, i.uv);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
