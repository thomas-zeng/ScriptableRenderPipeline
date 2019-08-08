using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace UnityEditor.Rendering.HighDefinition
{
    class MaterialModificationProcessor : AssetModificationProcessor
    {
        static void OnWillCreateAsset(string asset)
        {
            if (!asset.ToLowerInvariant().EndsWith(".mat"))
                return;

            MaterialPostprocessor.s_CreatedAssets.Add(asset);
        }
    }

    class MaterialReimporter : Editor
    {
        [InitializeOnLoadMethod]
        static void ReimportAllMaterials()
        {
            //Check to see if the upgrader has been run for this project/HDRP version
            PackageManager.PackageInfo hdrpInfo = PackageManager.PackageInfo.FindForAssembly(Assembly.GetAssembly(typeof(HDRenderPipeline)));
            var hdrpVersion = hdrpInfo.version;
            var curUpgradeVersion = HDProjectSettings.packageVersionForMaterialUpgrade;

            if (curUpgradeVersion != hdrpVersion)
            {
                string[] guids = AssetDatabase.FindAssets("t:material", null);

                foreach (var asset in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(asset);
                    AssetDatabase.ImportAsset(path);
                }
                HDProjectSettings.packageVersionForMaterialUpgrade = hdrpVersion;
            }
        }
    }

    class MaterialPostprocessor : AssetPostprocessor
    {
        internal static List<string> s_CreatedAssets = new List<string>();

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (var asset in importedAssets)
            {
                if (!asset.ToLowerInvariant().EndsWith(".mat"))
                    continue;

                var material = (Material)AssetDatabase.LoadAssetAtPath(asset, typeof(Material));
                if (!HDEditorUtils.IsHDRPShader(material.shader, upgradable: true))
                    continue;

                ShaderPathID id = HDEditorUtils.GetShaderEnumFromShader(material.shader);
                var wasUpgraded = false;
                var assetVersions = AssetDatabase.LoadAllAssetsAtPath(asset);
                AssetVersion assetVersion = null;
                foreach (var subAsset in assetVersions)
                {
                    if (subAsset.GetType() == typeof(AssetVersion))
                    {
                        assetVersion = subAsset as AssetVersion;
                        break;
                    }
                }

                if (!assetVersion)
                {
                    wasUpgraded = true;
                    assetVersion = ScriptableObject.CreateInstance<AssetVersion>();
                    if (s_CreatedAssets.Contains(asset))
                    {
                        //set to latest asset version
                        s_CreatedAssets.Remove(asset);
                    }
                    else
                        assetVersion.version = 0;

                    assetVersion.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable;
                    AssetDatabase.AddObjectToAsset(assetVersion, asset);
                }

                //upgrade

                if (wasUpgraded)
                    EditorUtility.SetDirty(assetVersion);
            }
        }

    }
}
