using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class EdgeDetectionFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class EdgeDetectionSettings
    {
        [Header("Edge Appearance")]
        [Tooltip("Color of the outline edges")]
        public Color edgeColor = Color.black;
        
        [Tooltip("Overall intensity/opacity of the edge effect (0-1)")]
        [Range(0f, 1f)]
        public float edgeIntensity = 1.0f;
        
        [Header("Edge Detection")]
        [Tooltip("Thickness/sample distance multiplier for edge detection (0-5)")]
        [Range(0f, 5f)]
        public float edgeThickness = 1.0f;
        
        [Tooltip("Sensitivity to depth changes (0-1). Higher values detect more subtle edges.")]
        [Range(0f, 1f)]
        public float depthThreshold = 0.01f;
        
        [Tooltip("Sensitivity to normal changes (0-1). Higher values detect more subtle edges.")]
        [Range(0f, 1f)]
        public float normalThreshold = 0.4f;
        
        [Header("Detection Modes")]
        [Tooltip("Enable depth-based edge detection")]
        public bool useDepth = true;
        
        [Tooltip("Enable normal-based edge detection")]
        public bool useNormal = true;
        
        [Header("Rendering")]
        [Tooltip("When to execute the edge detection pass")]
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        
        [Header("Debug")]
        [Tooltip("Show depth texture directly (for debugging)")]
        public bool debugShowDepth = false;
        
        [Tooltip("Show solid color to test if pass is running")]
        public bool debugShowSolidColor = false;
    }

    public EdgeDetectionSettings settings = new EdgeDetectionSettings();
    
    private EdgeDetectionPass edgeDetectionPass;
    private Material edgeDetectionMaterial;

    public override void Create()
    {
        Shader edgeDetectionShader = Shader.Find("Hidden/EdgeDetection");
        if (edgeDetectionShader != null)
        {
            edgeDetectionMaterial = new Material(edgeDetectionShader);
        }
        else
        {
            Debug.LogWarning("EdgeDetection shader not found. Make sure the shader is in the project.");
        }
        
        edgeDetectionPass = new EdgeDetectionPass(settings, edgeDetectionMaterial);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (edgeDetectionMaterial != null && settings.edgeIntensity > 0f)
        {
            edgeDetectionPass.ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal);
            renderer.EnqueuePass(edgeDetectionPass);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (edgeDetectionMaterial != null)
        {
            DestroyImmediate(edgeDetectionMaterial);
        }
        edgeDetectionPass?.Dispose();
    }

    private class EdgeDetectionPass : ScriptableRenderPass
    {
        private Material material;
        private EdgeDetectionSettings settings;
        private RenderTextureDescriptor descriptor;
        private RTHandle copiedColor;
        private const string k_RenderTag = "Edge Detection Effect";

        public EdgeDetectionPass(EdgeDetectionSettings settings, Material material)
        {
            this.settings = settings;
            this.material = material;
            renderPassEvent = settings.renderPassEvent;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            descriptor.msaaSamples = 1;

            RenderingUtils.ReAllocateHandleIfNeeded(ref copiedColor, descriptor, name: "_EdgeDetectionColorCopy");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (material == null)
                return;

            CommandBuffer cmd = CommandBufferPool.Get(k_RenderTag);
            
            // Update material properties
            material.SetColor("_EdgeColor", settings.edgeColor);
            material.SetFloat("_EdgeThickness", settings.edgeThickness);
            material.SetFloat("_DepthThreshold", settings.depthThreshold);
            material.SetFloat("_NormalThreshold", settings.normalThreshold);
            material.SetFloat("_EdgeIntensity", settings.edgeIntensity);
            material.SetFloat("_UseDepth", settings.useDepth ? 1.0f : 0.0f);
            material.SetFloat("_UseNormal", settings.useNormal ? 1.0f : 0.0f);

            // Get camera target
            var cameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
            
            // Copy color first so we can safely read it.
            Blitter.BlitCameraTexture(cmd, cameraColorTarget, copiedColor);
            // Blit through the edge detection shader
            Blitter.BlitCameraTexture(cmd, copiedColor, cameraColorTarget, material, 0);
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }

        public void Dispose()
        {
            copiedColor?.Release();
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (material == null || settings.edgeIntensity <= 0f)
                return;

            UniversalResourceData resourcesData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            TextureHandle source = resourcesData.activeColorTexture;
            TextureHandle destination = resourcesData.activeColorTexture;

            var targetDesc = renderGraph.GetTextureDesc(resourcesData.cameraColor);
            targetDesc.name = "_EdgeDetectionColorCopy";
            targetDesc.clearBuffer = false;

            TextureHandle colorCopy = renderGraph.CreateTexture(targetDesc);

            using (var builder = renderGraph.AddRasterRenderPass<CopyPassData>("Copy Edge Detection Color", out var passData))
            {
                passData.inputTexture = source;

                builder.UseTexture(passData.inputTexture, AccessFlags.Read);
                builder.SetRenderAttachment(colorCopy, 0, AccessFlags.Write);

                builder.SetRenderFunc((CopyPassData data, RasterGraphContext context) =>
                {
                    Blitter.BlitTexture(context.cmd, data.inputTexture, new Vector4(1, 1, 0, 0), 0.0f, false);
                });
            }

            using (var builder = renderGraph.AddRasterRenderPass<PassData>(k_RenderTag, out var passData))
            {
                // Update material properties
                material.SetColor("_EdgeColor", settings.edgeColor);
                material.SetFloat("_EdgeThickness", settings.edgeThickness);
                material.SetFloat("_DepthThreshold", settings.depthThreshold);
                material.SetFloat("_NormalThreshold", settings.normalThreshold);
                material.SetFloat("_EdgeIntensity", settings.edgeIntensity);
                material.SetFloat("_UseDepth", settings.useDepth ? 1.0f : 0.0f);
                material.SetFloat("_UseNormal", settings.useNormal ? 1.0f : 0.0f);
                material.SetFloat("_DebugShowDepth", settings.debugShowDepth ? 1.0f : 0.0f);

                passData.material = material;
                passData.inputTexture = colorCopy;
                passData.debugShowDepth = settings.debugShowDepth;
                passData.debugShowSolidColor = settings.debugShowSolidColor;
                passData.destination = destination;

                builder.UseTexture(passData.inputTexture, AccessFlags.Read);
                builder.SetRenderAttachment(destination, 0, AccessFlags.Write);

                bool hasDepth = settings.useDepth && resourcesData.cameraDepthTexture.IsValid();
                bool hasNormal = settings.useNormal && resourcesData.cameraNormalsTexture.IsValid();

                // Always use depth texture if available (needed for shader access)
                if (resourcesData.cameraDepthTexture.IsValid())
                {
                    builder.UseTexture(resourcesData.cameraDepthTexture, AccessFlags.Read);
                    passData.depthTextureHandle = resourcesData.cameraDepthTexture;
                }
                else
                {
                    passData.depthTextureHandle = TextureHandle.nullHandle;
                }

                if (hasNormal && resourcesData.cameraNormalsTexture.IsValid())
                {
                    builder.UseTexture(resourcesData.cameraNormalsTexture, AccessFlags.Read);
                    passData.normalTextureHandle = resourcesData.cameraNormalsTexture;
                }
                else
                {
                    passData.normalTextureHandle = TextureHandle.nullHandle;
                }

                builder.AllowPassCulling(false);
                builder.AllowGlobalStateModification(true);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    // Set depth and normal textures as global textures so SampleSceneDepth/SampleSceneNormals can access them
                    // This is required because DeclareDepthTexture.hlsl expects _CameraDepthTexture to be a global texture
                    if (data.depthTextureHandle.IsValid())
                    {
                        context.cmd.SetGlobalTexture("_CameraDepthTexture", data.depthTextureHandle);
                    }
                    
                    if (data.normalTextureHandle.IsValid())
                    {
                        context.cmd.SetGlobalTexture("_CameraNormalsTexture", data.normalTextureHandle);
                    }
                    
                    if (data.debugShowSolidColor)
                    {
                        // Debug: Fill screen with red to verify pass is running
                        context.cmd.ClearRenderTarget(true, true, Color.red);
                    }
                    else
                    {
                        // Use DrawProcedural like FullScreenPassRendererFeature does
                        // This properly executes the material/shader
                        var propertyBlock = new MaterialPropertyBlock();
                        if (data.inputTexture.IsValid())
                        {
                            propertyBlock.SetTexture(Shader.PropertyToID("_BlitTexture"), data.inputTexture);
                        }
                        propertyBlock.SetVector(Shader.PropertyToID("_BlitScaleBias"), new Vector4(1, 1, 0, 0));
                        
                        context.cmd.DrawProcedural(Matrix4x4.identity, data.material, 0, MeshTopology.Triangles, 3, 1, propertyBlock);
                    }
                });
            }
        }

        private class CopyPassData
        {
            public TextureHandle inputTexture;
        }

        private class PassData
        {
            public Material material;
            public TextureHandle inputTexture;
            public bool debugShowDepth;
            public bool debugShowSolidColor;
            public TextureHandle depthTextureHandle;
            public TextureHandle normalTextureHandle;
            public TextureHandle destination;
        }
    }
}

