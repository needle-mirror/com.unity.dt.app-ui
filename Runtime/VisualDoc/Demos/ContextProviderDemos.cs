using UnityEngine.UIElements;
using Unity.AppUI.UI;
using Button = Unity.AppUI.UI.Button;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the ContextProvider documentation page.</summary>
    static class ContextProviderDemos
    {
        [VisualDocDemo("contextprovider")]
        static VisualElement Themes()
        {
            var dark = new ContextProvider { themeOverride = "dark" };
            dark.Add(new Button { title = "Dark section" });

            var light = new ContextProvider { themeOverride = "light" };
            light.Add(new Button { title = "Light section" });

            return DemoUtils.Row(dark, light);
        }
    }
}
