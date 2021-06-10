Shader "Unlit/GradientWithPattern"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorA("Color A",Color) = (1,0,0,1)
        _ColorB("Color B",Color) = (0,1,0,1)
        _PatternVisibility("PatternVisibility",Float) = 0
        _GradientCenter("GradientCenter",Float) = 0
        _GradientWidth("GradientWidth",Float) = 0
        _TimeScalar("TimeScalar",Float) = 0
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
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _ColorA;
            float4 _ColorB;
            float _PatternVisibility;
            float _GradientCenter;
            float _GradientWidth;
            float _TimeScalar;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {

                fixed pattern = tex2D(_MainTex, i.uv*float2(4.5,1)+_Time.y*_TimeScalar).r*_PatternVisibility;
                float t = smoothstep(_GradientCenter-_GradientWidth*0.5, _GradientCenter + _GradientWidth * 0.5, i.uv.x + pattern);
                return lerp(_ColorA,_ColorB,t);
            }
            ENDCG
        }
    }
}
