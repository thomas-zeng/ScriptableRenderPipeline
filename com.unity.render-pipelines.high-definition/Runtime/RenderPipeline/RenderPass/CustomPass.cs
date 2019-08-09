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
            Opaque = HDRenderQueue.RenderQueueType.Opaque,
            AfterPostProcessOpaque = HDRenderQueue.RenderQueueType.AfterPostProcessOpaque,
            PreRefraction = HDRenderQueue.RenderQueueType.PreRefraction,
            Transparent = HDRenderQueue.RenderQueueType.Transparent,
            LowTransparent = HDRenderQueue.RenderQueueType.LowTransparent,
            AfterPostprocessTransparent = HDRenderQueue.RenderQueueType.AfterPostprocessTransparent,
            All = -1,
        }

        [System.Serializable]
        public class CustomPassSettings
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
            public CustomPassRenderQueueType    renderQueueType = CustomPassRenderQueueType.Opaque;
            public string[]                     passNames;
            public LayerMask                    layerMask = -1;
            public SortingCriteria              sortingCriteria = SortingCriteria.CommonOpaque;

            // Override material
            public Material         overrideMaterial = null;
            public int              overrideMaterialPassIndex = 0;

            // Fullscreen pass settingsL
            public Material         fullscreenPassMaterial;
        }

        [SerializeField]
        internal CustomPassSettings settings = new CustomPassSettings();

        // TODO: static factory to create CustomPass (common settings in parameter)

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
                ExecuteFullScreen(cmd);
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
            ShaderTagId[] unlitShaderTags = {
                HDShaderPassNames.s_ForwardName,
                HDShaderPassNames.s_ForwardOnlyName,        // HD Unlit shader
                HDShaderPassNames.s_SRPDefaultUnlitName     // Cross SRP Unlit shader
            };

            ShaderTagId[] shaderPasses = new ShaderTagId[settings.passNames.Length];
            for (int i = 0; i < settings.passNames.Length; i++)
                shaderPasses[i] = new ShaderTagId(settings.passNames[i]);
 
            var renderQueueType = (HDRenderQueue.RenderQueueType)settings.renderQueueType;

            var result = new RendererListDesc(settings.isHDRPShader ? unlitShaderTags : shaderPasses, cullResults, hdCamera.camera)
            {
                rendererConfiguration = PerObjectData.None,
                renderQueueRange = HDRenderQueue.GetRange(renderQueueType),
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
        protected void ExecuteFullScreen(CommandBuffer cmd)
        {
            if (settings.fullscreenPassMaterial != null)
            {
                CoreUtils.DrawFullScreen(cmd, settings.fullscreenPassMaterial, (MaterialPropertyBlock)null);
            }
        }
    }
}
