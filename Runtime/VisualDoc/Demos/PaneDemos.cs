using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Pane documentation page.</summary>
    static class PaneDemos
    {
        [VisualDocDemo("pane")]
        static VisualElement Basic()
        {
            var splitView = new SplitView();
            splitView.AddToClassList("demo-block");

            var leftPane = new Pane();
            leftPane.Add(new Text("Fixed pane"));

            var rightPane = new Pane { stretchFactor = 1 };
            rightPane.Add(new Text("Flexible pane"));

            splitView.AddPane(leftPane);
            splitView.AddPane(rightPane);

            return DemoUtils.Row(splitView);
        }
    }
}
