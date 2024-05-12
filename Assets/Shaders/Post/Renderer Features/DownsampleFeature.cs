using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/*
 *      Template + knowledge taken from https://www.cyanilux.com/tutorials/custom-renderer-features/#setuprenderpasses
 *      
 *      Special coder, special codebase, special code   (tuesday, tuesday)
 * 
 */

public class DownsampleFeature : ScriptableRendererFeature
{

    public class DownsamplePass : ScriptableRenderPass
    {

        private Settings settings;
        private FilteringSettings filteringSettings;
        private ProfilingSampler _profilingSampler;
        private List<ShaderTagId> shaderTagsList = new List<ShaderTagId>();
        private RTHandle rtCustomColor, rtDownsampleOne, rtDownsampleTwo;
        private RenderTextureDescriptor colorDesc;

        public DownsamplePass(Settings settings, string name)
        {
            this.settings = settings;
            filteringSettings = new FilteringSettings(RenderQueueRange.opaque, settings.layerMask);

            // Use default tags
            shaderTagsList.Add(new ShaderTagId("SRPDefaultUnlit"));
            shaderTagsList.Add(new ShaderTagId("UniversalForward"));
            shaderTagsList.Add(new ShaderTagId("UniversalForwardOnly"));

            _profilingSampler = new ProfilingSampler(name);
        }

        public ScaleFunc ScaleDown(int scale)
        {
            return v =>
            {
                for (int i = 0; i < scale; i++)
                {
                    v = Scale(v);
                }
                return v;
            };
        }

        public Vector2Int Scale(Vector2Int size)
        {
            return new Vector2Int(Mathf.FloorToInt((float)(size.x * 0.5)), Mathf.FloorToInt((float)(size.y * 0.5)));
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            colorDesc = renderingData.cameraData.cameraTargetDescriptor;
            colorDesc.depthBufferBits = 0;

            // Set up custom color target buffer (to render objects into)
            if (settings.colorTargetDestinationID != "")
            {
                RenderingUtils.ReAllocateIfNeeded(ref rtCustomColor, colorDesc, name: settings.colorTargetDestinationID);
            }
            else
            {
                // colorDestinationID is blank, use camera target instead
                rtCustomColor = renderingData.cameraData.renderer.cameraColorTargetHandle;
            }

            // Using camera's depth target (that way we can ZTest with scene objects still)
            RTHandle rtCameraDepth = renderingData.cameraData.renderer.cameraDepthTargetHandle;

            ConfigureTarget(rtCustomColor);
            //ConfigureClear(ClearFlag.Color, new Color(0, 0, 0, 0));
            //ConfigureTarget(renderingData.cameraData.renderer.cameraColorTargetHandle);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            // Set up profiling scope for Profiler & Frame Debugger
            using (new UnityEngine.Rendering.ProfilingScope(cmd, _profilingSampler))
            {
                // Command buffer shouldn't contain anything, but apparently need to
                // execute so DrawRenderers call is put under profiling scope title correctly
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();


                // Draw Renderers to Render Target (set up in OnCameraSetup)
                SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
                DrawingSettings drawingSettings = CreateDrawingSettings(shaderTagsList, ref renderingData, sortingCriteria);
                if (settings.overrideMaterial != null)
                {
                    drawingSettings.overrideMaterialPassIndex = settings.overrideMaterialPass;
                    drawingSettings.overrideMaterial = settings.overrideMaterial;
                }
                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);

                // Now, we have the renderers of anything in the designated layermask drawn into rtCustomColor, so
                // here we can post process only object of that specific layer!
                /*
                RenderTextureDescriptor ds = rtCustomColor.rt.descriptor;
                ds.width /= 2;
                ds.height /= 2;

                RenderingUtils.ReAllocateIfNeeded(ref rtCustomColor, ds, name: "downsampleLayer");
                */

                // Pass our custom target to shaders as a Global Texture reference
                // In a Shader Graph, you'd obtain this as a Texture2D property with "Exposed" unticked

                if (settings.colorTargetDestinationID != "")
                    cmd.SetGlobalTexture(settings.colorTargetDestinationID, rtCustomColor);

                ConfigureTarget(renderingData.cameraData.renderer.cameraColorTargetHandle);

                // Apply material (e.g. Fullscreen Graph) to camera
                if (settings.blitMaterial != null)
                {
                    RTHandle camTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
                    if (camTarget != null)
                    {
                        /*
                         *      POSTPROCESSING EFFECTS GO HERE
                         */

                        if (settings.Downsample > 0)
                        {
                            // Downsampling:

                            RenderTextureDescriptor downsample = colorDesc;
                            downsample.width /= 2;
                            downsample.height /= 2;
                            // First: Reallocate the downsample destination texture handle with half the size it had
                            RenderingUtils.ReAllocateIfNeeded(ref rtDownsampleOne, downsample, name: "_DownsampleOne");
                            if (settings.Downsample == 2)
                            {
                                downsample.width /= 2;
                                downsample.height /= 2;
                                RenderingUtils.ReAllocateIfNeeded(ref rtDownsampleTwo, downsample, name: "_DownsampleTwo");
                            }
                            // Second: Blit the camera texture from the downsample source to the downsample destination (halving the dimension) and using our empty material
                            Blitter.BlitCameraTexture(cmd, camTarget, rtDownsampleOne, settings.blitMaterial, 0);
                            if (settings.Downsample == 2)
                            {
                                Blitter.BlitCameraTexture(cmd, rtDownsampleOne, rtDownsampleTwo, settings.blitMaterial, 0);
                            }
                        }

                        if (settings.Downsample == 1)
                        {
                            Blitter.BlitCameraTexture(cmd, rtDownsampleOne, camTarget, settings.blitMaterial, 0);
                        }
                        else if (settings.Downsample == 2)
                        {
                            Blitter.BlitCameraTexture(cmd, rtDownsampleTwo, camTarget, settings.blitMaterial, 0);
                        }

                    }
                }
            }
            // Execute Command Buffer one last time and release it
            // (otherwise we get weird recursive list in Frame Debugger)
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd) { }

        // Cleanup Called by feature below
        public void Dispose()
        {
            if (settings.colorTargetDestinationID != "")
                rtCustomColor?.Release();
            rtDownsampleOne?.Release();
            rtDownsampleTwo?.Release();
        }
    }

    // Exposed Settings

    [System.Serializable]
    public class Settings
    {
        public bool showInSceneView = true;
        public RenderPassEvent _event = RenderPassEvent.AfterRenderingOpaques;

        [Header("Draw Renderers Settings")]
        public LayerMask layerMask = 1;
        public Material overrideMaterial;
        public int overrideMaterialPass;
        public string colorTargetDestinationID = "";

        [Header("Blit Settings")]
        public Material blitMaterial;

        [Header("Quantization Settings")]
        [Tooltip("0, 1, OR 2 ONLY for level of Downsample")] public int Downsample;
    }

    public Settings settings = new Settings();

    // Feature Methods

    private DownsamplePass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new DownsamplePass(settings, name);
        m_ScriptablePass.renderPassEvent = settings._event;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        CameraType cameraType = renderingData.cameraData.cameraType;
        if (cameraType == CameraType.Preview) return; // Ignore feature for editor/inspector previews & asset thumbnails
        if (!settings.showInSceneView && cameraType == CameraType.SceneView) return;
        renderer.EnqueuePass(m_ScriptablePass);
    }

    protected override void Dispose(bool disposing)
    {
        m_ScriptablePass.Dispose();
    }
}

