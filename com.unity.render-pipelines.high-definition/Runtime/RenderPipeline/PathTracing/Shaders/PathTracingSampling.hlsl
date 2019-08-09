// Add path-tracing specific routines for convenience, in case we want to change them.
// For now, simply reuse the ray tracing ones.

#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/Raytracing/Shaders/RaytracingSampling.hlsl"

uint2 GetScramblingValue(uint2 pixelCoord)
{
	return ScramblingValue(pixelCoord.x, pixelCoord.y);
}

float GetSample(uint index, uint dim, uint scrambling = 0)
{
	return GetRaytracingNoiseSample(index, dim, scrambling);
}
