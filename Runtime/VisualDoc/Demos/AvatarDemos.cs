using UnityEngine;
using UnityEngine.UIElements;
using Unity.AppUI.UI;
using Avatar = Unity.AppUI.UI.Avatar;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Avatar documentation page.</summary>
    static class AvatarDemos
    {
        [VisualDocDemo("avatar")]
        static VisualElement Variants()
        {
            return DemoUtils.Row(
                new Avatar { variant = AvatarVariant.Circular, label = "MB", backgroundColor = new Color(0.30f, 0.44f, 0.93f), autoLabelColor = true },
                new Avatar { variant = AvatarVariant.Rounded, label = "JS", backgroundColor = new Color(0.30f, 0.44f, 0.93f), autoLabelColor = true },
                new Avatar { variant = AvatarVariant.Square, label = "AL", backgroundColor = new Color(0.30f, 0.44f, 0.93f), autoLabelColor = true });
        }

        [VisualDocDemo("avatar", order = 1)]
        static VisualElement Sizing()
        {
            return DemoUtils.Row(
                new Avatar { size = Size.S, label = "S", backgroundColor = new Color(0.30f, 0.44f, 0.93f), autoLabelColor = true },
                new Avatar { size = Size.M, label = "M", backgroundColor = new Color(0.30f, 0.44f, 0.93f), autoLabelColor = true },
                new Avatar { size = Size.L, label = "L", backgroundColor = new Color(0.30f, 0.44f, 0.93f), autoLabelColor = true },
                new Avatar { size = Size.M, backgroundColor = new Color(0.30f, 0.44f, 0.93f), outlineColor = Color.white, label = "MB", autoLabelColor = true });
        }
    }
}
