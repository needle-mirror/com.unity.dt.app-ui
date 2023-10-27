using System;
using Unity.AppUI.Editor;
using Unity.AppUI.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Sample.Editor
{
    public class DragAndDropPage : StoryBookPage
    {
        public override string displayName => "Drag And Drop";

        public override Type componentType => null;

        public DragAndDropPage()
        {
            m_Stories.Add(new StoryBookStory("Main", () =>
            {
                var element = new VisualElement();
                var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Samples/App UI/0.5.5/Drag And Drop/DragAndDropUI.uxml");
                tree.CloneTree(element);
                var root = element.Q<VisualElement>("main-root");
                var desc = root.Q<Text>("dnd-desc");
                desc.text += "\nYou can also drag items from others panels of the Editor into the destination list to generate items based on dragged paths.";
                root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>("Assets/Samples/App UI/0.5.5/Drag And Drop/DragAndDropTheme.tss"));
                var script = new Unity.AppUI.Samples.DragAndDropSample.DragAndDropSampleScript();
                script.Start(root);
                return root;
            })); 
        }
    }
}
