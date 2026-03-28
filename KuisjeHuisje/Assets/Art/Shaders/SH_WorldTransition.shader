Shader "Effect/SH_WorldTransition"
{
    Properties
    {
        _Texture ("Texture", 2D) = "white" {}
        _PlayerTexture ("Player Texture", 2D) = "white" {}
        _Mask ("Mask", 2D) = "white" {}
        _Scale ("_Scale", Range(0,5)) = 0
        _BorderWidth ("Border Width", Range(0.0,1.0)) = 0.02
        _BorderColor ("Border Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
		Tags { "Queue"="Overlay" "RenderType"="Transparent" }

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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _Texture;
            sampler2D _PlayerTexture;
            sampler2D _Mask;

            float _Scale;
            float _BorderWidth;
            fixed4 _BorderColor;

			fixed4 frag(v2f i) : SV_Target
			{
                if (_Scale <= 0.001)
					return 0;

			    float2 uv = i.uv - 0.5;
			    float aspect = _ScreenParams.x / _ScreenParams.y;
                uv = uv * float2(aspect, 1);
                uv /= _Scale;
                uv += 0.5;
                uv = saturate(uv);

			    float maskVal = tex2D(_Mask, uv).r;

			    float alpha = maskVal;

				float border = step(maskVal, _BorderWidth);
				border = saturate(border);

			    fixed4 col = tex2D(_Texture, i.uv);
			    fixed4 playerCol = tex2D(_PlayerTexture, i.uv);
				col = lerp(col, playerCol, playerCol.a);
				col.a *= alpha;

			    col.rgb = lerp(col.rgb, _BorderColor.rgb, border);

			    return col;
			}
            ENDCG
        }
    }
}
