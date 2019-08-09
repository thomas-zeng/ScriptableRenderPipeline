#ifndef CUSTOM_PASS_COMMON
#define CUSTOM_PASS_COMMON

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonLighting.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

struct Attributes
{
    uint vertexID : SV_VertexID;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    UNITY_VERTEX_OUTPUT_STEREO
};

// If you add somthing here, be sure to update the associated template file: CustomPassShader.template
struct CustomPassInputs
{
    float2      screenSpaceUV;
    uint2       screenSpacePixelCoordinates;
    float3      viewDirection;
    float       rawDepth;
    float3      worldSpacePosition; // Can be camera-relative
};

Varyings Vert(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
    output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID, UNITY_RAW_FAR_CLIP_VALUE);
    return output;
}

CustomPassInputs LoadPassInputs(Varyings input)
{
    CustomPassInputs customPassInputs;
    ZERO_INITIALIZE(CustomPassInputs, customPassInputs);

    float depth = LoadCameraDepth(input.positionCS.xy);
    PositionInputs posInput = GetPositionInput(input.positionCS.xy, _ScreenSize.zw, depth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V, float2(0, 0));

    // Copy data to a more user friendly structure
    customPassInputs.screenSpaceUV = posInput.positionNDC;
    customPassInputs.worldSpacePosition = posInput.positionWS;
    customPassInputs.screenSpacePixelCoordinates = posInput.positionSS;
    customPassInputs.viewDirection = GetWorldSpaceNormalizeViewDir(posInput.positionWS);
    customPassInputs.rawDepth = depth;

    return customPassInputs;
}

#endif // CUSTOM_PASS_COMMON