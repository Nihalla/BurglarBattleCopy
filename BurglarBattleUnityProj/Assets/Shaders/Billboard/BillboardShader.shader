Shader "Billboard Shader"
{
  Properties
  {
    _MainTex ("Texture Image", 2D) = "white" {}
    [Toggle(BILLBOARD_SPHERICAL)] _SPHERICAL ("Spherical Billboarding", Float) = 0.0
    [Space]
    _StencilComp ("Stencil Comparison", Float) = 8
    _Stencil ("Stencil ID", Float) = 0
    _StencilOp ("Stencil Operation", Float) = 0
    _StencilWriteMask ("Stencil Write Mask", Float) = 255
    _StencilReadMask ("Stencil Read Mask", Float) = 255
  }
  
  SubShader
  {
    Tags 
    { 
      "Queue"="Transparent" 
      "RenderType"="Transparent"
    }
    
    Stencil
    {
      Ref [_Stencil]
      Comp [_StencilComp]
      Pass [_StencilOp]
      ReadMask [_StencilReadMask]
      WriteMask [_StencilWriteMask]
    }
    
    Cull Off
    Lighting Off
    ZWrite Off
    Blend SrcAlpha OneMinusSrcAlpha
    
    Pass
    {
      CGPROGRAM
      #pragma exclude_renderers gles
      #pragma vertex vert
      #pragma fragment frag

      #pragma shader_feature BILLBOARD_SPHERICAL
      
      uniform sampler2D _MainTex;

      struct vertexInput
      {
        float4 vertex : POSITION;
        float4 tex : TEXCOORD0;
        float4 col : COLOR;
      };

      struct vertexOutput
      {
        float4 pos : SV_POSITION;
        float4 tex : TEXCOORD0;
        float4 col : COLOR;
      };

      vertexOutput vert(vertexInput input)
      {
        vertexOutput output;

        float4x4 wvMat = UNITY_MATRIX_MV;

        wvMat[0][0] = 1.0f;
        wvMat[1][0] = 0.0f;
        wvMat[2][0] = 0.0f;

      #if BILLBOARD_SPHERICAL
        wvMat[0][1] = 0.0f;
        wvMat[1][1] = 1.0f;
        wvMat[2][1] = 0.0f;
      #endif

        wvMat[0][2] = 0.0f;
        wvMat[1][2] = 0.0f;
        wvMat[2][2] = 1.0f;

        output.pos = mul(mul(UNITY_MATRIX_P, wvMat), float4(input.vertex.x, input.vertex.y, 0.0, 1.0));

        // output.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0)) +
        //   float4(input.vertex.x, input.vertex.y, 0.0, 0.0) *
        //   float4(_ScaleY, _ScaleX, 1.0, 1.0));

        output.tex = input.tex;
        output.col = input.col;

        return output;
      }

      float4 frag(vertexOutput input) : COLOR
      {
        return tex2D(_MainTex, float2(input.tex.xy)) * input.col;
      }
      ENDCG
    }
  }
}