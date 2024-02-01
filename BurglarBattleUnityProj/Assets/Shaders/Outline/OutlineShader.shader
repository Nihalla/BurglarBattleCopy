Shader "WSWhitehouse/OutlineShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _Thickness ("Thickness", float) = 1.0
        _OutlineCol ("Outline Colour", color) = (0.0, 0.0, 0.0, 1.0)

        _OutlineClipPoint ("Outline Clip  Point", Range(0, 1)) = 0.25

        _AcuteDepthThreshold ("Acute Depth Threshold", float) = 0.0
        _AcuteAngleStartDot ("Acute Angle Start Dot", Range(-1.0, 1.0)) = 0.0

        _DepthStrength ("Depth Strength", float) = 0.0
        _DepthThickness ("Depth Thickness", float) = 0.0
        _DepthThreshold ("Depth Threshold", float) = 0.0

        _ColStrength ("Colour Strength", float) = 0.0
        _ColThickness ("Colour Thickness", float) = 0.0
        _ColThreshold ("Colour Threshold", float) = 0.0

        _NormalsStrength ("Normals Strength", float) = 0.0
        _NormalsThickness ("Normals Thickness", float) = 0.0
        _NormalsThreshold ("Normals Threshold", float) = 0.0
        _NormalsFarThreshold ("Normals Far Threshold", float) = 0.0
        _NormalsAdjustNearDepth ("Normals Adjust Near Depth", float) = 0.0
        _NormalsAdjustFarDepth ("Normals Adjust Far Depth", float) = 0.0
        
        _CrossHatchStrength("Cross Hatch Strength", float) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile REQUIRE_DEPTH_TEXTURE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "DecodeDepthNormals.hlsl"

            // This shader is based of the following videos, conversion to HLSL was done by WSWhitehouse.
            // https://www.youtube.com/watch?v=RMt6DcaMxcE
            // https://www.youtube.com/watch?v=fW-5srSHDMc

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                // float2 uv           : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _Thickness;
            float4 _OutlineCol;

            float _OutlineClipPoint;

            float _AcuteDepthThreshold;
            float _AcuteAngleStartDot;

            float _DepthStrength;
            float _DepthThickness;
            float _DepthThreshold;

            float _ColStrength;
            float _ColThickness;
            float _ColThreshold;

            float _NormalsStrength;
            float _NormalsThickness;
            float _NormalsThreshold;
            float _NormalsFarThreshold;
            float _NormalsAdjustNearDepth;
            float _NormalsAdjustFarDepth;

            float _CrossHatchStrength;

            TEXTURE2D(_DepthNormalsTexture);
            SAMPLER(sampler_DepthNormalsTexture);

            static const float2 sobelPoints[9] = {
                float2(-1, 1), float2(0, 1), float2(1, 1),
                float2(-1, 0), float2(0, 0), float2(1, 0),
                float2(-1, -1), float2(0, -1), float2(1, -1),
            };

            static const float sobelXMatrix[9] = {
                1, 0, -1,
                2, 0, -2,
                1, 0, -1
            };

            static const float sobelYMatrix[9] = {
                1, 2, 1,
                0, 0, 0,
                -1, -2, -1
            };

            static void GetDepthAndNormal(float2 uv, out float depth, out float3 normal)
            {
                float4 coded = SAMPLE_TEXTURE2D(_DepthNormalsTexture, sampler_DepthNormalsTexture, uv);
                DecodeDepthNormal(coded, depth, normal);
                // normal = normal * 2 - 1;
            }

            static float3 viewDirFromScreenUV(float2 uv)
            {
                // Code from Keijiro Takahashi @_kzr and Ben Golus @bgolus
                float2 p11_22 = float2(unity_CameraProjection._11, unity_CameraProjection._22);
                return -normalize(float3((uv * 2 - 1) / p11_22, -1));
            }

            static float sobelDepth(const float2 uv)
            {
                float2 sobel = 0;

                [unroll]
                for (int i = 0; i < 9; i++)
                {
                    float depth = SampleSceneDepth(uv + sobelPoints[i] * _Thickness);
                    sobel += depth * float2(sobelXMatrix[i], sobelYMatrix[i]);
                }

                return length(sobel);
            }

            static float sobelColour(const float2 uv)
            {
                float2 sobelRed = 0;
                float2 sobelGreen = 0;
                float2 sobelBlue = 0;

                [unroll]
                for (int i = 0; i < 9; i++)
                {
                    float3 rgb = tex2D(_MainTex, uv + sobelPoints[i] * _Thickness).rgb;
                    float2 kernel = float2(sobelXMatrix[i], sobelYMatrix[i]);

                    sobelRed += rgb.r * kernel;
                    sobelGreen += rgb.g * kernel;
                    sobelBlue += rgb.b * kernel;
                }

                return max(length(sobelRed), max(length(sobelGreen), length(sobelBlue)));
            }

            static float sobelNormals(const float2 uv)
            {
                float2 sobelR = 0;
                float2 sobelG = 0;
                float2 sobelB = 0;

                [unroll]
                for (int i = 0; i < 9; i++)
                {
                    float depth;
                    float3 rgb; //    = tex2D(_MainTex, uv + sobelPoints[i] * _Thickness).rgb;
                    GetDepthAndNormal(uv + sobelPoints[i] * _Thickness, depth, rgb);
                    float2 kernel = float2(sobelXMatrix[i], sobelYMatrix[i]);

                    sobelR += rgb.r * kernel;
                    sobelG += rgb.g * kernel;
                    sobelB += rgb.b * kernel;
                }

                return max(length(sobelR), max(length(sobelG), length(sobelB)));
            }

            static float applyOutlineSettings(const float val, const float threshold, const float thickness,
                                              const float strength)
            {
                float output;
                output = smoothstep(0, threshold, val);
                output = pow(output, thickness);
                output = output * strength;
                return output;
            }

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.screenPos = ComputeScreenPos(o.positionCS);
                o.color = v.color;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                
                const float2 uv = i.screenPos.xy / i.screenPos.w;

                float3 viewDir = viewDirFromScreenUV(uv);

                float _unused;
                float3 normal;
                GetDepthAndNormal(uv, _unused, normal);
                // return float4(normal.xyz, 1);

                float rawDepth = SampleSceneDepth(uv);
                float sceneEyeDepth = LinearEyeDepth(rawDepth, _ZBufferParams);

                // Calculate depth sobel and apply settings
                float depth = sobelDepth(uv);
                float d = 1 - dot(normal, viewDir);
                float s = smoothstep(_AcuteAngleStartDot, 1, d);
                float depthThreshold = lerp(_DepthThreshold, _AcuteDepthThreshold, s) * rawDepth;
                depth = applyOutlineSettings(depth, depthThreshold, _DepthThickness, _DepthStrength);

                // Calculate colour sobel and apply settings
                float col = sobelColour(uv);
                col = applyOutlineSettings(col, _ColThreshold, _ColThickness, _ColStrength);

                // Calculate colour sobel and apply settings
                float normals = sobelNormals(uv);

                float normalsAdjustDepth = smoothstep(_NormalsAdjustNearDepth, _NormalsAdjustFarDepth, sceneEyeDepth);
                float normalsThreshold = lerp(_NormalsThreshold, _NormalsFarThreshold, normalsAdjustDepth);
                normals = applyOutlineSettings(normals, normalsThreshold, _NormalsThickness, _NormalsStrength);

                //reduce appearance of pale outlines
                float depthNorm = max(depth, normals);
                if (depthNorm > _OutlineClipPoint)
                {
                    depthNorm = 1;
                }
                else
                {
                    depthNorm = 0;
                }

                float maximum = max(col, depthNorm);
                return lerp(tex2D(_MainTex, uv), _OutlineCol, maximum);
            }
            ENDHLSL
        }
    }
}