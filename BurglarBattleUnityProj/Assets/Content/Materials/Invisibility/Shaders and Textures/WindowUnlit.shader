Shader "Custom/AntiX-RayScreen"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
         _StencilRef ("Stencil Reference", Range(0,255)) = 0
    }
    SubShader
    {
            Tags { 
            "RenderType"="Transparent"  
            "Queue"="Geometry"
            }
        
        Stencil
        {
            Ref [_StencilRef]
            Comp Always
            Pass Replace
        }
        blend One  One
        Pass
        {

            ZWrite off

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
                fixed4 color : COLOR;
                float4 vertex : SV_POSITION;
            };

            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}
