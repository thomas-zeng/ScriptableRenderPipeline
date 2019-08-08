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
            public string           name = "Custom Pass";
            public bool             enabled = true;
            public CustomPassType   type;

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

        public virtual void Execute(ScriptableRenderContext renderContext, CommandBuffer cmd, HDCamera camera, CullingResults cullingResult)
        {
            if (settings.type == CustomPassType.Renderers)
                ExecuteRenderers(renderContext, cmd, camera, cullingResult);
            else
                ExecuteFullScreen(cmd);
        }

        protected void ExecuteRenderers(ScriptableRenderContext renderContext, CommandBuffer cmd, HDCamera hdCamera, CullingResults cullResults)
        {
            ShaderTagId[] unlitShaderTags = {
                HDShaderPassNames.s_ForwardName,
                HDShaderPassNames.s_ForwardOnlyName,        // HD Unlit shader
                HDShaderPassNames.s_SRPDefaultUnlitName     // Cross SRP Unlit shader
            };
 
            var renderQueueType = (HDRenderQueue.RenderQueueType)settings.renderQueueType;

            var result = new RendererListDesc(unlitShaderTags, cullResults, hdCamera.camera)
            {
                rendererConfiguration = PerObjectData.None,
                renderQueueRange = HDRenderQueue.GetRange(renderQueueType),
                sortingCriteria = settings.sortingCriteria,
                excludeObjectMotionVectors = true,
                overrideMaterial = settings.overrideMaterial,
                overrideMaterialPassIndex = settings.overrideMaterialPassIndex,
            };

            HDUtils.DrawRendererList(renderContext, cmd, RendererList.Create(result));
        }

        protected void ExecuteFullScreen(CommandBuffer cmd)
        {
            if (settings.fullscreenPassMaterial != null)
            {
                CoreUtils.DrawFullScreen(cmd, settings.fullscreenPassMaterial, (MaterialPropertyBlock)null);
            }
        }
    }
}
