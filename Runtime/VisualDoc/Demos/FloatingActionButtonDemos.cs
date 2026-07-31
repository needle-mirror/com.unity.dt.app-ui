using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the FloatingActionButton documentation page.</summary>
    static class FloatingActionButtonDemos
    {
        [VisualDocDemo("floatingactionbutton")]
        static VisualElement Variants()
        {
            var defaultFab = new FloatingActionButton();
            defaultFab.Add(new Icon { iconName = "plus" });

            var accentFab = new FloatingActionButton { accent = true, size = Size.L };
            accentFab.Add(new Icon { iconName = "heart" });

            return DemoUtils.Row(defaultFab, accentFab);
        }
    }
}
