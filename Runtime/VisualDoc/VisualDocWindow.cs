#if UNITY_EDITOR
using System;
using Unity.AppUI.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.VisualDoc
{
    class VisualDocWindow : EditorWindow
    {
        [UnityEditor.MenuItem("Window/App UI/Visual Documentation", priority = 2100)]
        static void OpenDocumentation()
        {
            var window = EditorWindow.GetWindow<VisualDocWindow>();
            window.titleContent = new GUIContent("App UI Visual Documentation");
            window.minSize = new Vector2(400, 400);
            window.Show();
        }

        void CreateGUI()
        {
            var panel = new Panel();
            panel.AddToClassList("unity-editor");
            panel.StretchToParentSize();
            var theme = EditorGUIUtility.isProSkin ? "Dark" : "Light";
            var styleSheet = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>($"Packages/com.unity.dt.app-ui/PackageResources/Styles/Themes/App UI - {theme} - Medium.tss");
            rootVisualElement.styleSheets.Add(styleSheet);
            rootVisualElement.Add(panel);
            var visualDocView = new VisualDocView();
            panel.Add(visualDocView);
            visualDocView.StretchToParentSize();
        }
    }
}
#endif // UNITY_EDITOR
