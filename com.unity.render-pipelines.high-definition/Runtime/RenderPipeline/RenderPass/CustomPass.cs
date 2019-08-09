using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.HighDefinition
{
    /// <summary>
    /// List all the injection points available for HDRP
    /// </summary>
    public enum CustomPassInjectionPoint
    {
        BeforeRendering,
        BeforeTransparent,
        BeforePostProcess,
        AfterPostProcess,
    }

    /// <summary>
    /// Type of the custom pass
    /// </summary>
    public enum CustomPassType
    {
        Renderers,
        FullScreen
    }

    /// <summary>
    /// Used to select the target buffer when executing the custom pass
    /// </summary>
    public enum CustomPassTargetBuffer
    {
        Camera,
        Custom,
    }

    /// <summary>
    /// Class that holds data and logic for the pass to be executed
    /// </summary>
    [System.Serializable]
    public class CustomPass : ScriptableObject // We need this to be a scriptableObject in order to make CustomPropertyDrawer work ...
    {
        public enum CustomPassRenderQueueType
        {
            OpaqueNoAlphaTest,
            OpaqueAlphaTest,
            AllOpaque,
            AfterPostProcessOpaque,
            PreRefraction,
            Transparent,
            LowTransparent,
            AllTransparent,
            AllTransparentWithLowRes,
            AfterPostProcessTransparent,
            All,
        }

        [System.Serializable]
        internal class CustomPassSettings
        {
            public string                   name = "Custom Pass";
            public bool                     enabled = true;
            public CustomPassType           type;
            public CustomPassTargetBuffer   targetColorBuffer;
            public CustomPassTargetBuffer   targetDepthBuffer;
            public ClearFlag                clearFlags;
            public bool                     isHDRPShader;

            // Used only for the UI to keep track of the toggle state
            public bool             filterFoldout;
            public bool             rendererFoldout;
            public bool             passFoldout;

            //Filter settings
            public CustomPassRenderQueueType    renderQueueType = CustomPassRenderQueueType.AllOpaque;
            public string[]                     passNames = new string[1] { "Forward" };
            public LayerMask                    layerMask = -1;
            public SortingCriteria              sortingCriteria = SortingCriteria.CommonOpaque;

            // Override material
            public Material         overrideMaterial = null;
            public int              overrideMaterialPassIndex = 0;

            // Fullscreen pass settingsL
            public Material         fullscreenPassMaterial;
        }

        static List<ShaderTagId> m_HDRPShaderTags;
        static List<ShaderTagId> hdrpShaderTags
        {
            get
            {
                if (m_HDRPShaderTags == null)
                {
                    m_HDRPShaderTags = new List<ShaderTagId>() {
                        HDShaderPassNames.s_ForwardName,
                        HDShaderPassNames.s_ForwardOnlyName,        // HD Unlit shader
                        HDShaderPassNames.s_SRPDefaultUnlitName,    // Cross SRP Unlit shader
                    };
                }
                return m_HDRPShaderTags;
            }
        }

        [SerializeField]
        internal CustomPassSettings settings = new CustomPassSettings();

        /// <summary>
        /// Create a custom pass to execute a fullscreen pass
        /// </summary>
        /// <param name="fullScreenMaterial"></param>
        /// <param name="targetColorBuffer"></param>
        /// <param name="targetDepthBuffer"></param>
        /// <returns></returns>
        public static CustomPass CreateFullScreenPass(Material fullScreenMaterial, CustomPassTargetBuffer targetColorBuffer = CustomPassTargetBuffer.Camera,
            CustomPassTargetBuffer targetDepthBuffer = CustomPassTargetBuffer.Camera)
        {
            var pass = ScriptableObject.CreateInstance<CustomPass>();
            pass.settings.type = CustomPassType.FullScreen;
            pass.settings.targetColorBuffer = targetColorBuffer;
            pass.settings.targetDepthBuffer = targetDepthBuffer;
            pass.settings.fullscreenPassMaterial = fullScreenMaterial;

            return pass;
        }

        /// <summary>
        /// Create a Custom Pass to render objects
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="mask"></param>
        /// <param name="overrideMaterial"></param>
        /// <param name="overrideMaterialPassIndex"></param>
        /// <param name="sorting"></param>
        /// <param name="clearFlags"></param>
        /// <param name="targetColorBuffer"></param>
        /// <param name="targetDepthBuffer"></param>
        /// <returns></returns>
        public static CustomPass CreateFullScreenPass(CustomPassRenderQueueType queue, LayerMask mask,
            Material overrideMaterial, int overrideMaterialPassIndex = 0, SortingCriteria sorting = SortingCriteria.CommonOpaque,
            ClearFlag clearFlags = ClearFlag.None, CustomPassTargetBuffer targetColorBuffer = CustomPassTargetBuffer.Camera,
            CustomPassTargetBuffer targetDepthBuffer = CustomPassTargetBuffer.Camera)
        {
            var pass = ScriptableObject.CreateInstance<CustomPass>();
            pass.settings.type = CustomPassType.Renderers;
            pass.settings.overrideMaterial = overrideMaterial;
            pass.settings.overrideMaterialPassIndex = overrideMaterialPassIndex;
            pass.settings.sortingCriteria = sorting;
            pass.settings.clearFlags = clearFlags;
            pass.settings.targetColorBuffer = targetColorBuffer;
            pass.settings.targetDepthBuffer = targetDepthBuffer;

            return pass;
        }

        internal void ExecuteInternal(ScriptableRenderContext renderContext, CommandBuffer cmd, HDCamera camera, CullingResults cullingResult, RTHandle cameraColorBuffer, RTHandle cameraDepthBuffer, RTHandle customColorBuffer, RTHandle customDepthBuffer)
        {
            SetCustomPassTarget(cmd, cameraColorBuffer, cameraDepthBuffer, customColorBuffer, customDepthBuffer);

            Execute(renderContext, cmd, camera, cullingResult);
            
            // Set back the camera color buffer is we were using a custom buffer as target
            if (settings.targetDepthBuffer != CustomPassTargetBuffer.Camera)
                CoreUtils.SetRenderTarget(cmd, cameraColorBuffer);
        }

        void SetCustomPassTarget(CommandBuffer cmd, RTHandle cameraColorBuffer, RTHandle cameraDepthBuffer, RTHandle customColorBuffer, RTHandle customDepthBuffer)
        {
            RTHandle colorBuffer = (settings.targetColorBuffer == CustomPassTargetBuffer.Custom) ? customColorBuffer : cameraColorBuffer;
            RTHandle depthBuffer = (settings.targetDepthBuffer == CustomPassTargetBuffer.Custom) ? customDepthBuffer : cameraDepthBuffer;
            CoreUtils.SetRenderTarget(cmd, colorBuffer, depthBuffer, settings.clearFlags);
        }

        /// <summary>
        /// Called when your pass needs to be executed by a camera
        /// </summary>
        /// <param name="renderContext"></param>
        /// <param name="cmd"></param>
        /// <param name="camera"></param>
        /// <param name="cullingResult"></param>
        protected virtual void Execute(ScriptableRenderContext renderContext, CommandBuffer cmd, HDCamera camera, CullingResults cullingResult)
        {
            if (settings.type == CustomPassType.Renderers)
                ExecuteRenderers(renderContext, cmd, camera, cullingResult);
            else
                ExecuteFullScreen(cmd, settings.fullscreenPassMaterial);
        }

        /// <summary>
        /// Returns the render queue range associated with the custom render queue type
        /// </summary>
        /// <returns></returns>
        protected RenderQueueRange GetRenderQueueRange(CustomPassRenderQueueType type)
        {
            switch (type)
            {
                case CustomPassRenderQueueType.OpaqueNoAlphaTest: return HDRenderQueue.k_RenderQueue_OpaqueNoAlphaTest;
                case CustomPassRenderQueueType.OpaqueAlphaTest: return HDRenderQueue.k_RenderQueue_OpaqueAlphaTest;
                case CustomPassRenderQueueType.AllOpaque: return HDRenderQueue.k_RenderQueue_AllOpaque;
                case CustomPassRenderQueueType.AfterPostProcessOpaque: return HDRenderQueue.k_RenderQueue_AfterPostProcessOpaque;
                case CustomPassRenderQueueType.PreRefraction: return HDRenderQueue.k_RenderQueue_PreRefraction;
                case CustomPassRenderQueueType.Transparent: return HDRenderQueue.k_RenderQueue_Transparent;
                case CustomPassRenderQueueType.LowTransparent: return HDRenderQueue.k_RenderQueue_LowTransparent;
                case CustomPassRenderQueueType.AllTransparent: return HDRenderQueue.k_RenderQueue_AllTransparent;
                case CustomPassRenderQueueType.AllTransparentWithLowRes: return HDRenderQueue.k_RenderQueue_AllTransparentWithLowRes;
                case CustomPassRenderQueueType.AfterPostProcessTransparent: return HDRenderQueue.k_RenderQueue_AfterPostProcessTransparent;
                case CustomPassRenderQueueType.All:
                default:
                    return HDRenderQueue.k_RenderQueue_All;
            }
        }

        /// <summary>
        /// Execute the pass with the renderers setup
        /// </summary>
        /// <param name="renderContext"></param>
        /// <param name="cmd"></param>
        /// <param name="hdCamera"></param>
        /// <param name="cullResults"></param>
        protected void ExecuteRenderers(ScriptableRenderContext renderContext, CommandBuffer cmd, HDCamera hdCamera, CullingResults cullResults)
        {
            ShaderTagId[] shaderPasses = new ShaderTagId[hdrpShaderTags.Count + ((settings.overrideMaterial != null) ? 1 : 0)];
            System.Array.Copy(hdrpShaderTags.ToArray(), shaderPasses, hdrpShaderTags.Count);
            if (settings.overrideMaterial != null)
            {
                shaderPasses[hdrpShaderTags.Count] = new ShaderTagId(settings.overrideMaterial.GetPassName(settings.overrideMaterialPassIndex));
            }

            if (shaderPasses.Length == 0)
            {
                Debug.LogWarning("Attempt to call DrawRenderers with an empty shader passes. Skipping the call to avoid errors");
                return;
            }

            var result = new RendererListDesc(shaderPasses, cullResults, hdCamera.camera)
            {
                rendererConfiguration = PerObjectData.None,
                renderQueueRange = GetRenderQueueRange(settings.renderQueueType),
                sortingCriteria = settings.sortingCriteria,
                excludeObjectMotionVectors = true,
                overrideMaterial = settings.overrideMaterial,
                overrideMaterialPassIndex = settings.overrideMaterialPassIndex,
                layerMask = settings.layerMask,
            };

            HDUtils.DrawRendererList(renderContext, cmd, RendererList.Create(result));
        }

        /// <summary>
        /// Execute the pass with the fullscreen setup
        /// </summary>
        /// <param name="cmd"></param>
        protected void ExecuteFullScreen(CommandBuffer cmd, Material fullScreenMaterial)
        {
            if (fullScreenMaterial != null)
            {
                CoreUtils.DrawFullScreen(cmd, fullScreenMaterial, (MaterialPropertyBlock)null);
            }
        }
    }
}
