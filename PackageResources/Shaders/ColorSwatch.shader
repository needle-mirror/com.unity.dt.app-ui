Shader "Hidden/App UI/ColorSwatch"
{
    Properties
    {
        _CheckerColor1 ("Color 1", Color) = (1,1,1,1)
        _CheckerColor2 ("Color 2", Color) = (1,1,1,1)
        _CheckerSize ("Size", Float) = 10
        _Width ("Width", Float) = 200
        _Height ("Height", Float) = 200
        [Toggle] _IsFixed ("Is Fixed", Int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "AppUI.cginc"

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

            fixed4 _CheckerColor1;
            fixed4 _CheckerColor2;
            float _CheckerSize;
            float _Width;
            float _Height;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 color = checker_board(i.uv, _Width, _Height, _CheckerSize, _CheckerColor1, _CheckerColor2);
                return color;
            }
            ENDCG
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

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

            struct ColorStop
            {
                float4 color;
                float position;
            };

            struct AlphaStop
            {
                float alpha;
                float position;
            };

            #define MAX_GRADIENT_COLORS_COUNT 11

            uniform int _ColorCount;
            uniform int _AlphaCount;
            int _IsFixed;
            StructuredBuffer<ColorStop> _Colors;
            StructuredBuffer<AlphaStop> _Alphas;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                const int colorCount = min(_ColorCount, MAX_GRADIENT_COLORS_COUNT);
                const int alphaCount = min(_AlphaCount, MAX_GRADIENT_COLORS_COUNT);

                clip(colorCount + alphaCount - 1);

                if (colorCount == 1 && alphaCount == 1)
                {
                    return fixed4(_Colors[0].color.rgb, _Alphas[0].alpha);
                }

                const float current_position = i.uv.x;

                // colors must be sorted by position to work

                int first_color_index = 0;
                int first_alpha_index = 0;

                UNITY_UNROLL
                for (int index = 0; index < colorCount; index++)
                {
                    first_color_index = max(first_color_index, step(_Colors[index].position, current_position) * index);
                }

                UNITY_UNROLL
                for (int index = 0; index < alphaCount; index++)
                {
                    first_alpha_index = max(first_alpha_index, step(_Alphas[index].position, current_position) * index);
                }

                const int second_color_index = min(first_color_index + 1, colorCount - 1);
                const int second_alpha_index = min(first_alpha_index + 1, alphaCount - 1);
                
                const float colorDelta = (current_position - _Colors[first_color_index].position) / max(0.00001, _Colors[second_color_index].position - _Colors[first_color_index].position);
                const float alphaDelta = (current_position - _Alphas[first_alpha_index].position) / max(0.00001, _Alphas[second_alpha_index].position - _Alphas[first_alpha_index].position);

                const float smoothColorDelta = lerp(smoothstep(0.0, 1.0, colorDelta), step(0.0, colorDelta), _IsFixed);
                const float smoothAlphaDelta = lerp(smoothstep(0.0, 1.0, alphaDelta), step(0.0, alphaDelta), _IsFixed);
                
                const fixed3 first_color = _Colors[first_color_index].color.rgb;
                const fixed3 second_color = _Colors[second_color_index].color.rgb;

                const float first_alpha = _Alphas[first_alpha_index].alpha;
                const float second_alpha = _Alphas[second_alpha_index].alpha;

                const fixed3 lerped_color = lerp(first_color, second_color, smoothColorDelta);
                const float lerped_alpha = lerp(first_alpha, second_alpha, smoothAlphaDelta);

                fixed4 color = fixed4(lerped_color, lerped_alpha);
                
                #ifdef UNITY_COLORSPACE_GAMMA
                return color;
                #endif

                return fixed4(GammaToLinearSpace(color.rgb), color.a);
            }
            ENDCG
        }
    }
}
