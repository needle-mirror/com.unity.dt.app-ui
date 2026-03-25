using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.AppUI.Editor
{
    /// <summary>
    /// Unity build processor to add AppUISettings object to preloaded assets.
    /// </summary>
    public class AppUIBuildSetup : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        /// <summary>
        /// Callback order used internally by Unity.
        /// </summary>
        public int callbackOrder => 0;

        /// <summary>
        /// Ensures App UI Settings are initialized. Attempts to find an existing settings asset in the project,
        /// or creates a default instance if none are found.
        /// </summary>
        static void EnsureAppUISettingsAreInitialized()
        {
            // If settings are already set, nothing to do
            if (Core.AppUI.settings)
                return;

            // Try to find an existing settings asset in the project
            var settingsPaths = Utils.FindAppUISettingsInProject();
            foreach (var path in settingsPaths)
            {
                var settings = AssetDatabase.LoadAssetAtPath<AppUISettings>(path);
                if (settings != null)
                {
                    Core.AppUI.settings = settings;
                    Debug.Log($"Auto-initialized App UI Settings from: {path}");
                    return;
                }
            }

            // No settings asset found, create a default one
            Core.AppUI.settings = ScriptableObject.CreateInstance<AppUISettings>();
            Debug.Log("No App UI Settings asset found, using default settings instance.");
        }

        /// <summary>
        /// Called before build starts.
        /// </summary>
        /// <param name="report"> Unity Build report. </param>
        public void OnPreprocessBuild(BuildReport report)
        {
            EnsureAppUISettingsAreInitialized();

            if (!Core.AppUI.settings)
            {
                throw new System.InvalidOperationException("There's no App UI Settings set in your project. " +
                    "Please go to Edit > Project Settings > App UI and create a new Settings asset.");
            }

            Revert();

            if (Core.AppUI.settings.editorOnly)
                return;

            EnsureAppUISettingsArePreloaded();

            if (Core.AppUI.settings.includeShadersInPlayerBuild)
                EnsureShadersAreEmbedded();
        }

        static void EnsureAppUISettingsArePreloaded()
        {
            // If we operate on temporary object instead of AppUI setting asset,
            // adding temporary asset would result in preloadedAssets containing null object "{fileID: 0}".
            // Hence we ignore adding temporary objects to preloaded assets.
            if (EditorUtility.IsPersistent(Core.AppUI.settings))
            {
                // Add AppUISettings object assets, if it's not in there already.
                var preloadedAssets = PlayerSettings.GetPreloadedAssets().ToList();
                if (!preloadedAssets.Contains(Core.AppUI.settings))
                {
                    preloadedAssets.Add(Core.AppUI.settings);
                    PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
                }
            }
            else
            {
                var persistentAppUISettings = Resources.FindObjectsOfTypeAll<AppUISettings>()?.ToList() ?? new List<AppUISettings>();
                if (persistentAppUISettings.Count > 0)
                {
                    Debug.LogWarning("App UI Settings is not a persistent object. Skipping adding it to preloaded assets.\n" +
                        "If this is not intended, please make sure the App UI Settings object is saved to disk and selected in " +
                        "the Project Settings (Edit > Project Settings > App UI).");
                }
            }
        }

        /// <summary>
        /// Called after build finishes.
        /// </summary>
        /// <param name="report"> Unity Build report. </param>
        public void OnPostprocessBuild(BuildReport report)
        {
            Revert();
        }

        static void Revert()
        {
            // Revert back to original state by removing all AppUI settings from preloaded assets.
            PlayerSettings.SetPreloadedAssets(PlayerSettings.GetPreloadedAssets().Where(x => x is not AppUISettings).ToArray());
            RemoveAnyEmbeddedShaders();
        }

#if ENABLE_ALWAYS_INCLUDED_SHADERS
        static readonly string[] k_ShaderPaths = new string[]
        {
            "Packages/com.unity.dt.app-ui/PackageResources/Shaders/Box.shader",
            "Packages/com.unity.dt.app-ui/PackageResources/Shaders/CanvasBackground.shader",
            "Packages/com.unity.dt.app-ui/PackageResources/Shaders/CircularProgress.shader",
            "Packages/com.unity.dt.app-ui/PackageResources/Shaders/ColorSwatch.shader",
            "Packages/com.unity.dt.app-ui/PackageResources/Shaders/ColorWheel.shader",
            "Packages/com.unity.dt.app-ui/PackageResources/Shaders/LinearProgress.shader",
            "Packages/com.unity.dt.app-ui/PackageResources/Shaders/Mask.shader",
            "Packages/com.unity.dt.app-ui/PackageResources/Shaders/SVSquare.shader",
        };
#endif

        static void EnsureShadersAreEmbedded()
        {
            var graphicsSettingsObj = AssetDatabase.LoadAssetAtPath<GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");
            var serializedObject = new SerializedObject(graphicsSettingsObj);
            var preloadedShadersProperty = serializedObject.FindProperty("m_PreloadedShaders");
            var collection = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>("Packages/com.unity.dt.app-ui/PackageResources/Shaders/App UI Shaders.shadervariants");

            var changed = Utils.AddItemInArray(preloadedShadersProperty, collection);
#if ENABLE_ALWAYS_INCLUDED_SHADERS
            var alwaysIncludedShadersProperty = serializedObject.FindProperty("m_AlwaysIncludedShaders");
            foreach (var shaderPath in k_ShaderPaths)
            {
                var shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
                if (shader == null)
                    throw new System.InvalidOperationException($"Shader not found at path: {shaderPath}");
                changed |= Utils.AddItemInArray(alwaysIncludedShadersProperty, shader);
            }
#endif

            if (changed)
            {
                serializedObject.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
            }
        }

        static void RemoveAnyEmbeddedShaders()
        {
            var graphicsSettingsObj = AssetDatabase.LoadAssetAtPath<GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");
            var serializedObject = new SerializedObject(graphicsSettingsObj);
            var preloadedShadersProperty = serializedObject.FindProperty("m_PreloadedShaders");
            var collection = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>("Packages/com.unity.dt.app-ui/PackageResources/Shaders/App UI Shaders.shadervariants");
            var collectionIndex = Utils.IndexOf(preloadedShadersProperty, collection);
            var changed = collectionIndex != -1;
            if (changed)
                preloadedShadersProperty.DeleteArrayElementAtIndex(collectionIndex);

#if ENABLE_ALWAYS_INCLUDED_SHADERS
            var alwaysIncludedShadersProperty = serializedObject.FindProperty("m_AlwaysIncludedShaders");
            var foundShaders = new List<int>();
            foreach (var shaderPath in k_ShaderPaths)
            {
                var shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
                if (shader == null)
                    throw new System.InvalidOperationException($"Shader not found at path: {shaderPath}");
                var shaderIndex = Utils.IndexOf(alwaysIncludedShadersProperty, shader);
                if (shaderIndex != -1)
                {
                    foundShaders.Add(shaderIndex);
                    changed = true;
                }
            }

            foundShaders
                .OrderByDescending(x => x)
                .ToList()
                .ForEach(x => alwaysIncludedShadersProperty.DeleteArrayElementAtIndex(x));
#endif

            if (changed)
            {
                serializedObject.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
            }
        }
    }
}
