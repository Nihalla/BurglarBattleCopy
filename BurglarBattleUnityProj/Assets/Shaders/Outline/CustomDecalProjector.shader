/* Written by Felix Antonelli.
* Adapted from: https://www.ronja-tutorials.com/post/054-unlit-dynamic-decals/
*/

Shader "Ai/CustomDecalProjector"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Tint ("Tint", color) = (1, 0, 0, 0.5)
        _Opacity ("Opacity", Range(0, 1)) = 0.5
        _AllowOpacityOverride ("Opacity Override", Range(0, 1)) = 0.0
        _FadePow ("Fade", Range(0, 10)) = 2
        _ProjectionNormal ("Projection Normal", Vector) = (0, 1, 0)
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite off
        Cull back

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile REQUIRE_DEPTH_TEXTURE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "DecodeDepthNormals.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 position : SV_POSITION;
                float3 ray : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Tint;
            float3 _ProjectionNormal;
            float _FadePow;
            float _Opacity;
            float _AllowOpacityOverride;
            
            TEXTURE2D(_DepthNormalsTexture);
            SAMPLER(sampler_DepthNormalsTexture);

            float4 ComputeScreenPosA(float4 positionCS)
            {
                float4 o = positionCS * 0.5f;
                o.xy = float2(o.x, o.y * _ProjectionParams.x) + o.w;
                o.zw = positionCS.zw;
                return o;
            }

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.uv = v.uv;
                float3 worldPos = mul(unity_ObjectToWorld, v.positionOS);
                o.position = TransformObjectToHClip(v.positionOS);
                o.ray = worldPos - UNITY_MATRIX_I_V._m03_m13_m23;
                //ComputeScreenPos is depreciated, ComputeNormalizedDeviceCoordinatesWithZ seems to be the closest
                o.screenPos = ComputeScreenPosA(o.position);
                return o;
            }

            float3 getProjectedObjectPos(float2 screenPos, float3 worldRay, out float3 surfaceNormalAtScreenPos)
            {
                //Retreive depth normal info from camera buffer
                float4 depthNormal = SAMPLE_TEXTURE2D(_DepthNormalsTexture, sampler_DepthNormalsTexture, screenPos);
                float depth;
                DecodeDepthNormal(depthNormal, depth, surfaceNormalAtScreenPos);
                depth = Linear01Depth(depth, _ZBufferParams);
                //Compute object position of screen space position
                worldRay = normalize(worldRay);
                worldRay /= dot(worldRay, -UNITY_MATRIX_V[2].xyz);
                float3 worldPos = UNITY_MATRIX_I_V._m03_m13_m23 + worldRay * depth;
                float3 objectPos = mul(unity_WorldToObject, float4(worldPos, 1)).xyz;
                // clip(0.5 - abs(objectPos));
                // objectPos += 0.5f;
                return objectPos;
            }

            static void GetDepthAndNormal(float2 uv, out float depth, out float3 normal)
            {
                float4 coded = SAMPLE_TEXTURE2D(_DepthNormalsTexture, sampler_DepthNormalsTexture, uv);
                DecodeDepthNormal(coded, depth, normal);
                // normal = normal * 2 - 1;
            }

            float2 SphereCoords(float3 pos)
            {
                return float2((atan2(pos.x, -pos.y) / PI + 1) / 2, asin(pos.z) / PI + .5);
            }

            half4 frag(Varyings i) : SV_Target
            {
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float3 surfaceNormal;
                float2 uv = getProjectedObjectPos(screenUV, i.ray, surfaceNormal).xz;
                float4 col = tex2D(_MainTex, i.uv);

                //transform surface normal to world normal
                surfaceNormal = mul((float3x3)UNITY_MATRIX_I_V, surfaceNormal);
                float dotP = dot(surfaceNormal, _ProjectionNormal);

                if (dotP < 0.2f)
                {
                    dotP = 0;
                }
                //
                // //Clip any normals that do not align with the projector normal and apply tint.
                col *= dotP;
                surfaceNormal *= dotP;
                if (_AllowOpacityOverride > 0.5f)
                {
                    col *= _Tint;
                }
                else
                {
                    col *= float4(_Tint.xyz, _Opacity);
                }
                // float2 sUV = float2(pow(i.uv.y, _FadePow), 0);
                // col *= length(sUV);
                return col;
            }
            ENDHLSL
        }
    }
}