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
            drawer.Add(new Button { title = "Home", leadingIcon = "house", quiet = true });
            drawer.Add(new Button { title = "Settings", leadingIcon = "gear", quiet = true });

            var wrapper = DemoUtils.Row(drawer);
            wrapper.AddToClassList("demo-block");
            return wrapper;
        }
    }
}
