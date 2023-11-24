using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.AppUI.Tests
{
    static class Utils
    {
        static readonly MethodInfo k_ImportXmlFromString;

        static readonly object k_UxmlImporterImplInstance;

        static Utils()
        {
#if UNITY_EDITOR
            Type importerType = null;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.FullName == "UnityEditor.UIElements.UXMLImporterImpl")
                    {
                        importerType = type;
                        break;
                    }
                }

                if (importerType != null)
                    break;
            }

            if (importerType == null)
            {
                Debug.LogError("Could not find UXMLImporterImpl type");
            }
            else
            {
                k_UxmlImporterImplInstance = importerType.GetConstructors(
                        BindingFlags.Instance | BindingFlags.NonPublic)
                    .First(c => !c.GetParameters().Any())
                    .Invoke(Array.Empty<object>());

                k_ImportXmlFromString = importerType.GetMethod("ImportXmlFromString",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                if (k_ImportXmlFromString == null)
                    Debug.LogError("Could not find ImportXmlFromString method");
            }
#endif
        }

        internal static readonly string snapshotsOutputDir =
            Environment.GetEnvironmentVariable("SNAPSHOTS_OUTPUT_DIR") is {Length:>0} p ?
            Path.GetFullPath(p) : null;

        internal static IEnumerable<string> scales
        {
            get
            {
                yield return "small";
                yield return "medium";
            }
        }
        
        internal static IEnumerable<string> themes
        {
            get
            {
                yield return "dark";
                yield return "light";
            }
        }

        internal static bool FileAvailable(string path) 
        {
            if (!File.Exists(path)) 
                return false;

            var file = new FileInfo(path);
            FileStream stream = null;

            try 
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException) 
            {
                // Can be either:
                // - file is processed by another thread
                // - file is still being written to
                // - file does not really exist yet
                return false;
            }
            finally
            {
                stream?.Close();
            }

            return true;
        }

        internal static UIDocument ConstructTestUI()
        {
            var obj = new GameObject("TestUI");
            obj.AddComponent<Camera>();
            var doc = obj.AddComponent<UIDocument>();
            var panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            panelSettings.scaleMode = PanelScaleMode.ConstantPhysicalSize;
#if UNITY_EDITOR
            panelSettings.themeStyleSheet = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(
                "Packages/com.unity.dt.app-ui/PackageResources/Styles/Themes/App UI.tss");
#else
            panelSettings.themeStyleSheet = Resources.Load<ThemeStyleSheet>("Themes/App UI");
#endif
            doc.panelSettings = panelSettings;

            return doc;
        }

        internal static VisualTreeAsset LoadUxmlTemplateFromString(string contents)
        {
            // ReSharper disable once RedundantAssignment
            VisualTreeAsset vta = null;
#if UNITY_EDITOR
            var args = new object[] {contents, null};
            k_ImportXmlFromString.Invoke(k_UxmlImporterImplInstance, args);
            vta = args[1] as VisualTreeAsset;
#endif
            return vta;
        }
    }
}
