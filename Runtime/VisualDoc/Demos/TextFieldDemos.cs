using UnityEngine.UIElements;
using Unity.AppUI.UI;
using TextField = Unity.AppUI.UI.TextField;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the TextField documentation page.</summary>
    static class TextFieldDemos
    {
        [VisualDocDemo("textfield")]
        static VisualElement Variants()
        {
            var basic = new TextField { placeholder = "Enter your name" };
            basic.AddToClassList("demo-field");

            var search = new TextField { placeholder = "Search...", leadingIconName = "magnifying-glass" };
            search.AddToClassList("demo-field");

            var password = new TextField { placeholder = "Enter password", isPassword = true, leadingIconName = "gear" };
            password.AddToClassList("demo-field");

            return DemoUtils.Column(basic, search, password);
        }
    }
}
