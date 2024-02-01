Shader "Custom/InvisibleObject"
{
    Properties
    {
        _Color("Object Color", Color) = (1, 1, 1, 1)
           _StencilRef ("Stencil Reference", Range(0,255)) = 0
    }
     
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue" = "Geometry+1"}
        
        Stencil{
            Ref [_StencilRef]
            Comp Equal
            Pass Keep
        }
        Pass
        {

            
            
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
                float3 worldPos : TEXCOORD0;
            };

            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = _Color;
                o.worldPos = mul(unity_ObjectToWorld,v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
              // if (i.worldPos.y < 0.0) discard;
                return i.color;
            }
            ENDCG
        }
    }
}