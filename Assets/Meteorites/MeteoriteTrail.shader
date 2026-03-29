Shader "Custom/MeteorTrail"
{
    Properties
    {
        _Color ("Color", Color) = (1, 0.65, 0.2, 1)
        _OffsetX ("Offset X", Range(-1.0, 1.0)) = -0.5
        _OffsetY ("Offset Y", Range(-1.0, 1.0)) = -0.25
        _HeadRadius ("Head Radius", Range(0.05, 0.8)) = 0.42
        _TailLength ("Tail Length", Range(0.1, 2.5)) = 1.2
        _TailWidth ("Tail Width", Range(0.001, 0.5)) = 0.06
        _Softness ("Softness", Range(0.001, 0.5)) = 0.08
        _Intensity ("Intensity", Range(0, 10)) = 2
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha One
        Cull Off
        ZWrite Off

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

            fixed4 _Color;
            float _OffsetX;
            float _OffsetY;
            float _HeadRadius;
            float _TailLength;
            float _TailWidth;
            float _Softness;
            float _Intensity;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Put the glow head near the bottom of the quad.
                float2 p = i.uv;
                p.x += _OffsetX;
                p.y += _OffsetY;

                // Tail direction: up from the head
                float y = p.y;

                // Only keep the tail within a finite range so it does not form a line to the quad edge.
                float inTailRange =
                    smoothstep(0.0, _Softness, y) *
                    (1.0 - smoothstep(_TailLength - _Softness, _TailLength, y));

                // Normalize tail position from 0 at the head to 1 at the tail tip.
                float t = saturate(y / _TailLength);

                // Tail narrows as it moves away from the meteorite.
                float tailRadius = lerp(_HeadRadius, _TailWidth, t);

                // Soft body of the tail.
                float tail = 1.0 - smoothstep(tailRadius, tailRadius + _Softness, abs(p.x));

                // Fade alpha toward the tail tip.
                float tailFade = pow(1.0 - t, 1.6);

                // Tail glow
                float glow = tail * inTailRange * tailFade;

                return fixed4(_Color.rgb * glow * _Intensity, glow * _Color.a);
            }
            ENDCG
        }
    }
}
