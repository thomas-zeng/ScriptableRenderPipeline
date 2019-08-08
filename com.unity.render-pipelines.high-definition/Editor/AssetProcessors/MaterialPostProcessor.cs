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
                var migrations = k_Migrations[id];
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
                        //set to latest asset version (migration.length)
                        assetVersion.version = migrations.Length;
                        s_CreatedAssets.Remove(asset);
                    }
                    else
                        assetVersion.version = 0;

                    assetVersion.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable;
                    AssetDatabase.AddObjectToAsset(assetVersion, asset);
                }

                //upgrade
                while (assetVersion.version < migrations.Length)
                {
                    migrations[assetVersion.version](material);
                    assetVersion.version++;
                    wasUpgraded = true;
                }

                if (wasUpgraded)
                    EditorUtility.SetDirty(assetVersion);
            }
        }

        // /!\ NEVER change order of migration function. Only add at end !
        static readonly Dictionary<ShaderPathID, Action<Material>[]> k_Migrations = new Dictionary<ShaderPathID, Action<Material>[]>
        {
            { ShaderPathID.Lit, new Action<Material>[] { /* EmissiveIntensityToColor, SecondMigrationStep, ... */ } },
            { ShaderPathID.LitTesselation, new Action<Material>[] {} },
            { ShaderPathID.LayeredLit, new Action<Material>[] {} },
            { ShaderPathID.LayeredLitTesselation, new Action<Material>[] {} },
            { ShaderPathID.StackLit, new Action<Material>[] {} },
            { ShaderPathID.Unlit, new Action<Material>[] {} },
            { ShaderPathID.Fabric, new Action<Material>[] {} },
            { ShaderPathID.Decal, new Action<Material>[] {} },
            { ShaderPathID.TerrainLit, new Action<Material>[] {} },
            { ShaderPathID.SG_Unlit, new Action<Material>[] {} },
            { ShaderPathID.SG_Lit, new Action<Material>[] {} },
            { ShaderPathID.SG_Hair, new Action<Material>[] {} },
            { ShaderPathID.SG_Fabric, new Action<Material>[] {} },
            { ShaderPathID.SG_StackLit, new Action<Material>[] {} },
            { ShaderPathID.SG_Decal, new Action<Material>[] {} },
        };

        #region Migrations

        //exemple migration method, remove it after first real migration
        //static void EmissiveIntensityToColor(Material material)
        //{
        //    var emissiveIntensity = material.GetFloat("_EmissiveIntensity");

        //    var emissiveColor = Color.black;
        //    if (material.HasProperty("_EmissiveColor"))
        //        emissiveColor = material.GetColor("_EmissiveColor");
        //    emissiveColor *= emissiveIntensity;
        //    emissiveColor.a = 1.0f;

        //    material.SetColor("_EmissiveColor", emissiveColor);
        //    material.SetColor("_EmissionColor", Color.white);
        //}

        #endregion
    }
}
