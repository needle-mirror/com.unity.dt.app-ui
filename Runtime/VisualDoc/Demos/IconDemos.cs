using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Icon documentation page.</summary>
    static class IconDemos
    {
        [VisualDocDemo("icon-component")]
        static VisualElement Sizes()
        {
            return DemoUtils.Row(
                new Icon { iconName = "heart", size = IconSize.XS },
                new Icon { iconName = "heart", size = IconSize.S },
                new Icon { iconName = "heart", size = IconSize.M },
                new Icon { iconName = "heart", size = IconSize.L });
        }
    }
}
