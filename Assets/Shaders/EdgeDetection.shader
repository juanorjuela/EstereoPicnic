Shader "Hidden/EdgeDetection"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _EdgeColor ("Edge Color", Color) = (0, 0, 0, 1)
        _EdgeThickness ("Edge Thickness", Range(0, 5)) = 1.0
        _DepthThreshold ("Depth Threshold", Range(0, 1)) = 0.01
        _NormalThreshold ("Normal Threshold", Range(0, 1)) = 0.4
        _EdgeIntensity ("Edge Intensity", Range(0, 1)) = 1.0
        [Toggle] _UseDepth ("Use Depth", Float) = 1
        [Toggle] _UseNormal ("Use Normal", Float) = 1
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100
        ZWrite Off
        Cull Off
        ZTest Always

        Pass
        {
            Name "EdgeDetection"
            
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };

            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);
            
            // _CameraDepthTexture is already declared by DeclareDepthTexture.hlsl
            // We'll set it via material.SetTexture, but it uses the same global name
            
            CBUFFER_START(UnityPerMaterial)
                float4 _EdgeColor;
                float _EdgeThickness;
                float _DepthThreshold;
                float _NormalThreshold;
                float _EdgeIntensity;
                float _UseDepth;
                float _UseNormal;
                float _DebugShowDepth;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                // Generate fullscreen triangle from vertex ID (for DrawProcedural)
                output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
                output.uv = GetFullScreenTriangleTexCoord(input.vertexID);
                return output;
            }

            float SampleDepth(float2 uv)
            {
                // Use SampleSceneDepth which accesses _CameraDepthTexture
                // The texture is set via material.SetTexture in the render pass
                // but SampleSceneDepth will use the global _CameraDepthTexture
                return SampleSceneDepth(uv);
            }

            float3 SampleNormal(float2 uv)
            {
                return SampleSceneNormals(uv);
            }

            float SobelDepth(float2 uv)
            {
                float2 texelSize = 1.0 / _ScreenParams.xy * _EdgeThickness;

                float depthCenter = SampleDepth(uv);
                float depthLeft = SampleDepth(uv - float2(texelSize.x, 0));
                float depthRight = SampleDepth(uv + float2(texelSize.x, 0));
                float depthUp = SampleDepth(uv - float2(0, texelSize.y));
                float depthDown = SampleDepth(uv + float2(0, texelSize.y));

                // Convert to linear depth for more stable edge detection.
                float linearCenter = Linear01Depth(depthCenter, _ZBufferParams);
                float linearLeft = Linear01Depth(depthLeft, _ZBufferParams);
                float linearRight = Linear01Depth(depthRight, _ZBufferParams);
                float linearUp = Linear01Depth(depthUp, _ZBufferParams);
                float linearDown = Linear01Depth(depthDown, _ZBufferParams);

                float depthHorizontal = abs(linearRight - linearLeft);
                float depthVertical = abs(linearDown - linearUp);

                return max(depthHorizontal, depthVertical);
            }

            float SobelNormal(float2 uv)
            {
                float2 texelSize = 1.0 / _ScreenParams.xy * _EdgeThickness;
                
                float3 normalCenter = SampleNormal(uv);
                float3 normalLeft = SampleNormal(uv - float2(texelSize.x, 0));
                float3 normalRight = SampleNormal(uv + float2(texelSize.x, 0));
                float3 normalUp = SampleNormal(uv - float2(0, texelSize.y));
                float3 normalDown = SampleNormal(uv + float2(0, texelSize.y));
                
                float3 normalHorizontal = abs(normalRight - normalLeft);
                float3 normalVertical = abs(normalDown - normalUp);
                
                float edgeHorizontal = dot(normalHorizontal, normalHorizontal);
                float edgeVertical = dot(normalVertical, normalVertical);
                
                return max(edgeHorizontal, edgeVertical);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                // Debug: Show depth visualization
                if (_DebugShowDepth > 0.5)
                {
                    float depth = SampleDepth(input.uv);
                    // If depth is 0 or 1, texture might not be accessible - show red
                    if (depth <= 0.0001 || depth >= 0.9999)
                    {
                        return half4(1, 0, 0, 1); // Red = depth texture not accessible or all same value
                    }
                    float linearDepth = Linear01Depth(depth, _ZBufferParams);
                    return half4(linearDepth, linearDepth, linearDepth, 1.0);
                }
                
                half4 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, input.uv);
                
                float depthEdge = 0.0;
                float normalEdge = 0.0;
                
                if (_UseDepth > 0.5)
                {
                    depthEdge = SobelDepth(input.uv);
                    depthEdge = step(_DepthThreshold, depthEdge);
                }
                
                if (_UseNormal > 0.5)
                {
                    normalEdge = SobelNormal(input.uv);
                    normalEdge = step(_NormalThreshold, normalEdge);
                }
                
                float edge = max(depthEdge, normalEdge) * _EdgeIntensity;
                
                return lerp(color, _EdgeColor, edge);
            }
            ENDHLSL
        }
    }
}

