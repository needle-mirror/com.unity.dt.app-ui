using UnityEngine.UIElements;
using Unity.AppUI.UI;
using Button = Unity.AppUI.UI.Button;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Canvas documentation page.</summary>
    static class CanvasDemos
    {
        [VisualDocDemo("canvas")]
        static VisualElement Basic()
        {
            var canvas = new Canvas();
            canvas.AddToClassList("demo-block");
            canvas.Add(new Button { title = "Zoom Content" });
            canvas.Add(new Text("Pan me around!"));
            return DemoUtils.Row(canvas);
        }
    }
}
