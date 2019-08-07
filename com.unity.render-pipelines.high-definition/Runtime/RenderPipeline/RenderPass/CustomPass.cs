using System.Collections.Generic;
using UnityEngine.Rendering;

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
    class FilterSettings
    {
        public HDRenderQueue.RenderQueueType    renderQueueType;
        public LayerMask                        layerMask;
        public string[]                         passNames;

        public FilterSettings()
        {
            renderQueueType = HDRenderQueue.RenderQueueType.Opaque;
            layerMask = 0;
        }
    }

    [System.Serializable]
    class CustomPass
    {
        public string               name;
        public CustomPassType       type;
        public RenderStateBlock     renderStateBlock;
        public DrawingSettings      drawSettings;
        public FilteringSettings    filteringSettings;        

        // Used only for the UI to keep track of the toggle state
        public bool               filterFoldout;
        public bool               rendererFoldout;
        public bool               passFoldout;

        //Filter settings
        public FilterSettings       filterSettings;

        // Override material
        public Material overrideMaterial = null;
        public int overrideMaterialPassIndex = 0;

        // Override depth state
        public bool overrideDepth = false;
        public CompareFunction depthCompareFunction = CompareFunction.LessEqual;
        public bool writeDepth = true;

        public void Execute(ScriptableRenderContext renderContext, CullingResults cullingResult)
        {
            // TODO: Construct the filtering settings from FilterSettings

            renderContext.DrawRenderers(cullingResult, ref drawSettings, ref filteringSettings, ref renderStateBlock);
        }
    }
}
