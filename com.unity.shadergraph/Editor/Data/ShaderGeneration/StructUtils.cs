using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Graphing;
using UnityEditor.ShaderGraph;

namespace UnityEditor.ShaderGraph
{
    delegate void OnGeneratePassDelegate(IMasterNode masterNode, ref Pass pass, ref ShaderGraphRequirements requirements);
    struct Pass
    {
        public string Name;
        public string LightMode;
        public string ShaderPassName;
        public List<string> Includes;
        public string TemplateName;
        public string MaterialName;
        public List<string> ExtraInstancingOptions;
        public List<string> ExtraDefines;
        public List<int> VertexShaderSlots;         // These control what slots are used by the pass vertex shader
        public List<int> PixelShaderSlots;          // These control what slots are used by the pass pixel shader
        public string CullOverride;
        public string BlendOverride;
        public string BlendOpOverride;
        public string ZTestOverride;
        public string ZWriteOverride;
        public string ColorMaskOverride;
        public string ZClipOverride;
        public List<string> StencilOverride;
        public List<string> RequiredFields;         // feeds into the dependency analysis (HDRP)
        public ShaderGraphRequirements Requirements; //Universal requirement collection
        public bool UseInPreview;

        // All these lists could probably be hashed to aid lookups.
        public bool VertexShaderUsesSlot(int slotId)
        {
            return VertexShaderSlots.Contains(slotId);
        }
        public bool PixelShaderUsesSlot(int slotId)
        {
            return PixelShaderSlots.Contains(slotId);
        }
        public void OnGeneratePass(IMasterNode masterNode, ShaderGraphRequirements requirements)
        {
            if (OnGeneratePassImpl != null)
            {
                OnGeneratePassImpl(masterNode, ref this, ref requirements);                
            }
        }
        public OnGeneratePassDelegate OnGeneratePassImpl;
    }
    
}

