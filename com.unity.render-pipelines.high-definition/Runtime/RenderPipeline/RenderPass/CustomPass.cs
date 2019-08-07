using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.HighDefinition
{
    enum CustomPassInjectionPoint
    {
        BeforeRendering,
        BeforeTransparent,
        BeforePostProcess,
        AfterPostProcess,
    }

    enum CustomPassType
    {
        Renderers,
        FullScreen
    }

    [System.Serializable]
    class CustomPass
    {
        public enum CustomPassRenderQueueType
        {
            Opaque = HDRenderQueue.RenderQueueType.Opaque,
            AfterPostProcessOpaque = HDRenderQueue.RenderQueueType.AfterPostProcessOpaque,
            PreRefraction = HDRenderQueue.RenderQueueType.PreRefraction,
            Transparent = HDRenderQueue.RenderQueueType.Transparent,
            LowTransparent = HDRenderQueue.RenderQueueType.LowTransparent,
            AfterPostprocessTransparent = HDRenderQueue.RenderQueueType.AfterPostprocessTransparent,
        }
        
        [System.Serializable]
        public class FilterSettings
        {
            public CustomPassRenderQueueType    renderQueueType;
            public LayerMask                    layerMask;
            public string[]                     passNames;

            public FilterSettings()
            {
                renderQueueType = CustomPassRenderQueueType.Opaque;
                layerMask = 0;
            }
        }

        public string           name;
        public CustomPassType   type;

        // Used only for the UI to keep track of the toggle state
        public bool             filterFoldout;
        public bool             rendererFoldout;
        public bool             passFoldout;

        //Filter settings
        public FilterSettings   filterSettings;

        // Override material
        public Material         overrideMaterial = null;
        public int              overrideMaterialPassIndex = 0;

        // Override depth state
        public bool             overrideDepth = false;
        public CompareFunction  depthCompareFunction = CompareFunction.LessEqual;
        public bool             writeDepth = true;

        // Fullscreen pass settingsL
        public Material         fullscreenPassMaterial;
        
        RenderStateBlock        renderStateBlock;
        DrawingSettings         drawSettings;
        FilteringSettings       filteringSettings;        

        CustomPass()
        {
            renderStateBlock = new RenderStateBlock();
            drawSettings = new DrawingSettings();
            filteringSettings = new FilteringSettings();
        }

        public void Execute(ScriptableRenderContext renderContext, CommandBuffer cmd, HDCamera camera, CullingResults cullingResult)
        {
            if (type == CustomPassType.Renderers)
                ExecuteRenderers(renderContext, cmd, camera, cullingResult);
            else
                ExecuteFullScreen(cmd);
        }

        RendererListDesc PrepareForwardEmissiveRendererList(CullingResults cullResults, HDCamera hdCamera)
        {
            ShaderTagId[] m_AllForwardOpaquePassNames = {    HDShaderPassNames.s_ForwardOnlyName,
                                                            HDShaderPassNames.s_ForwardName,
                                                            HDShaderPassNames.s_SRPDefaultUnlitName };

            var result = new RendererListDesc(m_AllForwardOpaquePassNames, cullResults, hdCamera.camera)
            {
                rendererConfiguration = 0,
                renderQueueRange = HDRenderQueue.k_RenderQueue_AllOpaque,
                sortingCriteria = SortingCriteria.CommonOpaque,
                stateBlock = null,
                overrideMaterial = null,
                excludeObjectMotionVectors = true
            };

            return result;
        }

        void ExecuteRenderers(ScriptableRenderContext renderContext, CommandBuffer cmd, HDCamera camera, CullingResults cullResults)
        {
            // Update the DrawRenderers settings with the values inside the pass
            renderStateBlock.depthState = new DepthState(writeDepth, depthCompareFunction);

            filteringSettings.layerMask = filterSettings.layerMask;
            filteringSettings.renderingLayerMask = 0xFFFFFFFF;
            filteringSettings.renderQueueRange = HDRenderQueue.GetRange((HDRenderQueue.RenderQueueType)filterSettings.renderQueueType);
            filteringSettings.sortingLayerRange = SortingLayerRange.all;

            SortingSettings sortingSettings = new SortingSettings(camera.camera) { criteria = SortingCriteria.CommonOpaque }; // criteria ???
            drawSettings = new DrawingSettings(new ShaderTagId(""), sortingSettings)
            {
                perObjectData = PerObjectData.None,
                enableInstancing = true,
                mainLightIndex = -1,
                enableDynamicBatching = false, // enable ?
            };

            drawSettings.overrideMaterial = overrideMaterial;
            drawSettings.overrideMaterialPassIndex = overrideMaterialPassIndex;

            // Debug.Log(drawSettings.sortingSettings.criteria);

            // renderContext.DrawRenderers(cullingResult, ref drawSettings, ref filteringSettings, ref renderStateBlock);
            // renderContext.DrawRenderers(cullingResult, ref drawSettings, ref filteringSettings);
            HDUtils.DrawRendererList(renderContext, cmd, RendererList.Create(PrepareForwardEmissiveRendererList(cullResults, camera)));
        }

        void ExecuteFullScreen(CommandBuffer cmd)
        {
            if (fullscreenPassMaterial != null)
            {
                MaterialPropertyBlock m = null;
                CoreUtils.DrawFullScreen(cmd, fullscreenPassMaterial, m);
            }
        }
    }
}
