using UnityEngine.TestTools;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace Unity.AppUI.Tests
{
    // Players load the default theme from Resources, which the package does not ship.
    // Applied per fixture because the test framework skips assembly attributes here.
    class ThemeResourceSetup : IPrebuildSetup, IPostBuildCleanup
    {
#if UNITY_EDITOR
        const string k_ResourcePath = "Assets/Resources/Themes/App UI.tss";
        const string k_CreatedKey = "Unity.AppUI.Tests.ThemeResourceCreated";
        const string k_ThemeContents =
            "@import url(\"/Packages/com.unity.dt.app-ui/PackageResources/Styles/Themes/App UI.tss\");\n\nVisualElement {}\n";
#endif

        public void Setup()
        {
#if UNITY_EDITOR
            if (File.Exists(k_ResourcePath))
                return;

            Directory.CreateDirectory(Path.GetDirectoryName(k_ResourcePath));
            File.WriteAllText(k_ResourcePath, k_ThemeContents);
            AssetDatabase.ImportAsset(k_ResourcePath);
            SessionState.SetBool(k_CreatedKey, true);
#endif
        }

        public void Cleanup()
        {
#if UNITY_EDITOR
            if (!SessionState.GetBool(k_CreatedKey, false))
                return;

            AssetDatabase.DeleteAsset(k_ResourcePath);
            SessionState.EraseBool(k_CreatedKey);
#endif
        }
    }
}
