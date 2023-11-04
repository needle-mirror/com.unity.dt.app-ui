using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Editor
{
    public class KitPage : StoryBookPage
    {
        public override string displayName => "Kit";

        public override Type componentType => null;

        public KitPage()
        {
            m_Stories.Add(new StoryBookStory("Main", () =>
            {
                var element = new VisualElement();
                var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Assets/Samples/App UI/1.0.0-pre.2/UI Kit/SampleResources/Examples.uxml");
                tree.CloneTree(element);
                var root = element.Q<VisualElement>("root-main");
                root.styleSheets.Add(
                    AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(
                        "Assets/Samples/App UI/1.0.0-pre.2/UI Kit/SampleResources/ExampleTheme.tss"));
                Samples.Examples.SetupDataBinding(root);
                root.Query(className: "example-context-switcher-panel").ForEach(visualElement => 
                    visualElement.style.display = DisplayStyle.None);
                return root;
            }));
        }
    }
}
