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
                new Icon { iconName = "heart", size = IconSize.Xs },
                new Icon { iconName = "heart", size = IconSize.Sm },
                new Icon { iconName = "heart", size = IconSize.Md },
                new Icon { iconName = "heart", size = IconSize.Lg });
        }
    }
}
