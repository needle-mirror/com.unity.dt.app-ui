using System;
using Unity.AppUI.Redux.DevTools.UI;
using UnityEditor;

namespace Unity.AppUI.Editor.Redux
{
    /// <summary>
    /// A window that provides a set of tools for debugging and inspecting the state of the store.
    /// </summary>
    public class DevToolsWindow : EditorWindow
    {
        /// <summary>
        /// Show a Redux DevTools editor window.
        /// </summary>
        //[MenuItem("Window/Analysis/Redux DevTools")]
        static void ShowWindow()
        {
            var window = CreateWindow<DevToolsWindow>();
            window.titleContent = new UnityEngine.GUIContent("DevTools");
            window.Show();
        }

        void CreateGUI()
        {
            var root = rootVisualElement;
            var devToolsGUI = new DevToolsView();
            root.Add(devToolsGUI);
        }
    }
}
