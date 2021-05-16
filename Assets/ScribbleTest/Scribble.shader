Shader "Unlit/Scribble"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Noise("Noise", 2D) = "gray" {}
        _Frequency("Frequency", Float) = 1
        _Magnitude("Magnitude",Float) = 0.01
        _UpdateSpeed("Update Speed",Float) = 0.2
        _Texels("Texels",Float) = 128
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
            sampler2D _Noise;
            float _Frequency;
            float _Magnitude;
            float _UpdateSpeed;
            float _Texels;
            float hash12(float2 p) {
                p = frac(p * float2(5.3983, 5.4427));
                p += dot(p.yx, p.xy + float2(21.5351, 14.3137));
                return frac(p.x * p.y * 95.4337);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                i.uv = floor(i.uv * _Texels) / _Texels;
                float t = floor(_Time.y* _UpdateSpeed);
                float2 offset = float2(
                    tex2D(_Noise, ((i.uv* _Frequency) + hash12(t + 0))).x,
                    tex2D(_Noise, ((i.uv* _Frequency) + hash12(t + 1))).x);
                offset = offset * 2 - 1;
                fixed4 col = tex2D(_MainTex, i.uv+(offset*_Magnitude));
                return col;
            }
            ENDCG
        }
    }
}
