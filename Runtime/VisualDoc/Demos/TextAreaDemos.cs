using UnityEngine.UIElements;
using Unity.AppUI.UI;
using TextArea = Unity.AppUI.UI.TextArea;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the TextArea documentation page.</summary>
    static class TextAreaDemos
    {
        [VisualDocDemo("textarea")]
        static VisualElement Basic()
        {
            var textArea = new TextArea { placeholder = "Enter your message here..." };
            textArea.AddToClassList("demo-field");
            return DemoUtils.Row(textArea);
        }
    }
}
