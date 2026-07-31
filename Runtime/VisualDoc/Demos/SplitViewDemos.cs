using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the SplitView documentation page.</summary>
    static class SplitViewDemos
    {
        [VisualDocDemo("splitview")]
        static VisualElement Basic()
        {
            var splitView = new SplitView(Direction.Horizontal);
            splitView.AddToClassList("demo-block");

            var leftPane = new Pane();
            leftPane.Add(new Text("Left Panel"));

            var rightPane = new Pane { stretch = true };
            rightPane.Add(new Text("Right Panel"));

            splitView.AddPane(leftPane);
            splitView.AddPane(rightPane);

            return splitView;
        }
    }
}
