using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Text documentation page.</summary>
    static class TextDemos
    {
        [VisualDocDemo("text")]
        static VisualElement Hierarchy()
        {
            return DemoUtils.Column(
                new Text("Welcome!") { size = TextSize.XXL, primary = true },
                new Text("Please read the following instructions") { size = TextSize.L, primary = false },
                new Text("Detailed instructions go here...") { size = TextSize.M });
        }
    }
}
