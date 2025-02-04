Shader "Sprite Overlay Dissolve"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Overlay("Overlay", Color) = (0, 0, 0 ,0)
        _Noise ("Disolve Texture", 2D) = "white" {}
        _Disolve ("Disolve", Range(0, 1)) = 0
        _Outline ("Outline", Range(0, 0.1)) = 0.03
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma target 2.0
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 color: COLOR;
                float4 vertex : SV_POSITION;
                float2 positionWS: TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.positionWS = mul(unity_ObjectToWorld, v.vertex).xy;
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _Noise;
            float4 _Color;
            float4 _Overlay;
            float _Disolve;
            float _Outline;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D (_MainTex, i.uv) * i.color * _Color;

                col.rgb = lerp(col.rgb, _Overlay.rgb, _Overlay.a);

                float noise = tex2D (_Noise, i.positionWS).x;
                float dif = _Disolve - noise;

                if(dif > _Outline)
                {
                    col.a = 0;
                } else if(dif > 0)
                {
                    col.rgb = 0;
                }

                col.rgb *= col.a;

                return col;
            }
            ENDCG
        }
    }
}
