using UnityEngine.UIElements;
using Unity.AppUI.UI;
using TextField = Unity.AppUI.UI.TextField;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the InputLabel documentation page.</summary>
    static class InputLabelDemos
    {
        [VisualDocDemo("inputlabel")]
        static VisualElement Basic()
        {
            var horizontal = new InputLabel { direction = Direction.Horizontal, label = "Name" };
            horizontal.Add(new TextField());
            horizontal.AddToClassList("demo-field");

            var required = new InputLabel
            {
                direction = Direction.Horizontal,
                label = "Email",
                required = true,
                helpMessage = "The value is not valid.",
                helpVariant = HelpTextVariant.Destructive
            };
            required.Add(new TextField());
            required.AddToClassList("demo-field");

            return DemoUtils.Column(horizontal, required);
        }
    }
}
