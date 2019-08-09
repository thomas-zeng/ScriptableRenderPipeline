#ifndef CUSTOM_PASS_RENDERERS
#define CUSTOM_PASS_RENDERERS

#define SHADERPASS SHADERPASS_FORWARD_UNLIT

//-------------------------------------------------------------------------------------
// Define
//-------------------------------------------------------------------------------------

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"

#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Unlit/UnlitProperties.hlsl"

#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Unlit/Unlit.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Unlit/ShaderPass/UnlitSharePass.hlsl"

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Sampling/SampleUVMapping.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalUtilities.hlsl"

// Note: when you update this struct, be sure to reflect the changes to the CustomPassRenderersShader.template
// We wrap SurfaceData and Builtin data into one struct for user API
struct CustomPassOutput
{
    float3      color;
    float       opacity;
    float3      emissiveColor;
};

// Note: when you update this struct, be sure to reflect the changes to the CustomPassRenderersShader.template
struct CustomPassInput
{
    float3      viewDirection;
    // float3      worldSpaceNormal;
    // float3      worldSpaceTangent;
    // float3      worldSpaceBiTangent;
    float3      worldSpacePosition; // relative to the camera
    float2      screenSpacePosition;
    float2      uv;
    float4      color; // vertex color
};

void GetCustomRenderersData(in CustomPassInput input, inout CustomPassOutput output);

CustomPassInput ConvertInputsToCustomPassInput(FragInputs fragInputs, float3 V)
{
    CustomPassInput input;

    input.viewDirection = V;
    input.uv = fragInputs.texCoord0.xy;
    // does not works currently
    // input.worldSpaceTangent = fragInputs.tangentToWorld[0].xyz;
    // input.worldSpaceBiTangent = fragInputs.tangentToWorld[1].xyz;
    // input.worldSpaceNormal = fragInputs.tangentToWorld[2].xyz;
    input.worldSpacePosition = fragInputs.positionRWS;
    input.screenSpacePosition = fragInputs.positionSS.xy;
    input.color = fragInputs.color;

    return input;
}

void GetSurfaceAndBuiltinData(FragInputs fragInputs, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData)
{
    CustomPassOutput output;
    ZERO_INITIALIZE(CustomPassOutput, output);
    
    GetCustomRenderersData(ConvertInputsToCustomPassInput(fragInputs, V), output);

    // Write back the data to the initial structures
    ZERO_INITIALIZE(BuiltinData, builtinData); // No call to InitBuiltinData as we don't have any lighting
    builtinData.opacity = output.opacity;
    builtinData.emissiveColor = output.emissiveColor;
    surfaceData.color = output.color;
}

#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassForwardUnlit.hlsl"

#endif // CUSTOM_PASS_RENDERERS