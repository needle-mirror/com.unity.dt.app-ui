using UnityEngine.UIElements;
using Unity.AppUI.UI;
using Button = Unity.AppUI.UI.Button;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Drawer documentation page.</summary>
    static class DrawerDemos
    {
        [VisualDocDemo("drawer")]
        static VisualElement Permanent()
        {
            var drawer = new Drawer { anchor = DrawerAnchor.Left, variant = DrawerVariant.Permanent };
            drawer.Add(new Button { title = "Home", leadingIcon = "house", variant = ButtonVariant.Ghost });
            drawer.Add(new Button { title = "Settings", leadingIcon = "gear", variant = ButtonVariant.Ghost });

            var wrapper = DemoUtils.Row(drawer);
            wrapper.AddToClassList("demo-block");
            return wrapper;
        }
    }
}
