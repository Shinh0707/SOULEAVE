Shader "Custom/GradientGauge"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Shift("Shift", Range(0, 1)) = 0
        _StartColor ("Start Color", Color) = (1,1,1,1)
        _EndColor ("End Color", Color) = (1,1,1,1)
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
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Shift;
            fixed4 _StartColor;
            fixed4 _EndColor;
            
            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float s = (1 - _Shift) * 0.5;
                float2 coord = float2(saturate(i.uv.x + _Shift * step(s, i.uv.x)), i.uv.y);
                half4 texColor = tex2D(_MainTex, coord);
                
                // Calculate gradient color
                fixed4 gradientColor = lerp(_StartColor, _EndColor, i.uv.x);
                
                // Apply gradient to texture color
                half4 col = texColor * gradientColor * i.color;
                
                // Apply mask
                half mask = abs(step(1 - _Shift, i.uv.x) - 1);
                col *= mask;
                
                return col;
            }
            ENDCG
        }
    }
}