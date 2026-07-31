using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Panel documentation page.</summary>
    static class PanelDemos
    {
        [VisualDocDemo("panel")]
        static VisualElement Basic()
        {
            var panel = new Panel();
            panel.AddToClassList("demo-block");
            panel.Add(new Heading("Welcome to App UI"));
            panel.Add(new Text("Main content goes here"));

            return DemoUtils.Row(panel);
        }
    }
}
