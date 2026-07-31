using UnityEngine;
using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Badge documentation page.</summary>
    static class BadgeDemos
    {
        [VisualDocDemo("badge")]
        static VisualElement Variants()
        {
            return DemoUtils.Row(
                new Badge { label = "5" },
                new Badge { variant = BadgeVariant.Dot },
                new Badge { label = "99+", backgroundColor = new Color(1f, 0.25f, 0.5f) });
        }
    }
}
