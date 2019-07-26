using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEditor.ShaderGraph;
using UnityEditor.ShaderGraph.Drawing;

namespace UnityEditor.Rendering.HighDefinition
{
    class RayTracingNode
    {
        private const string k_KeywordHigh = "RAYTRACING_SHADER_GRAPH_HIGH";
        private const string k_KeywordLow = "RAYTRACING_SHADER_GRAPH_LOW";

        static ShaderKeyword Keyword = new ShaderKeyword(ShaderKeywordType.Enum, false)
        {
            displayName = "Raytracing",
            overrideReferenceName = "RAYTRACING_SHADER_GRAPH",
            isEditable = false,
            keywordDefinition = ShaderKeywordDefinition.Predefined,
            entries = new List<ShaderKeywordEntry>()
            {
                new ShaderKeywordEntry(1, "Low", "LOW"),
                new ShaderKeywordEntry(2, "High",  "HIGH"),
            },
        };

        public enum RaytracingVariant
        {
            High,
            Low
        }

        public static string RaytracingVariantKeyword(RaytracingVariant variant)
        {
            switch (variant)
            {
                case RaytracingVariant.High: return k_KeywordHigh;
                case RaytracingVariant.Low: return k_KeywordLow;
                default: throw new ArgumentOutOfRangeException(nameof(variant));
            }
        }

        [CustomKeywordNodeProvider]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        static IEnumerable<ShaderKeyword> GetRayTracingKeyword() => Enumerable.Repeat(Keyword, 1);
    }
}
